using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using LUIZ;

public class LocalSaveLoaderSettings : LocalSaveLoaderPlayerPrefBase<LocalDataSettings>
{
    [SerializeField] private ManagerLocalData.ELocalDataType DataKey = ManagerLocalData.ELocalDataType.Settings;
    public LocalDataSettings Data { get; private set; }

    //-------------------------------------------------------
    protected override void OnSaveLoaderPlayerPrefDataNone()
    {
        Data = new LocalDataSettings();
        DoTaskSaveData();
        Debug.LogWarning("Settings Data None! making new Data");
    }

    //-------------------------------------------------------
    public override async Task DoTaskLoadData()
    {
        Data = await ProtLoadLocalData(DataKey.ToString());
    }

    public override Task DoTaskSaveData()
    {
        return ProtSaveLocalData(DataKey.ToString(), Data);
    }

    //-------------------------------------------------------
}
