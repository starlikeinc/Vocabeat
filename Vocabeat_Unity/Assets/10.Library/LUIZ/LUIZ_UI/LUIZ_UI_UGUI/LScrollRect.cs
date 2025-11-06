using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LUIZ.UI
{
    public class LScrollRect : ScrollRect
    {
        private bool m_isScrollDisabled = false;            public bool IsScrollDisabled() { return m_isScrollDisabled; } //스크롤 기능을 비활성화 한 상태

        private bool m_isDragging = false;                  public bool IsDragging() { return m_isDragging;  }

        public event Action OnBeginDragEvent = null;
        public event Action OnDragEvent      = null;
        public event Action OnEndDragEvent   = null;

        private ScrollRect m_parentScrollRect = null;

        //-------------------------------------------------------------------------------------
        public void DoScrollEnable()
        {
            m_isScrollDisabled = false;
        }

        public void DoScrollDisable()
        {
            m_isScrollDisabled = true;
        }

        //---------------------------------------------------------------------------------------
        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            m_isDragging = true;
            m_parentScrollRect?.OnBeginDrag(eventData);
            OnBeginDragEvent?.Invoke();
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            m_parentScrollRect?.OnDrag(eventData);
            OnDragEvent?.Invoke();
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            m_isDragging = false;
            m_parentScrollRect?.OnEndDrag(eventData);
            OnEndDragEvent?.Invoke();
        }

        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            base.OnInitializePotentialDrag(eventData);
            m_parentScrollRect?.OnInitializePotentialDrag(eventData);
        }

        public override void OnScroll(PointerEventData data)
        {
            if (m_isScrollDisabled == false)
            {
                base.OnScroll(data);
            }
        }

        //-------------------------------------------------------------------------------------
        public override void Rebuild(CanvasUpdate executing)
        {
            base.Rebuild(executing);

            if (verticalScrollbar is LScrollBar vertScrollBar)
            {
                if (vertScrollBar.IsFixedSize())
                    vertScrollBar.InterScrollRectRebuild();
            }
            else if (horizontalScrollbar is LScrollBar horScrollbar)
            {
                if (horScrollbar.IsFixedSize())
                    horScrollbar.InterScrollRectRebuild();
            }
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (verticalScrollbar is LScrollBar vertScrollBar)
            {
                if (vertScrollBar.IsFixedSize())
                    vertScrollBar.InterScrollRectLateUpdate();
            }
            else if (horizontalScrollbar is LScrollBar horScrollbar)
            {
                if (horScrollbar.IsFixedSize())
                    horScrollbar.InterScrollRectLateUpdate();
            }
        }

        //-------------------------------------------------------------------------------------
        internal void InterFindParentScrollRect()
        {
            bool isParentScrollFound = false;
            Transform parentTransform = gameObject.transform;

            while (parentTransform != null)
            {
                parentTransform = parentTransform.parent;
                m_parentScrollRect = parentTransform.gameObject.GetComponent<ScrollRect>();
                if (m_parentScrollRect != null)
                {
                    isParentScrollFound = true;
                    break;
                }
            }

            if (isParentScrollFound == false)
            {
                Debug.LogWarning("[LScrollRect] parent scroll not found!!");
            }
        }
    }
}
