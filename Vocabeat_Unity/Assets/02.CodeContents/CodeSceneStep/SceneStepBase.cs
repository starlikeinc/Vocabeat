using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using LUIZ;
using LUIZ.UI;
using UnityEngine.Serialization;

public abstract class SceneStepBase : SceneAttacherBase
{
    protected static ManagerUISO UIChannel
    {
        get
        {
            if (s_uiChannel == null)
                s_uiChannel = Resources.Load("ManagerUISO") as ManagerUISO;
            
            return s_uiChannel;
        }
    }

    protected static ManagerUISO s_uiChannel = null;


    protected const string c_ManagerPrefabPath = "FrontPrefab";
    protected const string c_ManagerPrefabName = "PrefabManagerFront";

    protected const string c_ManagerBundlePrefabName = "PrefabManagerBundle";

    //------------------------------------------------------------------------
    protected void ProtLoadBasicDependencies(Action delFinish)
    {
        ProtLoadResourcePrefab(c_ManagerPrefabPath, c_ManagerPrefabName, () =>
        {
            ProtLoadAddressablePrefab(c_ManagerBundlePrefabName, (bool isSuccess, GameObject bundlePrefab) =>
            {
                PrivWaitForBasicManagers(delFinish);
            });
        });
    }

    //------------------------------------------------------------------------
    private async void PrivWaitForBasicManagers(Action delFinish)
    {
        await PrivTaskWaitForManagerInit(ManagerBundle.Instance);
        await PrivTaskWaitForManagerInit(ManagerLocalDataBase.Instance);
        //TODO: 대기 해야할 거 있으면 여기 추가
        
        delFinish?.Invoke();
    }

    private async Task PrivTaskWaitForManagerInit<T>(T instance) where T : IManagerInstance
    {
        if(instance != null && instance.IsInitialized())
            return;
        
        while(instance == null)
            await Task.Yield();

        while (instance.IsInitialized() == false)
            await Task.Yield();

        Debug.Log($"{instance.GetType().Name} READY");
    }

    //------------------------------------------------------------------------
    protected class SceneLoadingWorkDB : ISceneLoadingWork
    {
        public int WorkID => 1001;
        public string WorkDescription => "데이터 베이스 로딩 중";
        public bool LoadAtLastStep => false;

        private readonly ManagerDB.EGameDBType m_DBType;
        
        //---------------------------------------------------------------
        public SceneLoadingWorkDB(ManagerDB.EGameDBType dbType)
        {
            m_DBType = dbType;
        }

        //---------------------------------------------------------------
        public Task DoTaskLoadingWork()
        {
            return PrivStartDBLoad(m_DBType);
        }

        //---------------------------------------------------------------
        private Task PrivStartDBLoad(ManagerDB.EGameDBType DBType)
        {
            return ManagerDB.Instance.DoManagerDBInitialize(DBType);
        }
    }

    protected class SceneLoadingWorkLocalData : ISceneLoadingWork
    {
        public int WorkID => 1002;
        public string WorkDescription => "로컬 데이터 로딩 중";
        public bool LoadAtLastStep => false;        

        public Task DoTaskLoadingWork()
        {
            //ManagerLocalData.Instance.Settings.DoTaskLoadData(); Settings는 LoadOnAwake로 자동 로드 중임
            //이후 필요 시 더 추가 할 것 ,,,
            return Task.CompletedTask;
        }
    }    
}
