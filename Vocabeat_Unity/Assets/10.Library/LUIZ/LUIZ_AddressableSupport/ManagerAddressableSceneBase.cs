using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using System.Collections;

namespace LUIZ.AddressableSupport
{
    public abstract class ManagerAddressableSceneBase : ManagerAddressableBase<AddressableProviderScene, SceneInstance>
    {
        protected class LoadedSceneData
        {
            public string AddressableSceneName;
            public SceneInstance SceneInstance;
            public Action<string, SceneInstance> EventFinishLoad;
            public Action<string> EventFinishUnload;
        }
        private string m_currentMainSceneName;      public string GetCurrentMainSceneName() { return m_currentMainSceneName; }

        private Dictionary<string, LoadedSceneData> m_dicLoadedScene = new Dictionary<string, LoadedSceneData>();
        private Queue<LoadedSceneData> m_queUnloadScene = new Queue<LoadedSceneData>();

        private Coroutine m_unloadCoroutine = null;
        //------------------------------------------------------------------------
        /// <summary>
        /// Main 씬으로 로드한다. 로드된 씬은 SceneManager.SetActiveScene()을 통해 ActiveScene로 설정된다.
        /// <para>m_CurrentMainSceneName 또한 해당 씬의 이름으로 변경된다. (GetCurrentMainSceneName() 로 확인 가능)</para>
        /// <para>isAutoActivate = true 일 경우 자동으로 SceneInstance.ActivateAsync()가 호출된다. false일 경우 수동으로 호출하여 씬 activate를 완료하여야한다.</para>
        /// <para>[주의!!]기존 Main씬을 자동으로 언로드 하지 않으므로 ProtUnloadScene()을 통해 기존 Main씬을 언로드 한 후 이용하도록 한다.</para>
        /// </summary>
        protected void ProtLoadSceneMain(string addressableSceneName, Action<string, float, DownloadStatus> delProgress, Action<string, SceneInstance> delFinish, bool isAutoActivate = true)
        {
            PrivSceneLoad(true, addressableSceneName, delProgress, delFinish, isAutoActivate);
        }

        /// <summary>
        /// Sub 씬을 로드한다. 로드된 씬은 MainScene이 아니다.
        /// <para>isAutoActivate = true 일 경우 자동으로 SceneInstance.ActivateAsync()가 호출된다. false일 경우 수동으로 호출하여 씬 activate를 완료하여야한다.</para>
        /// </summary>
        protected void ProtLoadSceneSub(string addressableSceneName, Action<string, float, DownloadStatus> delProgress, Action<string, SceneInstance> delFinish, bool isAutoActivate = true)
        {
            PrivSceneLoad(false, addressableSceneName, delProgress, delFinish, isAutoActivate);
        }

        /// <summary>
        /// 씬을 언로드한다.
        /// <para>일반적으론 master 씬을 메인으로 올려두고 sub 씬 전투, 홈 화면 등을 로드, 언로드 하면서 이용</para>
        /// <para>메인 씬을 언로드 할때는 다음 메인 씬을 세팅해줘야 하기 때문에 유의할 것 (일반적으로 언로드X)</para>
        /// <para>메인 씬 언로드시 (로딩 씬 Sub로 로드) => (현재 메인 씬 언로드, isUnloadUnused = true) => (다음 메인 씬 로드) 등의 과정을 거칠 것</para>
        /// <para>언로드는 m_queUnloadScene의 상황에 따라 즉시 이루어지지 않을 수도 있으므로 delFinish로 대기하는 등 사용에 주의가 필요 (메모리 피크에 유의) </para>
        /// </summary>
        protected void ProtUnloadScene(string addressableSceneName, Action<string> delFinish)
        {
            LoadedSceneData sceneData = null;

            if (m_dicLoadedScene.TryGetValue(addressableSceneName, out sceneData) == true)
            {
                sceneData.EventFinishUnload = delFinish;
                m_queUnloadScene.Enqueue(sceneData);
                m_dicLoadedScene.Remove(addressableSceneName);

                if (m_unloadCoroutine == null)
                    m_unloadCoroutine = StartCoroutine(PrivCOSceneUnload());
            }
            else
            {
                Debug.LogWarning($"[ManagerLoaderSceneBase] {addressableSceneName} is not Unloadable!!!");
            }
        }

