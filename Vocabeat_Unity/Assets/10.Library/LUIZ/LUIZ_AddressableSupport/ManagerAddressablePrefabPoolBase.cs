using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LUIZ.AddressableSupport
{
	public abstract class ManagerAddressablePrefabPoolBase : ManagerAddressableBase<AddressableProviderGameObject, GameObject>
	{
		private class PrefabPool
		{
			public string Category;
			public string AddressableName;
			public AsyncOperationHandle OriginHandle;
			public GameObject OriginInstance;
			public GameObject RootParent;
			public bool AutoRelease = false;											// 모든 클론이 회수되면 자동으로 비디오 메모리를 해재한다.
            public readonly Queue<GameObject> CloneStock = new Queue<GameObject>();		// 생성은 되었으나 현재 사용되고 있지 않는 객체들. 사용자에게 대여하거나 회수된다.
            public readonly List<GameObject> CloneList = new List<GameObject>();		// 생성된 모든 게임 오브젝트를 참조. 비디오 메모리 내릴때를 위한 참고용 항목
        }

		private readonly Dictionary<string, Dictionary<string, PrefabPool>> m_dicPrefabPoolCategory = new ();
		private readonly Dictionary<GameObject, PrefabPool> m_dicPrefabPoolMap = new (); // 검색 비용 절감을 위한 메모리 사용
        private readonly Dictionary<string, GameObject> m_dicPrefabParent = new ();

		private bool m_isManagerPrefabPoolInit = false;

		//----------------------------------------------------------------------
		public bool IsPrefabLoaded(string addressableName)
		{
			bool isLoaded = false;

			PrefabPool prefabPool = PrivFindPrefabPool(addressableName);
			if (prefabPool != null)
			{
				isLoaded = true;
			}

			return isLoaded;
		}

        //----------------------------------------------------------------------
        /// <summary>
        /// autoRelease 옵션의 경우 모든 클론이 반환될 경우 자동으로 메모리를 해재한다. 다음 호출때는 에셋번들에서 읽어오게 되므로 주의할것.
        /// </summary>
        protected void ProtPoolReserveInstance(string categoryName, string addressableName, int reserveCount, Action<string, GameObject> delFinish, bool isAutoRelease)
		{
			Dictionary<string, PrefabPool> dicPoolCategory = PrivFindPrefabPoolCategoryOrAlloc(categoryName);

			if (reserveCount <= 0)
			{
				isAutoRelease = false;  // reserveCount == 0 일경우 무조건 자율해제가 되므로 강제로 꺼준다.
            }

			if (dicPoolCategory.ContainsKey(addressableName))
			{
				GameObject cloneInstance = null;
				for (int i = 0; i < reserveCount; i++)
				{
					cloneInstance = ProtRequestClone(categoryName, addressableName);
				}
				delFinish?.Invoke(addressableName, cloneInstance);
			}
			else
			{
				ProtRequestLoad(addressableName, null, (loadResult) =>
				{
					PrivReserveInternal(categoryName, dicPoolCategory, loadResult.AddressableName, loadResult.LoadedHandle, reserveCount, isAutoRelease);

					if (reserveCount > 0)
					{
						GameObject cloneInstance = ProtRequestClone(categoryName, addressableName);
						delFinish?.Invoke(loadResult.AddressableName, cloneInstance);
					}
					else
					{
						delFinish?.Invoke(loadResult.AddressableName, null);
					}
				});
			}

			OnPrefabRequestOrigin(addressableName);
		}

		protected GameObject ProtRequestClone(string categoryName, string addressableName)
		{
			GameObject cloneInstance = null;
			Dictionary<string, PrefabPool> dicPoolCategory = PrivFindPrefabPoolCategoryOrAlloc(categoryName);

			if (dicPoolCategory.ContainsKey(addressableName))
			{
				PrefabPool PrefabPool = dicPoolCategory[addressableName];
				if (PrefabPool.CloneStock.Count == 0)
				{
					PrivAllocCloneInstance(PrefabPool, 1);
				}
				cloneInstance = PrefabPool.CloneStock.Dequeue();
			}

			return cloneInstance;
		}

		protected void ProtReturnClone(GameObject returnObject, bool isReleaseOrigin)
		{
			PrefabPool prefabPool = PrivFindPrefabPool(returnObject);
			if (prefabPool != null)
			{
				returnObject.transform.SetParent(prefabPool.RootParent.transform, false);
				prefabPool.RootParent.transform.position = Vector3.zero;
				returnObject.SetActive(false);
				prefabPool.CloneStock.Enqueue(returnObject);

				if (isReleaseOrigin || prefabPool.AutoRelease)
				{
					if (prefabPool.CloneList.Count == prefabPool.CloneStock.Count) // 외부로 나간 클론이 모두 돌아오면 메모리를 해제
                    {
						PrivRemoveInstance(prefabPool.Category, prefabPool.AddressableName, isReleaseOrigin);
					}
				}
			}
		}

		protected void ProtRemoveInstanceAll() // 모든 에셋, 메모리를 클리어. 비용이 크므로 주의
        {
			Dictionary<string, Dictionary<string, PrefabPool>>.Enumerator it = m_dicPrefabPoolCategory.GetEnumerator();

			while (it.MoveNext())
			{
				PrivRemoveCategoryInternal(it.Current.Value, true);
			}

			m_dicPrefabPoolMap.Clear();
			Resources.UnloadUnusedAssets();//번들의 refcount가 0이라고 바로 비디오 메모리에서 내려가지 않는다. UnloadUnusedAssets를 호출해야함;;
        }

		protected void ProtRemoveInstanceAllSoft()  // 클론만 해재. 번들은 유지
        {
			Dictionary<string, Dictionary<string, PrefabPool>>.Enumerator it = m_dicPrefabPoolCategory.GetEnumerator();

			while (it.MoveNext())
			{
				PrivRemoveCategoryInternal(it.Current.Value, false);
			}
		}

		protected void ProtRemoveCategory(string categoryName, bool isReleaseOrigin)
		{
			Dictionary<string, PrefabPool> dicPoolCategory = PrivFindPrefabPoolCategoryOrAlloc(categoryName);

			PrivRemoveCategoryInternal(dicPoolCategory, isReleaseOrigin);
		}

		//----------------------------------------------------------------------
		private void PrivReserveInternal(string categoryName, Dictionary<string, PrefabPool> dicPrefabPool, string addressableName, AsyncOperationHandle loadedHandle, int reserveCount, bool isAutoRelease)
		{
			if (loadedHandle.Result == null) return;

			PrefabPool prefabPool = new PrefabPool();

			prefabPool.Category = categoryName;
			prefabPool.AutoRelease = isAutoRelease;
			prefabPool.OriginHandle = loadedHandle;
			prefabPool.OriginInstance = loadedHandle.Result as GameObject;
			prefabPool.RootParent = PrivFindParentOrAlloc(categoryName);
			prefabPool.OriginInstance.transform.SetParent(prefabPool.RootParent.transform);
			prefabPool.AddressableName = addressableName;

			RemoveCloneObjectName(prefabPool.OriginInstance);

			dicPrefabPool[addressableName] = prefabPool;
			PrivAllocCloneInstance(prefabPool, reserveCount);

			OnPrefabOriginLoaded(addressableName, loadedHandle);
		}

		private GameObject PrivFindParentOrAlloc(string categoryName)
		{
			GameObject parentCategoryObj = null;
			if (!m_dicPrefabParent.TryGetValue(categoryName, out parentCategoryObj))
			{
				parentCategoryObj = PrivMakePrefabParent(categoryName);
			}

			return parentCategoryObj;
		}

		private GameObject PrivMakePrefabParent(string categoryName)
		{
			GameObject parentObject = new GameObject();
			parentObject.name = categoryName;
			parentObject.transform.SetParent(this.transform);

			m_dicPrefabParent.Add(categoryName, parentObject);

			return parentObject;
		}

		//----------------------------------------------------------------------
		private void PrivRemoveInstance(string categoryName, string addressableName, bool isReleaseOrigin)
		{
			Dictionary<string, PrefabPool> dicPoolCategory = PrivFindPrefabPoolCategoryOrAlloc(categoryName);
			if (dicPoolCategory.ContainsKey(addressableName))
			{
				PrivRemoveInternal(dicPoolCategory[addressableName], isReleaseOrigin);
				dicPoolCategory.Remove(addressableName);
			}
		}

		private void PrivRemoveInternal(PrefabPool removePool, bool isReleaseOrigin)
		{
			removePool.CloneStock.Clear();

			for (int i = 0; i < removePool.CloneList.Count; i++)
			{
				GameObject gameObjectInstance = removePool.CloneList[i];
				OnPrefabCloneRemove(removePool.AddressableName, gameObjectInstance);
				m_dicPrefabPoolMap.Remove(gameObjectInstance);
				Destroy(gameObjectInstance);
			}
			removePool.CloneList.Clear();

			if (isReleaseOrigin)
			{
				OnPrefabOriginRemove(removePool.AutoRelease, removePool.AddressableName, removePool.OriginHandle);
				Addressables.ReleaseInstance(removePool.OriginInstance);
				removePool.OriginInstance = null;
			}
		}

		private void PrivRemoveCategoryInternal(Dictionary<string, PrefabPool> categoryPool, bool isReleaseOrigin)
		{
			Dictionary<string, PrefabPool>.Enumerator it = categoryPool.GetEnumerator();
			while (it.MoveNext())
			{
				PrivRemoveInternal(it.Current.Value, isReleaseOrigin);
			}
			categoryPool.Clear();
		}

		private void PrivAllocCloneInstance(PrefabPool prefabPool, int cloneAllocCount)
		{
			for (int i = 0; i < cloneAllocCount; i++)
			{
				GameObject cloneInstance = Instantiate(prefabPool.OriginInstance);
				AddCloneInstance(prefabPool, cloneInstance);
			}
		}

		private void AddCloneInstance(PrefabPool prefabPool, GameObject cloneInstance)
		{
			cloneInstance.transform.SetParent(prefabPool.RootParent.transform);
			cloneInstance.transform.position = Vector3.zero;
			cloneInstance.transform.rotation = Quaternion.identity;
			cloneInstance.transform.localPosition = Vector3.zero;
			cloneInstance.transform.localRotation = Quaternion.identity;
			cloneInstance.SetActive(false);

			RemoveCloneObjectName(cloneInstance);

			prefabPool.CloneList.Add(cloneInstance);
			prefabPool.CloneStock.Enqueue(cloneInstance);

			m_dicPrefabPoolMap.Add(cloneInstance, prefabPool);

			OnPrefabCloneInstance(prefabPool.AddressableName, cloneInstance);
		}

		private Dictionary<string, PrefabPool> PrivFindPrefabPoolCategoryOrAlloc(string categoryName)
		{
			Dictionary<string, PrefabPool> dicPoolCategory = null;
			if (!m_dicPrefabPoolCategory.TryGetValue(categoryName, out dicPoolCategory))
			{
				dicPoolCategory = new Dictionary<string, PrefabPool>();
				m_dicPrefabPoolCategory.Add(categoryName, dicPoolCategory);
			}

			return dicPoolCategory;
		}

		private PrefabPool PrivFindPrefabPool(GameObject cloneInstance)
		{
			m_dicPrefabPoolMap.TryGetValue(cloneInstance, out var prefabPool);
			return prefabPool;
		}

		private PrefabPool PrivFindPrefabPool(string prefabPoolName)
		{
			PrefabPool prefabPool = null;

			foreach (Dictionary<string, PrefabPool> dicPrefabPool in m_dicPrefabPoolCategory.Values)
			{
				if (dicPrefabPool.TryGetValue(prefabPoolName, out prefabPool))
					break;
			}

			return prefabPool;
		}

		//----------------------------------------------------------------------
		protected virtual void OnPrefabOriginRemove(bool isAutoRelase, string addressableName, AsyncOperationHandle removedHandle) { }
		protected virtual void OnPrefabCloneRemove(string addressableName, GameObject removeClone) { }
		protected virtual void OnPrefabRequestOrigin(string addressableName) { }
		protected virtual void OnPrefabOriginLoaded(string addressableName, AsyncOperationHandle loadedHandle) { }
		protected virtual void OnPrefabCloneInstance(string addressableName, GameObject cloneInstance) { }
	}
}
