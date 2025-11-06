using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LUIZ.UI;

public class SceneStepDevelopLUIZ : SceneStepBase
{
    [SerializeField] private Camera DevelopCamera;
    [SerializeField] private UIContainerBase UIContainerDevelop;

    //-------------------------------------------------------------
    protected override void OnUnityStart()
    {
        ProtLoadBasicDependencies(() =>
        {
            List<ISceneLoadingWork> listLoadingWorks = new List<ISceneLoadingWork>
            {
                new SceneLoadingWorkDB(ManagerDB.EGameDBType.DevelopVirtual)
            };

            ManagerLoaderScene.Instance.DoMoveToSceneHome(() =>
            {
                UIContainerDevelop.DoRegisterContainer();
                DevelopCamera.gameObject.SetActive(false);

                PrivSceneStepFinish();

            }, listLoadingWorks);
        });
    }

    //-------------------------------------------------------------
    private void PrivSceneStepFinish()
    {
        UIChannel.UIShow<UIFrameTest>();

        //UIChannel.UIShow<UIFrameTestPopup>();
    }
}