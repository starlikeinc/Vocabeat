using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LUIZ;
public class ManagerLocalData : ManagerLocalDataBase
{
    public enum ELocalDataType
    {
        Settings = 0,
    }

    public new static ManagerLocalData Instance => ManagerLocalDataBase.Instance as ManagerLocalData;

    //------------------------------------------------------------------
    public LocalSaveLoaderSettings Settings { get; private set; } = null;

    //------------------------------------------------------------------
    protected override void OnMgrLocalSaveLoaderInit(IDataLoader saveLoader)
    {
        switch (saveLoader)
        {
            case LocalSaveLoaderSettings saveLoaderSettings:
                Settings = saveLoaderSettings;
                break;
        }
    }
    //------------------------------------------------------------------
}
