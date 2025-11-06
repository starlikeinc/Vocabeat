using System;
using System.Collections.Generic;
using LUIZ.UI;
using UnityEngine;

public abstract class SceneStepVirtual : SceneStepBase
{
    [SerializeField] private Camera DevelopCamera;
    [SerializeField] private UIContainerBase UIContainerDevelop;

    protected override void OnUnityStart()
    {
        ProtLoadBasicDependencies(() =>
        {
            List<ISceneLoadingWork> listLoadingWorks = new List<ISceneLoadingWork>
            {
                new SceneLoadingWorkDB(ManagerDB.EGameDBType.DevelopVirtual),
                new SceneLoadingWorkLocalData(),                
            };

            ManagerLoaderScene.Instance.DoMoveToSceneHome(() =>
            {
                UIContainerDevelop.DoRegisterContainer();
                DevelopCamera.gameObject.SetActive(false);

                OnSceneStepFinish();

            }, listLoadingWorks);
        });
    }

    protected virtual void OnSceneStepFinish() { }
}
