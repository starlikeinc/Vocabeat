using System.Collections.Generic;

[System.Serializable]
public class UserData
{
    public uint   UserID;
    public string UserName;
    public int    UserLevel;
    public int    UserEXP;

    public UserDataClone Clone()
    {
        UserDataClone cache = new UserDataClone();
        cache.UserID = UserID;
        cache.UserName = UserName;
        cache.UserLevel = UserLevel;
        cache.UserEXP = UserEXP;
        return cache;
    }
}

public struct UserDataClone
{
    public uint UserID;
    public string UserName;
    public int UserLevel;
    public int UserEXP;
}

public class DBSheetUserData
{
    private UserData m_userData = new UserData();

    //---------------------------------------
    public UserDataClone GetUserData()
    {
        return m_userData.Clone();
    }

    //---------------------------------------
    public void DoUserDataSetting(UserData userData)
    {
        m_userData = userData;
    }

    public void DoUserDataRefresh()
    {
        //m_userData, m_userDataCache
    }
}
