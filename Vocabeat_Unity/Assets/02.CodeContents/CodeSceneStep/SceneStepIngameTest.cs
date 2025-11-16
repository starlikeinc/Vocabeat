using LUIZ.UI;
using System.Collections.Generic;
using UnityEngine;

public class SceneStepIngameTest : SceneStepBase
{
    [SerializeField] private Camera DevelopCamera;
    [SerializeField] private UIContainerBase UIContainerDevelop;

    //-------------------------------------------------------------
    protected override void OnUnityStart()
    {
        ProtLoadBasicDependencies(() =>
        {
            UIContainerDevelop.DoRegisterContainer();
            PrivSceneStepFinish();
        });
    }

    //-------------------------------------------------------------
    private void PrivSceneStepFinish()
    {
        UIChannel.UIShow<UIFrameInGame>();
    }
}
