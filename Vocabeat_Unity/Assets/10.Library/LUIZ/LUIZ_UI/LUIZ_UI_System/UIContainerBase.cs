using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LUIZ.UI
{

    /// <summary>
    /// UIFrame을 소유 하고 있는 큰 객체. Container는 UIFrame과 다르게 사용자가 Show, Hide 할 수 없다.
    /// 컨테이너를 숨기고 싶다면 제거후 재등록 하거나 Exclusive 컨테이너가 켜져야한다.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    public abstract class UIContainerBase : MonoBase
    {
        [SerializeField] private ManagerUIBase.EUIContainerType ContainerType;      public ManagerUIBase.EUIContainerType GetContainerType(){ return ContainerType; }
        [SerializeField] private bool RegisterOnStart = true;

        protected ManagerUISO UIChannel { get; private set; }
        
        private Canvas m_rootCanvas = null;
        private CanvasScaler m_rootCanvasScaler = null;

        private readonly List<UIFrameBase> m_listContainerFrames = new();

        //-------------------------------------------------------
        protected override void OnUnityAwake()
        {
            base.OnUnityAwake();

            m_rootCanvas = GetComponent<Canvas>();
            m_rootCanvasScaler = GetComponent<CanvasScaler>();
        }

        protected override void OnUnityStart()
        {
            base.OnUnityStart();

            if (RegisterOnStart == true)
            {
                PrivRegisterContainer();
            }
        }

        protected override void OnUnityDestroy()
        {
            base.OnUnityDestroy();
            ManagerUIBase.Instance.InterMgrUIUnregisterContainer(this);
        }

        //-------------------------------------------------------
        internal void InterUIContainerManagerSOSet(ManagerUISO managerUISO) => this.UIChannel = managerUISO;
        
        //-------------------------------------------------------
        public void DoRegisterContainer()
        {
            PrivRegisterContainer();
        }

        public Canvas GetContainerCanvas()
        {
            return m_rootCanvas;
        }

        //-------------------------------------------------------
        private void PrivRegisterContainer()
        {
            if (ManagerUIBase.Instance == null)
            {
                Debug.LogError($"[UIContainerBase] Failed to register {this.gameObject.name}. ManagerUI is NULL.");
                return;
            }

            if (ManagerUIBase.Instance.InterMgrUITryRegisterContainer(this))
            {
                Camera uiCamera = ManagerUIBase.Instance.InterMgrUIGetUICamera();
                Camera currentCamera = m_rootCanvas.worldCamera;

                if (currentCamera != null && currentCamera != uiCamera)
                {
                    currentCamera.gameObject.SetActive(false);
                }

                m_rootCanvas.worldCamera = uiCamera;

                Canvas managerCanvas = ManagerUIBase.Instance.GetRootCanvas();
                m_rootCanvas.sortingOrder = managerCanvas.sortingOrder;
                m_rootCanvas.planeDistance = managerCanvas.planeDistance;
                m_rootCanvas.sortingLayerID = managerCanvas.sortingLayerID;
                //TODO : canvas scaler °ªµµ º¹»çÇÏ±â

                PrivRegisterContainerUIChild();
            }
        }

        private void PrivRegisterContainerUIChild()
        {
            for (int i = 0; i < this.transform.childCount; i++)
            {
                UIFrameBase uiFrame = this.transform.GetChild(i).gameObject.GetComponentInChildren<UIFrameBase>(true);
                if (uiFrame != null)
                {
                    if (ManagerUIBase.Instance.InterMgrUIRegisterUIFrame(uiFrame))
                    {
                        m_listContainerFrames.Add(uiFrame);
                    }
                    else
                    {
                        Debug.LogWarning($"[UIContainerBase] Fail to register {uiFrame.gameObject.name}. Frame is already registered to ManagerUI");
                        uiFrame.gameObject.SetActive(false);
                    }
                }
            }

            foreach (UIFrameBase childFrame in m_listContainerFrames)
            {
                childFrame.InterUIFrameInitializePost();
            }

            Debug.Log($"{this.gameObject.name} Container Initialized", this.gameObject);
            OnContainerInitialize();
        }

        //-------------------------------------------------------
        protected virtual void OnContainerInitialize() { }
    }
}
