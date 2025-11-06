using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LUIZ.AddressableSupport
{
    public class AddressableProviderObject : AddressableProviderBase<AddressableProviderObject, object>
    {
        //-------------------------------------------------------------------
        protected override void OnProviderLoadStart()
        {
            m_asyncHandle = Addressables.LoadAssetAsync<object>(m_addressableAssetName);
            m_asyncHandle.Completed += HandleLoadComplete;
        }

        //-------------------------------------------------------------------
        private void HandleLoadComplete(AsyncOperationHandle<object> loadedObjectHandle)
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
                ProviderLoadResult loadResult = new ProviderLoadResult(m_addressableAssetName, loadedObjectHandle);
                ProtProviderLoadFinish(ref loadResult);
            }
        }
    }
}
