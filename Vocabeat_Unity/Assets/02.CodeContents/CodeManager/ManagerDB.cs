using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using LUIZ;

public class ManagerDB : ManagerDBBase
{
    public new static ManagerDB Instance => ManagerDBBase.Instance as ManagerDB;

    //----------------------------------------------------------------
    public enum EGameDBType
    {
        Production,         //실제 프로덕션 플레이 ( 실제 DB에 연결 )
        DevelopServer,      //develop 서버 전용   ( 개발 전용 DB에 연결 )
        DevelopVirtual      //develop 전용       ( 하드코딩 된 가상 데이터에 연결 )
    }

    private bool m_isDBInitialized = false;

    private DBContainerBase m_DBContainer = null;   public DBContainerBase DB { get { return m_DBContainer; } }

    //----------------------------------------------------------------
    public Task DoManagerDBInitialize(EGameDBType DBType)
    {
        return PrivManagerDBInitialize(DBType);
    }

    public override bool IsInitialized()
    {
        return m_isDBInitialized;
    }

    //----------------------------------------------------------------
    private async Task PrivManagerDBInitialize(EGameDBType DBType)
    {
        if (m_isDBInitialized == true)
        {
            Debug.LogWarning("DB is already initialized!!");
            return;
        }

        switch (DBType)
        {
            case EGameDBType.Production:
                break;
            case EGameDBType.DevelopServer:
                break;
            case EGameDBType.DevelopVirtual:
                m_DBContainer = new DBContainerVirtual();
                break;
        }

        await m_DBContainer.DoDBContainerInitialize();
        m_isDBInitialized = true;
    }
}
