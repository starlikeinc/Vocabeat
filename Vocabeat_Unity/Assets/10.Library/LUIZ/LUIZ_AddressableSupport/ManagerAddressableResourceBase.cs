using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LUIZ.AddressableSupport
{
    public abstract class ManagerAddressableResourceBase : ManagerAddressableBase<AddressableProviderObject, object>
    {
        private readonly Dictionary<object, AsyncOperationHandle> m_dicLoadedHandle = new();

        //------------------------------------------------------------
        protected override void OnAddressableLoadError(string addressableName, string error)
        {
            Debug.LogError(string.Format("[Addressable] {0} Error : {1}", addressableName, error));
        }

        //------------------------------------------------------------
        public void DoReleaseAll()
        {
            foreach (AsyncOperationHandle handle in m_dicLoadedHandle.Values)
            {
                Addressables.Release(handle);
            }
            m_dicLoadedHandle.Clear();
        }

        // [주의!!] 로드된 object 들은 사용 종료 시 반드시 release 되어야한다.
        // 해당 에셋을 참조하는 모든 레퍼를 제거 해야 한다.
        public void DoReleaseAsset(UnityEngine.Object asset)
        {
            if (m_dicLoadedHandle.Remove(asset, out AsyncOperationHandle handle) == true)
            {
                Addressables.Release(handle);
            }
            else
            {
                Debug.LogError($"[ManagerAddressableResource] {asset.name} is not a releasable Object");
            }
        }

        //------------------------------------------------------------
        protected void ProtLoadResource<TResource>(string addressableName, Action<TResource> delFinish) where TResource : UnityEngine.Object
        {
            if (delFinish == null)
                return;

            ProtRequestLoad(addressableName, null, (AddressableProviderBase<AddressableProviderObject, object>.ProviderLoadResult loadResult) =>
            {
                object result = loadResult.LoadedHandle.Result;

                if (loadResult.LoadedHandle.Result != null && result is TResource loadedResource)
                {
                    if (m_dicLoadedHandle.TryAdd(result, loadResult.LoadedHandle) == false)
                    {
                        Debug.LogError($"[ManagerAddressableResourceBase] {addressableName} is already loaded");
                    }

                    delFinish?.Invoke(loadedResource);
                }
                else
                {
                    //받아온 리소스가 문제가 있다면 해제
                    Addressables.Release(loadResult.LoadedHandle);
                    delFinish?.Invoke(null);
                }
            });
        }
    }
}
