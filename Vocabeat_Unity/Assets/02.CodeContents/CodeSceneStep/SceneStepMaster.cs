using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LUIZ;
using System;
using System.Threading.Tasks;

public class SceneStepMaster : SceneStepBase
{
    protected override void OnUnityStart()
    {
        ProtLoadBasicDependencies(() =>
        {
            if (ManagerApplication.Instance.IsRegularAppPlay() == false)
            {
                PrivSceneStepFinish();
                return;
            }

            List<ISceneLoadingWork> listLoadingWorks = new List<ISceneLoadingWork>
            {
                new SceneLoadingWork_MinTime(2f),
                new SceneLoadingWorkDB(ManagerDB.EGameDBType.DevelopVirtual),
                new SceneLoadingWorkMainUI()
            };

            ManagerLoaderScene.Instance.DoMoveToSceneHome(() =>
            {
                PrivSceneStepFinish();
            }, listLoadingWorks);
        });
    }

    //-------------------------------------------------------------
    private void PrivSceneStepFinish()
    {
        Destroy(gameObject);
        Debug.Log("FIN");
    }

    //-------------------------------------------------------------
    private class SceneLoadingWorkMainUI : ISceneLoadingWork
    {
        public int WorkID => 2001;
        public string WorkDescription => "메인 UI 로딩";
        public bool LoadAtLastStep => true;

        //---------------------------------------------------------------
        public Task DoTaskLoadingWork()
        {
            return PrivLoadMainUI();
        }

        //-----------------------------------------------
        private async Task PrivLoadMainUI()
        {
            var tcs = new TaskCompletionSource<bool>();
            
            ManagerLoaderScene.Instance.DoLoadSceneUIMain(() => { 
                Debug.Log("LoadMainUI_Fin");
                tcs.SetResult(true);
            });

            await tcs.Task;
        }
    }
}
