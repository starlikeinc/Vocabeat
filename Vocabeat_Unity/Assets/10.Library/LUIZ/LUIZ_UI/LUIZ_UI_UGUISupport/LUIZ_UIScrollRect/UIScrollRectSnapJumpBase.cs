using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LUIZ.UI
{
    //스냅, 점프 스크롤. 가장 가까운 아이템에 자동으로 스냅되거나, 원하는 아이템에 자동으로 스크롤이 이동하는 기능을 지원한다.
    //[ 주의!! ]Viewport와 Content의 피봇을 (0, 1) 로 해야함
    public abstract class UIScrollRectSnapJumpBase : UIScrollRectBase
    {
        private static IScrollSnapJumpHelper c_defaultScrollHelper = new UIScrollSnapHelper_Default();

        //---------------------------------------------------------------------

        [Header("[ ScrollSnap ]")]
        [SerializeField] private bool IsScrollSnap = false;
        [SerializeField] private float SnapTime = 2f;
        [SerializeField] private float SnapUnderVelocity = 500f;
        [SerializeField] private AnimationCurve SnapCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField, Range(0, 1)] private float SnapOffsetX = 0f;
        [SerializeField, Range(0, 1)] private float SnapOffsetY = 0f;

        [Header("[ ScrollJump ]")]
        [SerializeField] private float JumpTime = 2f;       public float GetJumpTime() { return JumpTime; }

        [SerializeField] private AnimationCurve JumpCurve = AnimationCurve.Linear(0, 0, 1, 1);

        //----------------------------------------------------------------------
        private List<UITemplateItemBase> m_listScrollItems = null;

        private IScrollSnapJumpHelper m_curSnapHelper = null;

        private Vector2 m_curSnapOffset = Vector2.zero;
        private Coroutine m_snapJumpMoveCO = null;
        private Coroutine m_snapCheckCO = null;

        //----------------------------------------------------------------------
        private UITemplateItemBase m_jumpItem = null;

        private bool m_isJumping = false;

        //----------------------------------------------------------------------
        protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
        {
            base.OnUIWidgetInitialize(parentFrame);

            m_scrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            m_curSnapHelper = c_defaultScrollHelper;

            if(IsScrollSnap == true)
            {
                m_listScrollItems = new List<UITemplateItemBase>();

                m_scrollRect.OnBeginDragEvent += PrivResetSnapJump;

                m_scrollRect.OnEndDragEvent += () =>
                {
                    m_snapCheckCO = StartCoroutine(PrivCOSnapScrollCheck());
                };
            }
        }

        protected override void OnUITemplateRequestItem(UITemplateItemBase item)
        {
            base.OnUITemplateRequestItem(item);

            m_listScrollItems?.Add(item);
        }

        protected override void OnUITemplateReturnItem(UITemplateItemBase item)
        {
            base.OnUITemplateReturnItem(item);

            m_listScrollItems?.Remove(item);
        }

        //----------------------------------------------------------------------
        internal void InterForceCheckSnap()
        {
            if (m_snapCheckCO != null)
                StopCoroutine(m_snapCheckCO);

            m_snapCheckCO = StartCoroutine(PrivCOSnapScrollCheck());
        }

        //----------------------------------------------------------------------
        public void DoScrollSnapJumpToItem(UITemplateItemBase jumpItem, Vector2 jumpOffset, float jumpTime = 0)
        {
            if(jumpTime != 0)
            {
                JumpTime = jumpTime;
            }

            PrivResetSnapJump();

            m_jumpItem = jumpItem;
            Vector2 contentPos = GetUIPositionLeftTop(m_scrollRect.content);
            Vector2 destPos = PrivGetItemClosetPosFromVector(jumpItem, contentPos, jumpOffset);
            m_snapJumpMoveCO = StartCoroutine(PrivCOSnapJumpMoveScroll(contentPos, destPos, false));
        }

        public void DoScrollSnapJumpStop()
        {
            m_jumpItem = null;
            PrivResetSnapJump();
        }

        /// <summary>
        /// 커스텀 스크롤 핼퍼 설정. 디폴트로 되돌리고자하면 null을 보낼것
        /// </summary>
        public void DoScrollSnapJumpCustomHelper(IScrollSnapJumpHelper helper)
        {
            if(helper != null)
            {
                m_curSnapHelper = helper;
            }
            else
            {
                m_curSnapHelper = c_defaultScrollHelper;
            }
        }

        //----------------------------------------------------------------------
        private void PrivUpdateSnapOffset()
        {
            m_curSnapOffset.x = m_scrollRect.viewport.rect.width * SnapOffsetX;
            m_curSnapOffset.y = m_scrollRect.viewport.rect.height * SnapOffsetY * -1; //pivot이 0,1 고정이므로 y축 값은 반드시 음수임
            //TODO : 다양한 pivot에 대응하도록 수정?
        }

        private bool PrivCheckVelocityUnder()
        {
            bool isSnap = false;

            Vector2 velocity = m_scrollRect.velocity;

            if (m_scrollRect.horizontal == true)
            {
                if (Mathf.Abs(velocity.x) < SnapUnderVelocity)
                {
                    isSnap = true;
                }
            }

            if (m_scrollRect.vertical == true)
            {
                if (Mathf.Abs(velocity.y) < SnapUnderVelocity)
                {
                    isSnap = true;
                }
            }

            m_scrollRect.velocity = velocity;

            if(isSnap)
            {
                UITemplateItemBase item = PrivGetSnapItemByViewportOffset(m_curSnapOffset);

                if (item == null)
                    return false;

                Vector2 contentPos = GetUIPositionLeftTop(m_scrollRect.content);
                Vector2 destPos = PrivGetItemClosetPosFromVector(item, contentPos, Vector2.zero);

                //스크롤을 정지시키고 별도의 스냅 스크롤로 이동
                m_scrollRect.velocity = Vector2.zero;
                m_snapJumpMoveCO = StartCoroutine(PrivCOSnapJumpMoveScroll(contentPos, destPos, true));
            }

            return isSnap;
        }

        private Vector2 PrivGetItemClosetPosFromVector(UITemplateItemBase item, Vector2 contentPos, Vector2 offset)
        {
            Vector2 itemSize = item.GetUISize();

            Vector2 itemTopLeftPos = (item.GetUIPositionLeftTop() + contentPos);
            Vector2 itemBottomRightPos = new Vector2(itemTopLeftPos.x + itemSize.x, itemTopLeftPos.y - itemSize.y);

            Vector2 offsetPos = PrivGetClosetVector(itemTopLeftPos, itemBottomRightPos);

            Vector2 destPos = m_curSnapOffset - offsetPos + contentPos;

            return destPos + offset;
        }

        //----------------------------------------------------------------------
        private IEnumerator PrivCOSnapScrollCheck()
        {
            PrivUpdateSnapOffset();

            while (PrivCheckVelocityUnder() == false)
            {
                PrivUpdateSnapOffset();
                yield return null;
            }
        }

        private IEnumerator PrivCOSnapJumpMoveScroll(Vector2 startPos, Vector2 destPos, bool isSnap)
        {
            float curSnapTime = 0;

            float maxTime = isSnap ? SnapTime : JumpTime;
            AnimationCurve curve = isSnap ? SnapCurve : JumpCurve;

            if(isSnap == true)
            {
                OnUIScrollSnapStart();
            }

            while (curSnapTime < maxTime)
            {
                curSnapTime += Time.deltaTime;

                float timePercent = Mathf.Clamp01(curSnapTime / maxTime);
                float curveValue = curve.Evaluate(timePercent);

                Vector2 nextPos = m_curSnapHelper.GetNextMovePosition(startPos, destPos, timePercent, curveValue);

                m_scrollRect.content.anchoredPosition = nextPos;

                yield return null;
            }

            m_scrollRect.content.anchoredPosition = destPos;

            if (isSnap == true)
                PrivFinishScrollSnap();
            else
                PrivFinishScrollJump();
        }

        private UITemplateItemBase PrivGetSnapItemByViewportOffset(Vector2 viewportOffset)
        {
            if (m_isJumping == true)
                return null;

            UITemplateItemBase findItem = null;

            Vector2 contentPosition = GetUIPositionLeftTop(m_scrollRect.content);
            foreach(UITemplateItemBase item in m_listScrollItems)
            {
                Vector2 localPos = item.GetUIPositionLeftTop() + contentPosition;
                Vector2 size = item.GetUISize();

                Vector2 start = localPos;
                Vector2 end = new Vector2(start.x + size.x, start.y - size.y);

                if(viewportOffset.x >= start.x && viewportOffset.x < end.x)
                {
                    if(viewportOffset.y <= start.y && viewportOffset.y > end.y)
                    {
                        findItem = item;
                        break;
                    }
                }
            }

            return findItem;
        }

        private Vector2 PrivGetClosetVector(Vector2 start, Vector2 end)
        {
            Vector2 closeVec = Vector2.zero;

            float startDis = 0;
            float endDis = 0;

            if (m_scrollRect.horizontal == true && m_scrollRect.vertical == true)
            {
                startDis = Vector2.Distance(m_curSnapOffset, start);
                endDis = Vector2.Distance(m_curSnapOffset, end);

                closeVec = startDis < endDis ? start : end;
            }
            else if(m_scrollRect.horizontal == true)
            {
                startDis = m_curSnapOffset.x - start.x;
                endDis = end.x - m_curSnapOffset.x;

                closeVec = new Vector2(startDis < endDis ? start.x : end.x, 0);
            }
            else if (m_scrollRect.vertical == true)
            {
                startDis = start.y - m_curSnapOffset.y;
                endDis = m_curSnapOffset.y - end.y;

                closeVec = new Vector2(0, startDis < endDis ? start.y : end.y);
            }

            return closeVec;
        }

        private void PrivFinishScrollSnap()
        {
            PrivResetSnapJump();

            OnUIScrollSnapFinish();
        }

        private void PrivFinishScrollJump()
        {
            PrivResetSnapJump();

            OnUIScrollJumpFinish(m_jumpItem);
            m_jumpItem = null;
        }

        //----------------------------------------------------------------------
        private void PrivResetSnapJump()
        {
            if (m_snapJumpMoveCO != null)
                StopCoroutine(m_snapJumpMoveCO);

            if (m_snapCheckCO != null)
                StopCoroutine(m_snapCheckCO);

            m_snapJumpMoveCO = null;
            m_snapCheckCO = null;
        }

        //----------------------------------------------------------------------
        protected virtual void OnUIScrollSnapStart() { }
        protected virtual void OnUIScrollSnapFinish() { }
        protected virtual void OnUIScrollJumpFinish(UITemplateItemBase jumpItem) { }
    }
}
