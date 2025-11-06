using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System;
using LUIZ.Patcher;

namespace LUIZ.AddressableSupport
{
    internal class PatcherAddressable : PatcherBase
    {
        private AsyncOperationHandle m_DownloadProgress;

        private bool    m_isPatchStart = false;
        private bool    m_isInitialized = false;

        private int     m_currentLabelWorkIndex = 0;
        private long    m_currentDownloadSize = 0;
        private int     m_currentAssetBundleCount = 0;

        private long    m_totalDownloadSize = 0;
        private int     m_totalAssetBundleCount = 0;

        private List<string>    m_listPatchLabel = null;
        private StringBuilder   m_stringBuilder  = new StringBuilder();

        //----------------------------------------------------------------------
        protected override void OnPatcherInitialize(string strDownloadURL, string strDownloadSavePath)
        {
            Addressables.InitializeAsync().Completed += (AsyncOperationHandle<IResourceLocator> result) => 
            {
                m_isInitialized = true;
                ProtPatchInitComplete();
            };
        }

        protected override void OnPatcherUpdateEvent(float DeltaTime)
        {
            //실제 다운로드를 위한 Update가 아닌 이벤트 호출을 위한 업데이트.
            if (m_isPatchStart)
            {
                if (m_DownloadProgress.IsValid() && m_DownloadProgress.IsDone == false && m_DownloadProgress.Status != AsyncOperationStatus.Failed)
                {
                    // 다운로드가 진행중일때는 다운로드를 출력한다. 다운로드가 없을 경우(패치 없음)는 0이 리턴되며 대신 loadCurrent가 증가한다.
                    DownloadStatus status = m_DownloadProgress.GetDownloadStatus();

                    // 에셋번들 로드 카운트
                    int loadCurrent = PrivExtractAssetBundleCurrent(m_DownloadProgress) + m_currentAssetBundleCount;
                    long currentDownload = status.DownloadedBytes + m_currentDownloadSize;
                    float percent = m_totalDownloadSize == 0 ? status.Percent : (float)((double)currentDownload / m_totalDownloadSize);

                    ProtPatchProgress(PrivExtractAssetBundleLoadName(m_DownloadProgress), currentDownload, m_totalDownloadSize, percent, loadCurrent, m_totalAssetBundleCount);

                }
            }
        }

        //----------------------------------------------------------------------
        public void DoPatcherAddressableStart(List<string> _listAssetBundleLableName)
        {
            if (m_isInitialized == false)
            {
                ProtPatchError(EPatchErrorType.NotInitialized);
                return;
            }

            if (m_isPatchStart)
            {
                PrivPatchError(EPatchErrorType.AlreadyPatchProcess, null);
                return;
            }

            m_currentDownloadSize = 0;
            m_isPatchStart = true;

            PrivPatchStart(_listAssetBundleLableName);
        }

        public void DoPatcherAddressableDowloadSize(List<string> listAssetBundleLabelName, Action<long> delFinish)
        {
            if (m_isInitialized == false)
            {
                ProtPatchError(EPatchErrorType.NotInitialized);
                return;
            }

            if (m_isPatchStart)
            {
                PrivPatchError(EPatchErrorType.AlreadyPatchProcess, null);
                return;
            }

            PrivPatchDownloadSize(listAssetBundleLabelName, delFinish);
        }

        public void DoPatcherAddressableCleanExpiredBundles(Action delFinish)
        {
            PrivCleanBundleCacheForAllCatalogs(delFinish);
        }

        //----------------------------------------------------------------------
        private void PrivPatchStart(List<string> listAssetBundleLabelName)
        {
            m_currentDownloadSize = 0;
            m_currentLabelWorkIndex = 0;
            m_listPatchLabel = listAssetBundleLabelName;

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                PrivPatchError(EPatchErrorType.NetworkDisable, null);
            }
            else
            {
                PrivPatchDownloadNextWork(m_currentLabelWorkIndex);
            }
        }

        private void PrivPatchDownloadNextWork(int workIndex)
        {
            if (workIndex < m_listPatchLabel.Count)
            {
                m_currentLabelWorkIndex = workIndex;

                string labelName = PrivGetCurrentLabelName(m_currentLabelWorkIndex);

                Debug.LogFormat("<color=red>PrivPatchDownloadNextWork LabelName {0} </color>", labelName);

                ProtPatchLabelStart(labelName);

                m_DownloadProgress = Addressables.DownloadDependenciesAsync(labelName);

                m_DownloadProgress.Completed += HandlePatchEnd;
            }
            else
            {
                m_isPatchStart = false;
                ProtPatchFinish();
            }
        }

