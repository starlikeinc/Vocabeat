using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Threading.Tasks;

namespace LUIZ
{
    //Json을 기반으로 한 로컬 persistentdatapath의 저장, 로드 를 도와주는 매니저.
    //보안, 백엔드 데이터는 ManagerDB를 이용할 것
    public class ManagerLocalDataBase : SingletonBase<ManagerLocalDataBase>, IManagerInstance
    {
        private bool m_isInitialized = false;
        
        //-----------------------------------------------------------------
        public bool IsInitialized() => m_isInitialized;
        
        //-----------------------------------------------------------------
        protected override async void OnUnityAwake()
        {
            base.OnUnityAwake();
            
            List<IDataLoader> listSaveLoader = new List<IDataLoader>();
            GetComponentsInChildren(true, listSaveLoader);
            
            List<Task> listTasks = new List<Task>();

            for (int i = 0; i < listSaveLoader.Count; i++)
            {
                if (listSaveLoader[i].IsAutoLoad)
                {
                    var task = listSaveLoader[i].DoTaskLoadData();
                    listTasks.Add(task);
                }

                // IsAutoLoad가 false여도 초기화 콜백은 바로 호출
                OnMgrLocalSaveLoaderInit(listSaveLoader[i]);
            }
            OnMgrLocalSaveLoaderInitFinish();
            
            await Task.WhenAll(listTasks);
            m_isInitialized = true;
        }

        //-----------------------------------------------------------------
        protected virtual void OnMgrLocalSaveLoaderInit(IDataLoader saveLoader) { }
        protected virtual void OnMgrLocalSaveLoaderInitFinish() { }
    }
}
