using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace LUIZ.AddressableSupport
{
    public class AddressableProviderGameObject : AddressableProviderBase<AddressableProviderGameObject, GameObject>
    {
        //-------------------------------------------------------------------
        protected override void OnProviderLoadStart()
        {
            // 이미 로드된 프리팹의 경우 로드 직후 1프레임 랜더링에 걸리는 것을 막을 수 없어서 임의좌표로 이동
            InstantiationParameters instatiationParam = new InstantiationParameters(new Vector3(0f, 10000f, 0), Quaternion.identity, null);
            m_asyncHandle = Addressables.InstantiateAsync(m_addressableAssetName, instatiationParam, true);
            m_asyncHandle.Completed += HandleLoadComplete;
        }

        //-------------------------------------------------------------------
        private void HandleLoadComplete(AsyncOperationHandle<GameObject> loadedObjectHandle)
        {
            if (loadedObjectHandle.IsValid() == false)
            {
                ProtProviderLoadError(loadedObjectHandle);
                return;
            }

            if (loadedObjectHandle.Status == AsyncOperationStatus.Failed)
            {
                ProtProviderLoadError(loadedObjectHandle);
            }
            else
            {
                loadedObjectHandle.Result.SetActive(false);

                ProviderLoadResult loadResult = new ProviderLoadResult(m_addressableAssetName, loadedObjectHandle);
                ProtProviderLoadFinish(ref loadResult);
            }
        }
    }
}
