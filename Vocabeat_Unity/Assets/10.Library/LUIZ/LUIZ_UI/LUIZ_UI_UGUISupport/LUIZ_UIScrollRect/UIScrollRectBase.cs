using System;
using UnityEngine;
using UnityEngine.UI;

namespace LUIZ.UI
{
    [RequireComponent(typeof(LScrollRect))]
    public abstract class UIScrollRectBase : UITemplateBase
    {
        public enum EInitScrollPosType
        {
            Keep, //이전 스크롤 위치 유지
            ResetToZero //(0,0) 리셋
        }

        [Header("[ UIScrollRect ]")]
        
        [Tooltip("다른 스크롤 하위에 들어가있는 자식 스크롤일 경우 true로 체크해야한다")]
        [SerializeField]
        private bool IsNestedScroll = false;

        protected LScrollRect m_scrollRect = null;
        protected bool m_isProgrammaticAdjust = false;

        //-------------------------------------------------------------------
        protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
        {
            base.OnUIWidgetInitialize(parentFrame);

            m_scrollRect = GetComponent<LScrollRect>();

            if (m_scrollRect.viewport == null)
            {
                m_scrollRect.viewport = m_scrollRect.transform as RectTransform;
            }

            if (IsNestedScroll)
            {
                m_scrollRect.InterFindParentScrollRect();
            }
        }

        protected override void ProtChangeSortOrder()
        {
            if (IsNestedScroll == false)
            {
                base.ProtChangeSortOrder();
            }
            else
            {
                Canvas widgetCanvas = GetWidgetCanvas();
                widgetCanvas.overrideSorting = false;
            }
        }
        
        //-------------------------------------------------------------------
        protected void ProtUIScrollPositionReset()
        {
            m_scrollRect.content.anchoredPosition = Vector2.zero;
            OnAfterAnchoredPositionChanged();
        }

        //원하는 위치로 세팅
        protected void ProtSetAnchoredPosition(Vector2 pos, bool andNotify = true)
        {
            m_scrollRect.content.anchoredPosition = pos;
            if (andNotify) OnAfterAnchoredPositionChanged();
        }

        ///<summary>
        ///공통 init 시퀀스 실행 도우미
        ///위치 적용(ResetToZero면 0,0임)
        ///컨텐츠 사이즈, 아이템 준비
        ///위치 기준 처리 후에 OnAfterAnchoredPositionChanged 호출
        ///허용 범위 클램프, 다시 후처리
        ///</summary>
        protected void ProtRunReinit(EInitScrollPosType posOption, Action recalcBody)
        {
            float keepH = m_scrollRect.horizontal ? m_scrollRect.horizontalNormalizedPosition : 0f;
            float keepV = m_scrollRect.vertical   ? m_scrollRect.verticalNormalizedPosition   : 1f;
            
            m_isProgrammaticAdjust = true;
            
            if (posOption == EInitScrollPosType.ResetToZero)
                ProtSetAnchoredPosition(Vector2.zero, andNotify: false);

            recalcBody?.Invoke();
            if (posOption == EInitScrollPosType.Keep)
            {
                m_scrollRect.StopMovement();
                m_scrollRect.velocity = Vector2.zero;
                if (m_scrollRect.horizontal) m_scrollRect.horizontalNormalizedPosition = keepH;
                if (m_scrollRect.vertical)   m_scrollRect.verticalNormalizedPosition   = keepV;

            }
            
            OnAfterAnchoredPositionChanged(); //현재 위치 기준 패딩/인덱스 반영
            ClampAnchoredPositionToView(andNotify: true);
            
            m_isProgrammaticAdjust = false;
            OnAfterReinitPositionSettled();
        }
        
        //---------------------------------------------------
        private void ForceRebuild()
        {
            if (m_scrollRect != null && m_scrollRect.content != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(m_scrollRect.content);
        }

        //뷰포트 허용 범위로 클램프
        private void ClampAnchoredPositionToView(bool andNotify = true)
        {
            var vp = m_scrollRect.viewport != null
                ? m_scrollRect.viewport
                : (RectTransform)m_scrollRect.transform;

            var content = m_scrollRect.content;

            LayoutRebuilder.ForceRebuildLayoutImmediate(content);

            float cw = content.rect.width;
            float ch = content.rect.height;
            float vw = vp.rect.width;
            float vh = vp.rect.height;

            Vector2 pos = content.anchoredPosition;

            //TODO : 현재 pivot (0,1) 가정했음 x는 [-maxRight, 0], y는 [0, maxDown]
            //나중에 다른 피봇들도 지원할것..
            if (m_scrollRect.horizontal)
            {
                float maxRight = Mathf.Max(0f, cw - vw);
                pos.x = Mathf.Clamp(pos.x, -maxRight, 0f);
            }
            if (m_scrollRect.vertical)
            {
                float maxDown = Mathf.Max(0f, ch - vh);
                pos.y = Mathf.Clamp(pos.y, 0f, maxDown);
            }

            if (pos != content.anchoredPosition)
            {
                content.anchoredPosition = pos;
                if (andNotify) OnAfterAnchoredPositionChanged();
            }
        }
        //-------------------------------------------------------------------
        ///<summary>
        ///anchoredPosition가 바뀌었을 때 패딩/인덱스/아이템 배치 등을 갱신
        ///</summary>
        protected virtual void OnAfterAnchoredPositionChanged() { }
        ///<summary>
        ///재초기화가 끝나 "최종 위치가 확정"된 직후 한 번 호출되는 훅.
        ///(ex. 가상 스크롤은 여기서 현재 뷰 기준으로 아이템 채움)
        ///</summary>
        protected virtual void OnAfterReinitPositionSettled() { }
    }
}