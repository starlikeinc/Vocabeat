using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LUIZ.AddressableSupport
{
    public abstract class AddressableProviderBase<TProvider, TType> : InstancePoolBase<TProvider> where TProvider : AddressableProviderBase<TProvider, TType>, new()
    {
        protected AsyncOperationHandle<TType> m_asyncHandle;
        protected string m_addressableAssetName;

        private bool m_isUpdate = false;

        //일반적으로 번들을 Patcher에서 미리 다운 받은 상태이기때문에 DownloadStatus는 0일 경우가 많음, 
        //2번째 변수인 float PercentComplete 값을 주로 이용
        private event Action<string, float, DownloadStatus> m_delLoadProgress   = null;
        private event Action<ProviderLoadResult>            m_delLoadFinish     = null;
        private event Action<TProvider, string, string>     m_delLoadError      = null;

        //-------------------------------------------------------------------
        protected sealed override void OnInstancePoolCreation() { }
        protected sealed override void OnInstancePoolGet() { }
        protected sealed override void OnInstancePoolReturn()
        {
            m_delLoadFinish = null;
            m_delLoadProgress = null;
            m_delLoadError = null;
            m_addressableAssetName = null;
            m_isUpdate = false;
        }

        //-------------------------------------------------------------------
        public void UpdateLoadWork()
        {
            if (m_isUpdate == false)
                return;

            if (m_asyncHandle.IsValid() == false)
                return;

            if (m_asyncHandle.Status == AsyncOperationStatus.Failed)
            {
                ProtProviderLoadError(m_asyncHandle);
                return;
            }
            else
            {
                ProtProviderLoadProgress(m_asyncHandle.PercentComplete, m_asyncHandle.GetDownloadStatus());
            }
        }

        //-------------------------------------------------------------------
        public string GetProviderName()
        {
            return m_addressableAssetName;
        }

        public void SetProviderLoadPrepare(string assetName, Action<string, float, DownloadStatus> delProgress, Action<ProviderLoadResult> delFinish, Action<TProvider, string, string> delError)
        {
            m_addressableAssetName = assetName;
            m_delLoadProgress = delProgress;
            m_delLoadFinish = delFinish;
            m_delLoadError = delError;
        }

        public void DoProviderLoadStart()
        {
            m_isUpdate = true;
            OnProviderLoadStart();
        }

        //-------------------------------------------------------------------
        protected void ProtProviderLoadProgress(float percentComplete, DownloadStatus downloadStatus)
        {
            m_delLoadProgress?.Invoke(m_addressableAssetName, percentComplete, downloadStatus);
        }

        protected void ProtProviderLoadError(AsyncOperationHandle errorHandle)
        {
            m_isUpdate = false;
            m_delLoadError?.Invoke(this as TProvider, m_addressableAssetName, errorHandle.OperationException.ToString());
            Addressables.Release(errorHandle);
        }

        protected void ProtProviderLoadFinish(ref ProviderLoadResult loadResult)
        {
            m_isUpdate = false;
            m_delLoadFinish?.Invoke(loadResult);
        }

        //-------------------------------------------------------------------
        protected abstract void OnProviderLoadStart();

        //-------------------------------------------------------------------
        public struct ProviderLoadResult
        {
            public string AddressableName;
            public AsyncOperationHandle<TType> LoadedHandle;

            public ProviderLoadResult(string addressableName, AsyncOperationHandle<TType> loadedHandle)
            {
                AddressableName = addressableName;
                LoadedHandle = loadedHandle;
            }
        }

    }
}
