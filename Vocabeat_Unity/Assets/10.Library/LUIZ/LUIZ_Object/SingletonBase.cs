using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LUIZ
{
    public interface IManagerInstance
    {
        public bool IsInitialized(); //매니저 마다 Initialize가 완료되어 사용자가 호출 할 수 있는 타이밍이 다를 수 있음.
    }

    public abstract class SingletonBase<TInstance> : MonoBase where TInstance : class
    {
        private static SingletonBase<TInstance> StaticInstance = null;

        [SerializeField] private bool DontDestroy = true;

        //------------------------------------------------------------
        public static TInstance Instance
        {
            get
            {
                return StaticInstance as TInstance;
            }
        }

        //------------------------------------------------------------
        protected override void OnUnityAwake()
        {
            if (DontDestroy)
            {
                Transform rootTransform = GetTransformRoot();
                DontDestroyOnLoad(rootTransform.gameObject);
            }

            StaticInstance = this;
        }
    }
}