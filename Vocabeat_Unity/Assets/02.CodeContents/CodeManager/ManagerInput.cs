using System;
using System.Collections.Generic;
using UnityEngine;
using LUIZ.InputSystem;

public class ManagerInput : ManagerInputBase
{
    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();

        var playerInputActions = new PlayerInputActions();
        ProtMgrInputBaseSetting(playerInputActions, playerInputActions.asset);

        DoMgrInputBaseDisableAllActionMaps();
        DoMgrInputBaseChangeActionMap("PC_Player");
        
        //이거로 등록 해두면 InputSO의 이벤트에 등록 됨
        ProtMgrInputAddAction<Vector2>("Move");
        //ProtMgrInputAddAction<bool>("CompositeTest"); 테스트중
    }
}