        //------------------------------------------------------------------------
        private IEnumerator PrivCOSceneUnload()
        {
            while (true)
            {
                if (m_queUnloadScene.Count > 0)
                {
                    LoadedSceneData loadedSceneData = m_queUnloadScene.Dequeue();

                    AsyncOperationHandle<SceneInstance> unloadHandle = Addressables.UnloadSceneAsync(loadedSceneData.SceneInstance, true);
                    unloadHandle.Completed += (AsyncOperationHandle<SceneInstance> resultHandle) =>
                    {
                        loadedSceneData.EventFinishUnload?.Invoke(loadedSceneData.AddressableSceneName);
                    };
                    yield return unloadHandle;
                }
                else
                {
                    break;
                }
            }
            m_unloadCoroutine = null;
        }

        private void PrivSceneLoad(bool isMainScene, string addressableSceneName, Action<string, float, DownloadStatus> delProgress, Action<string, SceneInstance> delFinish, bool isAutoActivate)
        {
            LoadedSceneData sceneData = null;

            if (m_dicLoadedScene.TryGetValue(addressableSceneName, out sceneData) == true)
            {
                Debug.LogWarning($"[ManagerLoaderSceneBase] {addressableSceneName} is already Loaded!!!");
                delFinish?.Invoke(addressableSceneName, sceneData.SceneInstance);
            }
            else
            {
                sceneData = new LoadedSceneData();
                sceneData.AddressableSceneName = addressableSceneName;
                sceneData.EventFinishLoad = delFinish;
                m_dicLoadedScene[addressableSceneName] = sceneData;

                ProtRequestLoad(addressableSceneName, delProgress, (loadResult) =>
                {
                    PrivScneLoadFinish(isMainScene, addressableSceneName, loadResult, isAutoActivate);
                });
            }
        }

        private void PrivScneLoadFinish(bool isMainScene, string finishSceneName, AddressableProviderBase<AddressableProviderScene, SceneInstance>.ProviderLoadResult loadResult, bool isAutoActivate)
        {
            LoadedSceneData loadedSceneData = m_dicLoadedScene[finishSceneName];
            loadedSceneData.SceneInstance = loadResult.LoadedHandle.Result;

            if (isMainScene == true)//메인 씬일 경우
            {
                //SceneManager.SetActiveScene 수동 호출 (SceneLoadMode.Single 일 경우에는 자동 호출 됨)
                PrivSceneCheckAutoActive(isAutoActivate, loadedSceneData, () =>
                {
                    SceneManager.SetActiveScene(loadedSceneData.SceneInstance.Scene);
                    m_currentMainSceneName = loadedSceneData.AddressableSceneName;

                    loadedSceneData.EventFinishLoad?.Invoke(loadResult.AddressableName, loadResult.LoadedHandle.Result);
                });
            }
            else
            {
                PrivSceneCheckAutoActive(isAutoActivate, loadedSceneData, () =>
                {
                    loadedSceneData.EventFinishLoad?.Invoke(loadResult.AddressableName, loadResult.LoadedHandle.Result);
                });
            }
        }

        private void PrivSceneCheckAutoActive(bool isAutoActive, LoadedSceneData loadedSceneData, Action delFinish)
        {
            if (isAutoActive == true)
            {
                loadedSceneData.SceneInstance.ActivateAsync().completed += (AsyncOperation pSceneLoadWork) =>
                {
                    delFinish?.Invoke();
                };
            }
            else
            {
                delFinish?.Invoke();
            }
        }

        //------------------------------------------------------------------------
    }
}
