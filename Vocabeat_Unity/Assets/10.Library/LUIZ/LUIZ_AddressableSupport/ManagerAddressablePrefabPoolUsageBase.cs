using System;
using UnityEngine;

namespace LUIZ.AddressableSupport
{
    //poolCategoryName을 string으로 제어하면 위험할 수 있기 때문에 하위 자식 레이어에서 Enum등으로 래핑하여 관리해주는 것이 좋다
    public abstract class ManagerAddressablePrefabPoolUsageBase : ManagerAddressablePrefabPoolBase
	{
        // [주의!!] 로드된 object 들은 사용 종료 시 반드시 release 되어야한다.
        // 해당 에셋을 참조하는 모든 레퍼를 제거 해야 한다.
        public void DoReturnClone(GameObject releaseObject, bool isReleaseOrigin = false)
		{
			ProtReturnClone(releaseObject, isReleaseOrigin);
		}

		public void DoClearAll(bool isReleaseOrigin)
		{
			if (isReleaseOrigin)
			{
				ProtRemoveInstanceAll();
			}
			else
			{
				ProtRemoveInstanceAllSoft();
			}
		}

		//-----------------------------------------------------------------
		protected void ProtLoadGameObject(string poolCategoryName, string addressableName, Action<GameObject> delFinish, int reserveCount, bool isAutoRelease)
		{
			ProtPoolReserveInstance(poolCategoryName.ToString(), addressableName, reserveCount, (string strLoadedAddressable, GameObject pLoadedObject) =>
			{
				delFinish?.Invoke(pLoadedObject);
			}, isAutoRelease);
		}

		protected void ProtLoadComponent<T>(string poolCategoryName, string addressableName, Action<T> delFinish, int reserveCount, bool isAutoRelease) where T : Component
		{
			ProtPoolReserveInstance(poolCategoryName, addressableName, reserveCount, (string strLoadedAddressable, GameObject pLoadedObject) =>
			{
				T component = pLoadedObject.GetComponent<T>();
				delFinish?.Invoke(component);
			}, isAutoRelease);
		}

        /// <summary>
        /// 별도의 Clone을 생성하지 않고 메모리에만 올려두는 용도
        /// </summary>
        protected void ProtLoadGameObjectNoClone(string poolCategoryName, string addressableName, Action delFinish)
		{
			ProtPoolReserveInstance(poolCategoryName, addressableName, 0, (string loadedAddressableName, GameObject loadedObject) =>
			{
				delFinish?.Invoke();
			}, false);
		}

        //-----------------------------------------------------------------
        /// <summary>
        /// 해당 어드레서블 에셋이 Reserve가 되어 있지 않으면 null을 반환 TODO: 지금 null 대신 새로 clone 추가해줄텐데 수정필요
        /// </summary>
        protected GameObject ProtFindClone(string poolCategoryName, string addressableName)
		{
			GameObject cloneInstance = ProtRequestClone(poolCategoryName, addressableName);
			if (cloneInstance == null)
			{
				Debug.LogWarning($"[ManagerAddressablePrefabPool] There is no reserved instance : {addressableName}");
			}
			return cloneInstance;
		}

		//-----------------------------------------------------------------
		protected void ProtClearCategory(string poolCategoryName, bool isReleaseOrigin = false)
		{
			ProtRemoveCategory(poolCategoryName, isReleaseOrigin);
		}
	}
}
