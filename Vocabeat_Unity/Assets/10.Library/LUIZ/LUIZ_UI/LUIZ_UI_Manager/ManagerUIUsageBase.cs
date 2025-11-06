using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LUIZ.UI
{
    /// <summary>
    /// 미사용 클래스. SO를 경유해서 매니저 기능에 접근하도록 바뀜
    /// </summary>
    public abstract class ManagerUIUsageBase : ManagerUIOrderBase
    {
        //------------------------------------------------------------------------
        internal T InterUIShow<T>() where T : UIFrameBase, new()
        {
            return ProtMgrUIOrderShowHide(typeof(T), true) as T;
        }
        
        internal T InterUIHide<T>() where T : UIFrameBase, new()
        {
            return ProtMgrUIOrderShowHide(typeof(T), false) as T;
        }
        
        internal T InterUIFind<T>() where T : UIFrameBase, new()
        {
            return ProtMgrUIFindUIFrame(typeof(T)) as T;
        }

        //전달 받은 overrideFrame을 기존에 존재하는 TOrigin에 덮어씌운다. 이후 UIShow<TOrigin>의 호출하더라도 TOverride 프레임이 Show 된다
        //오버라이드 객체인 TOverride는 TOrigin을 상속받은 프레임이어야한다. ( 게임의 모드에 따라 같은 목적의 UIFrame인데 바리에이션이 있거나 할떄 이용 )
        internal TOverride InterUIOverrideAdd<TOverride, TOrigin>(TOverride overrideFrame) where TOverride : TOrigin, new() where TOrigin : UIFrameBase, new()
        {
            return ProtMgrUIOverrideFrameAdd(overrideFrame, typeof(TOrigin)) as TOverride;
        }
        
        internal bool InterUIOverrideRemove<TOrigin>() where TOrigin : UIFrameBase, new()
        {
            return ProtMgrUIOverrideFrameRemove(typeof(TOrigin));
        }
    }
}