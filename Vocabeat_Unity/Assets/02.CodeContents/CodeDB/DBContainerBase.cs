using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class DBContainerBase
{
    protected DBSheetUserData m_DBUserData = new ();                    public DBSheetUserData UserData { get { return m_DBUserData; } }

    //------------------------------------------------------------
    public abstract Task DoDBContainerInitialize();
}