        private void PrivPatchDownloadSize(List<string> listBundleLabelNames, Action<long> delFinish)
        {
            int count = 0;
            m_totalDownloadSize = 0;

            for (int i = 0; i < listBundleLabelNames.Count; i++)
            {
                Addressables.GetDownloadSizeAsync(listBundleLabelNames[i]).Completed += (downloadSizeHandle) => {

                    if (downloadSizeHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        m_totalDownloadSize += downloadSizeHandle.Result;
                        downloadSizeHandle.Release();
                    }
                    else
                    {
                        Debug.LogWarning("[Addressable] Label does not exist : ");
                    }

                    count++;

                    if (count >= listBundleLabelNames.Count)
                    {
                        if (Caching.defaultCache.spaceFree < m_totalDownloadSize)
                        {
                            PrivPatchError(EPatchErrorType.NotEnoughDiskSpace, $"downloadSize = {m_totalDownloadSize} / cache free = {Caching.defaultCache.spaceFree}");
                        }
                        else
                        {
                            delFinish?.Invoke(m_totalDownloadSize);
                        }
                    }
                };
            }
        }

        private void PrivCleanBundleCacheForAllCatalogs(Action delFinish)
        {
            //Ä«Å»·Î±× º°·Î ¸¸·áµÈ Ä³½Ã Á¦°Å Ãß°¡ ÇØ¾ßÇÔ CleanBundleCache °ü·Ã °ø½Ä ¹®¼­ Âü°í ÇÒ °Í
            AsyncOperationHandle<bool> cleanBundleCacheHandle = Addressables.CleanBundleCache();
            cleanBundleCacheHandle.Completed += (AsyncOperationHandle<bool> handle) =>
            {
                delFinish?.Invoke();
                handle.Release();
            };
        }

        //----------------------------------------------------------------------
        private string PrivGetCurrentLabelName(int labelIndex)
        {
            string result = "None";

            if (labelIndex < m_listPatchLabel.Count)
            {
                result = m_listPatchLabel[labelIndex];
            }

            return result;
        }

        private int ExtractAssetBundleTotal(AsyncOperationHandle handle)
        {
            int count = 0;
            List<AsyncOperationHandle> listDependencies = PrivExtractAssetBundleList(handle);
            count = listDependencies.Count;
            return count;
        }

        private int PrivExtractAssetBundleCurrent(AsyncOperationHandle handle)
        {
            int count = 0;
            List<AsyncOperationHandle> listDependencies = PrivExtractAssetBundleList(handle);
            for (int i = 0; i < listDependencies.Count; i++)
            {
                //·Îµå°¡ ¿Ï·áµÇ¾úÀ» °æ¿ì succeded
                if (listDependencies[i].Status == AsyncOperationStatus.Succeeded)
                {
                    count++;
                }
            }
            return count;
        }

        private List<AsyncOperationHandle> PrivExtractAssetBundleList(AsyncOperationHandle handle)
        {
            List<AsyncOperationHandle> listDependencies = new List<AsyncOperationHandle>();
            handle.GetDependencies(listDependencies);

            if (listDependencies.Count > 0)
            {
                AsyncOperationHandle defHandle = listDependencies[0];
                listDependencies.Clear();
                defHandle.GetDependencies(listDependencies);
            }

            return listDependencies;
        }

        private string PrivExtractAssetBundleLoadName(AsyncOperationHandle handle)
        {
            string assetBundleName = "";
            List<AsyncOperationHandle> listDependencies = PrivExtractAssetBundleList(handle);
            for (int i = 0; i < listDependencies.Count; i++)
            {
                if (listDependencies[i].IsDone == false)
                {
                    assetBundleName = PrivConvertStringToBundleName(listDependencies[i].DebugName);
                    break;
                }
            }
            return assetBundleName;
        }

        private string PrivConvertStringToBundleName(string _debugName)
        {
            string assetBundleName = string.Empty;
            m_stringBuilder.Clear();
            bool bracket = false;
            for (int i = 0; i < _debugName.Length; i++)
            {
                if (bracket == false)
                {
                    if (_debugName[i] == '(')
                    {
                        bracket = true;
                    }
                }
                else
                {
                    if (_debugName[i] == ')')
                    {
                        assetBundleName = m_stringBuilder.ToString();
                        break;
                    }
                    else
                    {
                        m_stringBuilder.Append(_debugName[i]);
                    }
                }
            }

            return assetBundleName;
        }

        private void PrivPatchError(EPatchErrorType _errorType, string _message = null)
        {
            m_isPatchStart = false;
            ProtPatchError(_errorType, _message);
        }

        //----------------------------------------------------------------------
        private void HandlePatchEnd(AsyncOperationHandle AsyncHandle)
        {
            if (AsyncHandle.Status == AsyncOperationStatus.Succeeded)
            {
                DownloadStatus downloadSize = AsyncHandle.GetDownloadStatus();
                m_currentDownloadSize += downloadSize.TotalBytes;
                m_currentAssetBundleCount += PrivExtractAssetBundleCurrent(AsyncHandle);
                AsyncHandle.Release();
                PrivPatchDownloadNextWork(m_currentLabelWorkIndex + 1);
            }
            else
            {
                PrivPatchError(EPatchErrorType.PatchFail);
            }
        }
    }
}
