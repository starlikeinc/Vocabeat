using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LUIZ
{
    public class SceneAttacherBase : MonoBase
    {
        //----------------------------------------------------------
        protected void ProtLoadResourcePrefab(string prefabDirectory, string prefabName, Action delFinish)
        {
            GameObject prefabCurrent = GameObject.Find(prefabName);

            if (prefabCurrent == null)
            {
                prefabDirectory = $"{prefabDirectory}{Path.DirectorySeparatorChar}{prefabName}";
                GameObject prefabLoad = Instantiate(Resources.Load(prefabDirectory), Vector3.zero, Quaternion.identity) as GameObject;
                if (prefabLoad == null)
                {
                    Debug.LogError("[LoadResourcePrefab]");
                }
                else
                {
                    RemoveCloneObjectName(prefabLoad);
                }
            }

            delFinish?.Invoke();
        }

        protected void ProtLoadAddressablePrefab(string prefabName, Action<bool, GameObject> delFinish)
        {
            GameObject prefabCurrent = GameObject.Find(prefabName);
            if (prefabCurrent == null)
            {
                AsyncOperationHandle<GameObject> loadHandle = Addressables.InstantiateAsync(prefabName);
                loadHandle.Completed += (AsyncOperationHandle<GameObject> resultHandle) =>
                {
                    if (resultHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        RemoveCloneObjectName(resultHandle.Result);
                        resultHandle.Result.SetActive(true);
                        delFinish?.Invoke(true, resultHandle.Result.gameObject);
                    }
                    else
                    {
                        delFinish?.Invoke(false, null);
                    }
                };
            }
            else
            {
                delFinish?.Invoke(true, prefabCurrent);
            }
        }
        //----------------------------------------------------------
    }
}
