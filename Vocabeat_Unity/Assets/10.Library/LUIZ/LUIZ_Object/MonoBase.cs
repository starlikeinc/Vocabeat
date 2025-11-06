using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LUIZ
{
    public abstract class MonoBase : MonoBehaviour
    {
        //Update 같은 경우는 한정적인 클래스에서만 사용하므로 각자 선언하여 이용
        //-----------------------------------------------------
        private void Awake() { OnUnityAwake(); }
        private void Start() { OnUnityStart(); }
        private void OnEnable() { OnUnityEnable(); }
        private void OnDisable() { OnUnityDisable(); }
        private void OnDestroy() { OnUnityDestroy(); }

        //-----------------------------------------------------
        public static void RemoveCloneObjectName(UnityEngine.Object pObject)
        {
            pObject.name = pObject.name.Replace("(Clone)", "").Trim();
        }

        public static string RemoveCloneObjectName(string strName)
        {
            return strName.Replace("(Clone)", "").Trim();
        }

        //-----------------------------------------------------
        public Transform GetTransformRoot(Transform transform = null)
        {
            Transform rootTransform = transform == null ? this.transform : transform;

            if (rootTransform.parent != null)
            {
                rootTransform = GetTransformRoot(rootTransform.parent);
            }
            return rootTransform;
        }

        //--------------------------------------------------------
        protected virtual void OnUnityStart() { }
        protected virtual void OnUnityAwake() { }
        protected virtual void OnUnityEnable() { }
        protected virtual void OnUnityDisable() { }
        protected virtual void OnUnityDestroy() { }
    }
}
