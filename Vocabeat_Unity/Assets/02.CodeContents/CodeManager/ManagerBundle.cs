using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LUIZ;
public class ManagerBundle : SingletonBase<ManagerBundle>, IManagerInstance
{
    private List<IManagerInstance> m_listChildManagers = new List<IManagerInstance>();

    //----------------------------------------------------
    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();
        GetComponentsInChildren(true, m_listChildManagers);
    }

    //----------------------------------------------------
    public bool IsInitialized()
    {
        return PrivCheckBundleManagersInitialize();
    }
 
    //----------------------------------------------------
    private bool PrivCheckBundleManagersInitialize()
    {
        int readyCount = 0;

        for (int i = 0; i < m_listChildManagers.Count; i++)
        {
            if (m_listChildManagers[i] is ManagerBundle)
            {
                readyCount++;
            }
            else
            {
                if (m_listChildManagers[i].IsInitialized())
                {
                    readyCount++;
                }
            }
        }

        return readyCount == m_listChildManagers.Count;
    }
}
