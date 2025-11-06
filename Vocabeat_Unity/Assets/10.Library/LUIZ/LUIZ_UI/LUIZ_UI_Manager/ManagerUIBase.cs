using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LUIZ.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    public abstract class ManagerUIBase : SingletonBase<ManagerUIBase>, IManagerInstance
    {
        public enum EUIContainerType
        {
            Normal,   //중복으로 레이어링이 가능한 일반적인 컨테이너
            Exclusive,//Exclusive 컨테이너가 등록되면 해당 컨테이너가 등록되어 있는 동안 다른 Normal 컨테이너 gameobject는 전부 Hide 된다.
        }

        private readonly Dictionary<Type, UIFrameBase> m_dicUIFrameOriginAll  = new();//오리진 프레임의 타입 / 오리진 프레임
        private readonly Dictionary<Type, UIFrameBase> m_dicUIFrameOverrides = new();//Origin 프레임 / 오버라이드 프레임 객체
        private readonly List<UIContainerBase> m_listContainers = new();

        private UIContainerBase m_currentExclusiveContainer = null;

        private Vector3 m_screenSize = Vector3.zero;        public Vector2 GetUIScreenSize() { return m_screenSize; }
        private Canvas m_rootCanvas = null;                 public Canvas GetRootCanvas() { return m_rootCanvas; }

        private CanvasScaler m_rootCanvasScaler = null;
        private EventSystem m_eventSystem = null;
        private StandaloneInputModule m_standAloneInputModule = null;

        //-------------------------------------------------------------------
        protected override void OnUnityAwake()
        {
            base.OnUnityAwake();
            PrivInitializeManagerUI();
        }

        //-------------------------------------------------------------------
        public bool IsInitialized()
        {
            return Instance != null;
        }

        //-------------------------------------------------------------------
        internal bool InterMgrUITryRegisterContainer(UIContainerBase uiContainer)
        {
            bool isSuccess = true;

            if (m_listContainers.Contains(uiContainer) == false)
            {
                if (uiContainer.GetContainerType() == EUIContainerType.Exclusive)
                {
                    PrivShowHideAllContainers(false);
                    m_currentExclusiveContainer = uiContainer;
                }
                else
                {
                    if (m_currentExclusiveContainer != null)//Exclusive 컨테이너가 존재하면 추가된 컨테이너는 우선 SetActive(false)
                        uiContainer.gameObject.SetActive(false);
                }

                m_listContainers.Add(uiContainer);
                
                OnMgrUIContainerRegister(uiContainer);
            }
            else
            {
                Debug.LogWarning($"[ManagerUIBase] Failed to register {uiContainer.gameObject.name}. container is already registered.");
                isSuccess = false;
            }

            return isSuccess;
        }

        internal void InterMgrUIUnregisterContainer(UIContainerBase uiContainer)
        {
            if (m_listContainers.Remove(uiContainer) == true)
            {
                if (uiContainer.GetContainerType() == EUIContainerType.Exclusive)
                {
                    PrivShowHideAllContainers(true);
                }
                
                OnMgrUIContainerUnregister(uiContainer);
            }
            else
            {
                //ERROR
            }
        }

        internal bool InterMgrUIRegisterUIFrame(UIFrameBase uiFrame)
        {
            bool isSuccess = PrivTryRegisterUIFrame(uiFrame);

            if (isSuccess == true)
            {
                uiFrame.InterUIFrameInitialize();
            }

            return isSuccess;
        }

        internal bool InterMgrUIUnregisterUIFrame(UIFrameBase uiFrame)
        {
            return PrivTryUnregisterUIFrame(uiFrame);
        }

        internal Camera InterMgrUIGetUICamera()
        {
            if (m_rootCanvas == null)
                return null;

            return m_rootCanvas.worldCamera;
        }

        //-------------------------------------------------------------------
        protected UIFrameBase ProtMgrUIFindUIFrame(Type uiFrameType)
        {
            UIFrameBase findUIFrame = null;

            if (m_dicUIFrameOverrides.TryGetValue(uiFrameType, out UIFrameBase overrideFrame))
            {
                //오버라이드 된 프레임이 있다면 해당 프레임을 반환한다.
                findUIFrame = overrideFrame;
            }
            else
            {
                m_dicUIFrameOriginAll.TryGetValue(uiFrameType, out findUIFrame);
            }
            
            return findUIFrame;
        }

        protected UIFrameBase ProtMgrUIOverrideFrameAdd(UIFrameBase uiFrameOverride, Type uiFrameOrigin)
        {
            if (m_dicUIFrameOriginAll.TryGetValue(uiFrameOrigin, out _) == false)
            {
                Debug.LogError($"[ManagerUIBase] Failed to override {uiFrameOrigin.Name}. Origin Frame does not exist.");
                //uiFrameOrigin가 존재 하지않음. 오버라이드 할 오리진이 없음
                return null;
            }

            if (m_dicUIFrameOverrides.TryAdd(uiFrameOrigin, uiFrameOverride))
            {
                uiFrameOverride.OriginUIFrameType = uiFrameOrigin;
                return uiFrameOverride;
            }
            else
            {
                Debug.LogError($"[ManagerUIBase] Failed to override on {uiFrameOrigin.Name}. Already Overrided. Remove Override First");
                return null;
            }
        }

        protected bool ProtMgrUIOverrideFrameRemove(Type uiFrameOrigin)
        {
            if (m_dicUIFrameOverrides.Remove(uiFrameOrigin))
            {
                return true;
            }
            else
            {
                Debug.LogError($"[ManagerUIBase] Failed to Unregister override on {uiFrameOrigin.Name}. no Override");
                return false;
            }
        }
        
        //-------------------------------------------------------------------
        private void PrivShowHideAllContainers(bool isShow)
        {
            foreach(UIContainerBase uiContainer in m_listContainers)
            {
                uiContainer.gameObject.SetActive(isShow);
            }
        }

        private void PrivInitializeManagerUI()
        {
            PrivInitializeManagerUIDefaultComponent();

            RectTransform uiScreenSize = this.transform as RectTransform;
            m_screenSize = uiScreenSize.sizeDelta;

            OnMgrUIInitialize();
        }

        private void PrivInitializeManagerUIDefaultComponent()
        {
            m_rootCanvas = GetComponent<Canvas>();
            m_rootCanvasScaler = GetComponent<CanvasScaler>();
            m_eventSystem = GetComponent<EventSystem>();
            m_standAloneInputModule = GetComponent<StandaloneInputModule>();
        }

        private bool PrivTryRegisterUIFrame(UIFrameBase uiFrame)
        {
            Type uiFrameType = uiFrame.GetType();
            bool isSuccess = false;

            if (m_dicUIFrameOriginAll.ContainsKey(uiFrameType))
            {
                //Error
            }
            else
            {
                m_dicUIFrameOriginAll[uiFrameType] = uiFrame;
                isSuccess = true;
                OnMgrUIFrameRegister(uiFrame);
            }

            return isSuccess;
        }

        private bool PrivTryUnregisterUIFrame(UIFrameBase uiFrame)
        {
            Type uiFrameType = uiFrame.GetType();
            bool isSuccess = false;

            if (m_dicUIFrameOriginAll.TryGetValue(uiFrameType, out UIFrameBase findFrame))
            {
                if (findFrame != null && findFrame == uiFrame)
                {
                    OnMgrUIFrameUnregister(uiFrame);
                    
                    m_dicUIFrameOriginAll.Remove(uiFrameType);
                    isSuccess = true;   
                }
            }
            else//오리진에 없는 객체라면 오버라이드 객체가 파괴되어서 Unregist가 오는걸 수 있음.
            {
                Type originFrame = uiFrame.OriginUIFrameType;
                if (originFrame == null)
                    isSuccess = false;
                else
                {
                    //만약 이 UiFrame이 오버라이드 프레임 객체라면, 오버라이드를 제거함
                    isSuccess = ProtMgrUIOverrideFrameRemove(originFrame);
                }
            }

            return isSuccess;
        }

        //-------------------------------------------------------------------
        protected virtual void OnMgrUIInitialize() { }
        
        protected virtual void OnMgrUIFrameRegister(UIFrameBase uiFrame) { }       
        protected virtual void OnMgrUIFrameUnregister(UIFrameBase uiFrame) { }
        
        protected virtual void OnMgrUIContainerRegister(UIContainerBase uiContainer) { }       
        protected virtual void OnMgrUIContainerUnregister(UIContainerBase uiContainer) { }
    }
}
