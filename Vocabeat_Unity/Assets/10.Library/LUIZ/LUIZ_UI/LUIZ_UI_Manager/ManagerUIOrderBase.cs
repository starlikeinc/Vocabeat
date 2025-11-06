using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LUIZ.UI
{
    public abstract class ManagerUIOrderBase : ManagerUIBase
    {
        private const int c_FrameGap = 10;      //같은 OrderType의 UIFrame들 간의 Gap
        private const int c_TypeGap = 100;      //다른 OrderType 과의 Gap

        public enum EUIFrameOrderType
        {
            None = 0,

            Panel,          //Show 순서에 따라 정렬되어 출력되며 일반적으로 사용하는 타입
            PanelTop,       //Panel 중 항상 위쪽에 위치한다.

            Popup,          //Panel, PanelTop 보다 항상 위에 그려진다.
            //PopupExclusive, //다른 Popup을 모두 Hide시키고 자신만을 보여준다. 닫힐 경우 이전 Popup이 출력된다. (TODO : 구현 필요)
        }

        /// <summary>
        /// 해당 SO는 반드시 존재해야한다.
        /// 타 객체나 UIFrame 들에게 매니저의 기능을 전달하기 위해 이용한다. 없으면 타 객체에서 UI매니저에 접근할 수 없다.
        /// </summary>
        [SerializeField] private ManagerUISO ManagerUISO;
        
        private int m_iCanvasOrder = 0; //인스펙터에서 셋팅된 켄버스 오프셋 

        private readonly LinkedList<UIFrameBase> m_listFrameOrderPanel = new LinkedList<UIFrameBase>();
        private readonly LinkedList<UIFrameBase> m_listFrameOrderPanelTop = new LinkedList<UIFrameBase>();
        private readonly LinkedList<UIFrameBase> m_listFrameOrderPopup = new LinkedList<UIFrameBase>();

        //---------------------------------------------------------
        protected override void OnMgrUIInitialize()
        {
            base.OnMgrUIInitialize();
            
            m_iCanvasOrder = GetRootCanvas().sortingOrder;
            SubscribeToManagerUISO();
        }

        protected override void OnMgrUIFrameRegister(UIFrameBase uiFrame)
        {
            uiFrame.InterUIFrameManagerSOSet(this.ManagerUISO);
        }
        
        protected override void OnMgrUIFrameUnregister(UIFrameBase uiFrame)
        {
            base.OnMgrUIFrameUnregister(uiFrame);
            ProtMgrUIOrderShowHide(uiFrame.GetType(), false);
        }

        protected override void OnMgrUIContainerRegister(UIContainerBase uiContainer)
        {
            base.OnMgrUIContainerRegister(uiContainer);
            uiContainer.InterUIContainerManagerSOSet(this.ManagerUISO);
        }

        protected override void OnMgrUIContainerUnregister(UIContainerBase uiContainer)
        {
            base.OnMgrUIContainerUnregister(uiContainer);
        }

        //---------------------------------------------------------
        protected bool ProtGetUIFrameState(Type type)
        {
            var uiFrame = ProtMgrUIFindUIFrame(type);
            return uiFrame != null && uiFrame.IsUIFrameShow();
        }
        
        protected UIFrameBase ProtMgrUIOrderShowHide(Type uiFrameType, bool isShow)
        {
            UIFrameBase uiFrame = ProtMgrUIFindUIFrame(uiFrameType);

            if (uiFrame != null)
            {
                UIFrameShowHide(uiFrame, isShow);
            }
            else
            {
                Debug.LogError("[UIFrame] Invalid UIFrame : " + uiFrameType.Name);
            }

            return uiFrame;
        }

        //---------------------------------------------------------
        private void UIFrameShowHide(UIFrameBase uiFrame, bool isShow)
        {
            EUIFrameOrderType orderType = uiFrame.GetUIFrameOrderType();
            switch (orderType)
            {
                case EUIFrameOrderType.Panel:
                    UIFramePanelShowHide(uiFrame, isShow);
                    break;
                case EUIFrameOrderType.PanelTop:
                    UIFramePanelTopShowHide(uiFrame, isShow);
                    break;
                case EUIFrameOrderType.Popup:
                    UIFramePopupShowHide(uiFrame, isShow);
                    break;
                    /*            case EUIFrameOrderType.PopupExclusive:
                                    PrivUIFramePopupExclusiveShowHide(uiFrame, isShow);
                                    break;*/
            }
        }

        //---------------------------------------------------------
        private void UIFramePanelShowHide(UIFrameBase uiFrame, bool isShow)
        {
            if (isShow == true)
            {
                m_listFrameOrderPanel.Remove(uiFrame);
                m_listFrameOrderPanel.AddLast(uiFrame);

                UIFrameRefreshOrder(m_listFrameOrderPanel, PrivExtractBottomOrderPanel());
                UIFrameRefreshOrder(m_listFrameOrderPanelTop, PrivExtractBottomOrderPanelTop());
                UIFrameRefreshOrder(m_listFrameOrderPopup, PrivExtractBottomOrderPanelPopup());

                uiFrame.InterUIFrameShow();
            }
            else
            {
                m_listFrameOrderPanel.Remove(uiFrame);

                UIFrameRefreshOrder(m_listFrameOrderPanel, PrivExtractBottomOrderPanel());

                uiFrame.InterUIFrameHide();
            }
        }

        private void UIFramePanelTopShowHide(UIFrameBase uiFrame, bool isShow)
        {
            if (isShow == true)
            {
                m_listFrameOrderPanelTop.Remove(uiFrame);
                m_listFrameOrderPanelTop.AddLast(uiFrame);

                UIFrameRefreshOrder(m_listFrameOrderPanelTop, PrivExtractBottomOrderPanelTop());
                UIFrameRefreshOrder(m_listFrameOrderPopup, PrivExtractBottomOrderPanelPopup());

                uiFrame.InterUIFrameShow();
            }
            else
            {
                m_listFrameOrderPanelTop.Remove(uiFrame);

                UIFrameRefreshOrder(m_listFrameOrderPanelTop, PrivExtractBottomOrderPanelTop());

                uiFrame.InterUIFrameHide();
            }
        }

        private void UIFramePopupShowHide(UIFrameBase uiFrame, bool isShow)
        {
            if (isShow == true)
            {
                m_listFrameOrderPopup.Remove(uiFrame);
                m_listFrameOrderPopup.AddLast(uiFrame);

                UIFrameRefreshOrder(m_listFrameOrderPopup, PrivExtractBottomOrderPanelPopup());

                uiFrame.InterUIFrameShow();
            }
            else
            {
                m_listFrameOrderPopup.Remove(uiFrame);

                UIFrameRefreshOrder(m_listFrameOrderPopup, PrivExtractBottomOrderPanelPopup());

                uiFrame.InterUIFrameHide();
            }
        }

        private void UIFrameRefreshOrder(LinkedList<UIFrameBase> listFrames, int bottomOrder)
        {
            int currentGap = 0;

            foreach (UIFrameBase uiFrame in listFrames)
            {
                uiFrame.InterUIFrameChangeOrder(bottomOrder + currentGap);
                currentGap += c_FrameGap;
            }
        }

        private void SubscribeToManagerUISO()
        {
            if (this.ManagerUISO == null)
            {
                Debug.LogWarning("[ManagerUIOrderBase] ManagerUISO is NULL!!!!]");
                return;
            }
            
            this.ManagerUISO.OnUIShowRequest = (type) => ProtMgrUIOrderShowHide(type, true);
            this.ManagerUISO.OnUIHideRequest = (type) => ProtMgrUIOrderShowHide(type, false);
            this.ManagerUISO.OnUIOverrideAddRequest = ProtMgrUIOverrideFrameAdd;
            this.ManagerUISO.OnUIOverrideRemoveRequest = ProtMgrUIOverrideFrameRemove;
            
            this.ManagerUISO.OnUIFrameStateRequest = ProtGetUIFrameState;
        }

        //---------------------------------------------------------
        private int PrivExtractBottomOrderPanel()
        {
            return m_iCanvasOrder + c_TypeGap;
        }

        private int PrivExtractBottomOrderPanelTop()
        {
            return PrivExtractBottomOrderPanel() + (m_listFrameOrderPanel.Count * c_FrameGap) + c_TypeGap;
        }

        private int PrivExtractBottomOrderPanelPopup()
        {
            return PrivExtractBottomOrderPanelTop() + (m_listFrameOrderPanelTop.Count * c_FrameGap) + c_TypeGap;
        }
    }
}
