using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LUIZ
{
    public abstract class ManagerDBBase : SingletonBase<ManagerDBBase>, IManagerInstance
    {
        //-----------------------------------------------
        public abstract bool IsInitialized();

        //-----------------------------------------------
    }
}
