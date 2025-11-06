using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LUIZ.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public abstract class UIFrameBase : MonoBase
    {
        [Header("[ UIFrame ]")]
        [SerializeField] private ManagerUIOrderBase.EUIFrameOrderType OrderType = ManagerUIOrderBase.EUIFrameOrderType.Panel;

        private bool m_isShow = false;              internal bool   IsUIFrameShow()         { return m_isShow; }
        private Canvas m_canvas;                    internal Canvas GetUIFrameCanvas()      { return m_canvas; }
        
        //자식위젯들 TODO : linkedlist로 바꾸기?
        private List<UIWidgetBase> m_listChildWidget = new List<UIWidgetBase>();
        
        //------------------------------------------------------
        protected ManagerUISO UIChannel { get; private set; } = null;
        
        public ManagerUIOrderBase.EUIFrameOrderType GetUIFrameOrderType() { return OrderType; }
        
        //------------------------------------------------------
        protected override void OnUnityDestroy()
        {
            base.OnUnityDestroy();

            ManagerUIBase.Instance.InterMgrUIUnregisterUIFrame(this);
        }

        //------------------------------------------------------
        internal Type OriginUIFrameType { get; set; } = null;
        internal void InterUIFrameManagerSOSet(ManagerUISO managerUISO) => this.UIChannel = managerUISO;
        internal void InterUIFrameInitialize()
        {
            m_canvas = GetComponent<Canvas>();

            m_isShow = false;

            m_canvas.overrideSorting = true;
            m_canvas.sortingLayerName = LayerMask.LayerToName(gameObject.layer);

            this.gameObject.SetActive(false);

            OnUIFrameInitialize();

            PrivCollectChildWidgets();

            PrivNotifyChildWidgetsInitialize();
            PrivNotifyChildWidgetsInitializePost();
        }

        internal void InterUIFrameInitializePost()
        {
            OnUIFrameInitializePost();
        }

        internal void InterUIFrameChangeOrder(int uiOrder)
        {
            PrivChangeUIFrameOrder(uiOrder);
        }

        internal void InterUIFrameShow()
        {
            if (this.gameObject.activeSelf == false)
            {
                m_isShow = true;
                this.gameObject.SetActive(true);
                m_canvas.overrideSorting = true;

                OnUIFrameShow();

                PrivNotifyChildWidgetsFrameShow();
            }
            else
            {
                Debug.LogWarning($"[UIFrameBase] {this.gameObject.name} is already Shown. Show event will not be triggered.");
            }
        }

        internal void InterUIFrameHide()
        {
            if (this.gameObject.activeSelf == true)
            {
                m_isShow = false;
                this.gameObject.SetActive(false);

                OnUIFrameHide();

                PrivNotifyChildWidgetsFrameHide();
            }
            else
            {
                Debug.LogWarning($"[UIFrameBase] {this.gameObject.name} is already Hidden. Hide event will not be triggered.");
            }
        }

        internal void InterUIFrameAddChildWidget(UIWidgetBase addWidget)
        {
            PrivAddChildWidget(addWidget);
        }

        internal void InterUIFrameRemoveChildWidget(UIWidgetBase removeWidget)
        {
            PrivRemoveChildWidget(removeWidget);
        }

        //------------------------------------------------------
        private void PrivCollectChildWidgets()
        {
            GetComponentsInChildren(true, m_listChildWidget);
        }

        private void PrivAddChildWidget(UIWidgetBase addWidget)
        {
            List<UIWidgetBase> listWidgets = new List<UIWidgetBase>();
            addWidget.GetComponentsInChildren(true, listWidgets);

            foreach (UIWidgetBase widget in listWidgets)
            {
                m_listChildWidget.Add(widget);
                widget.InterUIWidgetInitialize(this);
            }

            foreach (UIWidgetBase widget in listWidgets)
            {
                m_listChildWidget.Add(widget);
                widget.InterUIWidgetInitializePost(this);
            }
        }

        private void PrivRemoveChildWidget(UIWidgetBase removeWidget)
        {
            List<UIWidgetBase> listWidgets = new List<UIWidgetBase>();
            removeWidget.GetComponentsInChildren(true,listWidgets);

            foreach(UIWidgetBase widget in listWidgets)
            {
                m_listChildWidget.Remove(widget);
            }
        }

        //------------------------------------------------------
        private void PrivChangeUIFrameOrder(int uiOrder)
        {
            m_canvas.sortingOrder = uiOrder;
        }

        private void PrivNotifyChildWidgetsInitialize()
        {
            for (int i = 0; i < m_listChildWidget.Count; i++)
            {
                m_listChildWidget[i].InterUIWidgetInitialize(this);
            }
        }

        private void PrivNotifyChildWidgetsInitializePost()
        {
            for (int i = 0; i < m_listChildWidget.Count; i++)
            {
                m_listChildWidget[i].InterUIWidgetInitializePost(this);
            }
        }

        private void PrivNotifyChildWidgetsFrameShow()
        {
            for (int i = 0; i < m_listChildWidget.Count; i++)
            {
                m_listChildWidget[i].InterUIWidgetParentFrameShow();
            }
        }
        private void PrivNotifyChildWidgetsFrameHide()
        {
            for (int i = 0; i < m_listChildWidget.Count; i++)
            {
                m_listChildWidget[i].InterUIWidgetParentFrameHide();
            }
        }

        //------------------------------------------------------
        protected virtual void OnUIFrameInitialize() { }
        protected virtual void OnUIFrameInitializePost() { }
        protected virtual void OnUIFrameShow() { }
        protected virtual void OnUIFrameHide() { }
    }
}
