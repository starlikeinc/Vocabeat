using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace LUIZ.AddressableSupport
{
    public class AddressableProviderScene : AddressableProviderBase<AddressableProviderScene, SceneInstance>
    {
        //-------------------------------------------------------------------
        protected override void OnProviderLoadStart()
        {
            m_asyncHandle = Addressables.LoadSceneAsync(m_addressableAssetName, UnityEngine.SceneManagement.LoadSceneMode.Additive, false);
            m_asyncHandle.Completed += HandleLoadSceneComplete;
        }

        //-------------------------------------------------------------------------
        private void HandleLoadSceneComplete(AsyncOperationHandle<SceneInstance> loadedSceneHandle)
        {
            // [주의!] 씬 인스턴스는 로드 되었으나 씬 내부의 게임 오브젝트가 Awake되었는지는 불분명
            //         Finish 이벤트 발생시 Awake가 모두 호출되지 않을 수도 있음
            if (loadedSceneHandle.IsValid() == false)
            {
                ProtProviderLoadError(loadedSceneHandle);
                return;
            }

            if (loadedSceneHandle.Status == AsyncOperationStatus.Failed)
            {
                ProtProviderLoadError(loadedSceneHandle);
            }
            else
            {
                ProviderLoadResult loadResult = new ProviderLoadResult(m_addressableAssetName, loadedSceneHandle);
                ProtProviderLoadFinish(ref loadResult);
            }
        }
    }
}
