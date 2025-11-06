using System;
using UnityEngine;

namespace LUIZ.UI
{
    /// <summary>
    /// UI의 Show, Hide, 오버라이드 등을 할 때 이용. 제한된 객체만 접근 가능하도록 하기위해 SO를 이용한다.
    /// [참고]
    /// UIFrame들은 본인을 제외한 다른 UIFrame을 참조해선 안된다. 반드시 독립된 객체로 행동하며, Show Hide 정도만 통제할 수 있다.
    /// </summary>
    [CreateAssetMenu(fileName = "ManagerUISO", menuName = "LUIZ/ScriptableObjects/ManagerUISO")]
    public class ManagerUISO : ScriptableObject
    {
        //TODO : showHide 상태 말고 그냥 UIFrame의 좀 많이 쓸만한 상태들을 인터페이스로 분리해서 반환해주는거 고려
        internal Func<Type, bool> OnUIFrameStateRequest;
        internal Func<Type, UIFrameBase> OnUIShowRequest;
        internal Func<Type, UIFrameBase> OnUIHideRequest;
        internal Func<UIFrameBase, Type, UIFrameBase> OnUIOverrideAddRequest;
        internal Func<Type, bool> OnUIOverrideRemoveRequest;
        
        //-------------------------------------------------------
        public bool IsUIFrameShow<T>() where T : UIFrameBase, new()
        {
            return OnUIFrameStateRequest?.Invoke(typeof(T)) ?? false;
        }
        
        public T UIShow<T>() where T : UIFrameBase, new()
        {
            if (OnUIShowRequest == null)
            {
                Debug.LogError("[ManagerUISO] UIShowRequest가 구독되어 있지 않습니다.");
                return null;
            }
            return OnUIShowRequest(typeof(T)) as T;
        }

        public T UIHide<T>() where T : UIFrameBase, new()
        {
            if (OnUIHideRequest == null)
            {
                Debug.LogError("[ManagerUISO] UIHideRequest가 구독되어 있지 않습니다.");
                return null;
            }
            return OnUIHideRequest(typeof(T)) as T;
        }

        //Find는 노출 하지 않는거로
        /*public T UIFind<T>() where T : UIFrameBase, new()
        {
            if (OnUIFindRequest == null)
            {
                Debug.LogError("[ManagerUISO] UIFindRequest가 구독되어 있지 않습니다.");
                return null;
            }
            return OnUIFindRequest(typeof(T)) as T;
        }*/

        //전달 받은 overrideFrame을 기존에 존재하는 TOrigin에 덮어씌운다. 이후 UIShow<TOrigin>의 호출하더라도 TOverride 프레임이 Show 된다
        //오버라이드 객체인 TOverride는 TOrigin을 상속받은 프레임이어야한다. ( 게임의 모드에 따라 같은 목적의 UIFrame인데 바리에이션이 있거나 할떄 이용 )
        public TOverride UIOverrideAdd<TOverride, TOrigin>(TOverride overrideFrame) where TOverride : TOrigin, new() where TOrigin : UIFrameBase, new()
        {
            if (OnUIOverrideAddRequest == null)
            {
                Debug.LogError("[ManagerUISO] UIOverrideAddRequest가 구독되어 있지 않습니다.");
                return null;
            }
            return OnUIOverrideAddRequest(overrideFrame, typeof(TOrigin)) as TOverride;
        }

        public bool UIOverrideRemove<TOrigin>() where TOrigin : UIFrameBase, new()
        {
            if (OnUIOverrideRemoveRequest == null)
            {
                Debug.LogError("[ManagerUISO] UIOverrideRemoveRequest가 구독되어 있지 않습니다.");
                return false;
            }
            return OnUIOverrideRemoveRequest(typeof(TOrigin));
        }
    }
}
