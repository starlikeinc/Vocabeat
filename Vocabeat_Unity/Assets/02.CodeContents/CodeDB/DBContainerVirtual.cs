using LUIZ;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class DBContainerVirtual : DBContainerBase
{
    public override Task DoDBContainerInitialize()
    {
        return PrivInitializeAllVirtualData();
    }

    //--------------------------------------------------------
    private Task PrivInitializeAllVirtualData()
    {
        UserData userData = new UserData();
        userData.UserID = 10001;
        userData.UserName = "TestUser10001";
        userData.UserLevel = 39;
        userData.UserEXP = 55;

        m_DBUserData.DoUserDataSetting(userData);       

        Debug.Log("DBVirtual Init Fin");
        return Task.CompletedTask;
    }
}
