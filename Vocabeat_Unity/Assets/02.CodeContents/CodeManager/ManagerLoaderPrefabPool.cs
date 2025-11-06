using System;
using System.Collections.Generic;
using UnityEngine;
using LUIZ.AddressableSupport;

public class ManagerLoaderPrefabPool : ManagerAddressablePrefabPoolUsageBase
{
    public new static ManagerLoaderPrefabPool Instance => ManagerAddressablePrefabPoolUsageBase.Instance as ManagerLoaderPrefabPool;

    //------------------------------------------------------------
    //프로젝트에 따라 해당 enum을 변경하여 사용
    public enum EPoolType
    {
        UI,

		StageProp,
        Character,

        Effect,

        AlwaysLoad,   //기타 자원
    }

	//------------------------------------------------------------
	/// <summary>
	/// 사용 후 반드시 ReturnClone으로 반환할 것
	/// </summary>
	public void DoLoadGameObject(EPoolType poolType, string addressableName, Action<GameObject> delFinish, int reserveCount = 1, bool isAutoRelease = false)
	{
		ProtLoadGameObject(poolType.ToString(), addressableName, (GameObject pLoadedObject) =>
		{
			delFinish?.Invoke(pLoadedObject);
		}, reserveCount, isAutoRelease);
	}

	/// <summary>
	/// 사용 후 반드시 ReturnClone으로 반환할 것
	/// </summary>
	public void DoLoadComponent<T>(EPoolType poolType, string addressableName, Action<T> delFinish, int reserveCount = 1, bool isAutoRelease = false) where T : Component
	{
		ProtLoadComponent<T>(poolType.ToString(), addressableName, (T loadedComponent) =>
		{
			delFinish?.Invoke(loadedComponent);
		}, reserveCount, isAutoRelease);
	}

	/// <summary>
	/// 별도의 Clone을 생성하지 않고 메모리에만 올려두는 용도 , 사용 후 반드시 ReturnClone으로 반환할 것
	/// </summary>
	public void DoLoadGameObjectNoClone(EPoolType poolType, string addressableName, Action delFinish)
	{
		ProtLoadGameObjectNoClone(poolType.ToString(), addressableName, () =>
		{
			delFinish?.Invoke();
		});
	}

	//-----------------------------------------------------------------
	/// <summary>
	/// 해당 어드레서블 에셋이 Reserve가 되어 있지 않으면 null을 반환
	/// </summary>
	public GameObject FindClone(EPoolType poolType, string addressableName)
	{
		GameObject cloneInstance = ProtFindClone(poolType.ToString(), addressableName);
		return cloneInstance;
	}

	public T FindClone<T>(EPoolType poolType, string addressableName) where T : Component
	{
		GameObject cloneInstance = ProtFindClone(poolType.ToString(), addressableName);
		T cloneComponent = null;
		if (cloneInstance != null)
		{
			cloneComponent = cloneInstance.GetComponent<T>();
		}
		return cloneComponent;
	}

	//-----------------------------------------------------------------
	public void ClearCategory(EPoolType poolType, bool isReleaseOrigin = false)
	{
		ProtClearCategory(poolType.ToString(), isReleaseOrigin);
	}
}
