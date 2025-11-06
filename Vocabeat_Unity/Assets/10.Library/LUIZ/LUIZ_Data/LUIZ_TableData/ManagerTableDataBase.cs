using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LUIZ
{
    public abstract class ManagerTableDataBase : SingletonBase<ManagerTableDataBase>, IManagerInstance
    {
        private bool m_isInitialized = false;

        //-----------------------------------------------------------------
        protected override async void OnUnityAwake()
        {
            base.OnUnityAwake();
            
            List<IDataLoader> listTableData = new List<IDataLoader>();
            GetComponentsInChildren(true, listTableData);
            
            List<Task> listTasks = new List<Task>();

            for (int i = 0; i < listTableData.Count; i++)
            {
                if (listTableData[i].IsAutoLoad)
                {
                    var task = listTableData[i].DoTaskLoadData();
                    listTasks.Add(task);
                }
                // IsAutoLoad가 false여도 초기화 콜백은 바로 호출
                OnMgrTableDataInit(listTableData[i]);
            }
            OnMgrTableDataInitFinish();
            
            await Task.WhenAll(listTasks);
            m_isInitialized = true;
        }
        
        //-----------------------------------------------------------------
        public bool IsInitialized()
        {
            return m_isInitialized;
        }

        //-----------------------------------------------------------------
        protected virtual void OnMgrTableDataInit(IDataLoader tableData) { }
        protected virtual void OnMgrTableDataInitFinish() { }
    }
}
