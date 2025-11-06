using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LUIZ.UI
{
    ///<summary>
    ///LongPressStartOffset 이후부터 LongPressTick이 매 프레임 발생
    ///포인터가 영역 밖으로 나가면 즉시 취소
    ///롱프레스 중 영역 위에서 손을 떼면 일반 Click은 막고 LongPressClick만 발생
    ///</summary>
    public class LButtonLongPress : LButton, IPointerExitHandler
    {
        [Tooltip("해당 초 만큼 지난 이후부터 롱클릭으로 간주 시작")]
        [SerializeField, Range(0f, 4f)] private float LongPressStartOffset = 1.0f;

        [Header("Long Press Events")]
        public UnityEvent OnLongPressBegin;    // 롱 프레스 오프셋 도달 시 호출
        public UnityEvent OnLongPressTick;     // 롱프레스 유지 동안 매 프레임 호출
        public UnityEvent OnLongPressClick;    // 롱프레스 상태에서 "영역 위에서" 손을 뗀 경우
        public UnityEvent OnLongPressCancel;   // 롱프레스 상태에서 영역 밖으로 나가거나 취소된 경우

        private Coroutine m_coLongPress = null;
        private bool m_isPointerDown = false;
        private bool m_isLongPressActive = false;
        private bool m_wasCanceledByExit = false;

        private bool m_consumeNextClick;
        
        private PointerEventData m_activePED;

        //----------------------------------------------------
        //외부에서 한 번만 호출하면 현재 레이캐스트/드래그/클릭 체인을 즉시 끊음
        public void ForceCancelCurrentPointerChain()
        {
            var ped = m_activePED;
            if (ped == null) return;
            
            if (ped.dragging && ped.pointerDrag != null)
            {
                ExecuteEvents.Execute(ped.pointerDrag, ped, ExecuteEvents.endDragHandler);
            }
            ped.dragging = false;
            ped.pointerDrag = null;

            if (ped.pointerPress != null)
            {
                ExecuteEvents.Execute(ped.pointerPress, ped, ExecuteEvents.pointerUpHandler);
            }
            ped.pointerPress = null;
            ped.rawPointerPress = null;
            ped.eligibleForClick = false;
            
            ped.pointerEnter = null;
            ped.Use();
            m_consumeNextClick = true;
        }
        
        //----------------------------------------------------
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            if (!IsActive() || !IsInteractable())
                return;
            
            m_activePED = eventData;
            m_isPointerDown = true;
            m_isLongPressActive = false;
            m_wasCanceledByExit = false;
            m_consumeNextClick = false;
            
            if (m_coLongPress != null)
                StopCoroutine(m_coLongPress);
            m_coLongPress = StartCoroutine(CoLongPress());
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            m_isPointerDown = false;
            
            if (m_coLongPress != null)
            {
                StopCoroutine(m_coLongPress);
                m_coLongPress = null;
            }

            //일반 클릭 대신 LongPressClick을 발생시키고 일반 클릭은 OnPointerClick에서 차단
            if (m_isLongPressActive && !m_wasCanceledByExit)
            {
                bool releasedOverSelf = eventData.pointerCurrentRaycast.gameObject == gameObject ||
                                        (eventData.pointerCurrentRaycast.gameObject != null &&
                                         eventData.pointerCurrentRaycast.gameObject.transform.IsChildOf(transform));

                if (releasedOverSelf)
                {
                    m_consumeNextClick = true;
                    OnLongPressClick?.Invoke();
                }
            }
            
            m_isLongPressActive = false;
            m_wasCanceledByExit = false;
            m_activePED = null;
        }

        /// <summary>
        /// 기본 버튼의 Click 호출 시점
        /// 롱프레스가 활성화된 상태였다면 일반 Click을 막아 LongPressClick만 동작
        /// </summary>
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (m_consumeNextClick)
            {
                m_consumeNextClick = false;
                return;
            }
            
            //롱프레스 중이었거나 Exit로 취소된 경우도 일반 Click 막음..
            if (m_isLongPressActive || m_wasCanceledByExit)
                return;

            base.OnPointerClick(eventData);
        }

        /// <summary>
        /// 누르고 있는 중 영역 밖으로 나가면 즉시 취소
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!m_isPointerDown)
                return;

            m_wasCanceledByExit = true;
            m_isPointerDown = false;

            if (m_coLongPress != null)
            {
                StopCoroutine(m_coLongPress);
                m_coLongPress = null;
            }

            if (m_isLongPressActive)
            {
                OnLongPressCancel?.Invoke();
            }

            m_isLongPressActive = false;
            //시각적 상태 복구
            DoStateTransition(SelectionState.Normal, instant: true);
            
            m_activePED = null;
        }

        private IEnumerator CoLongPress()
        {
            float t = 0f;

            while (m_isPointerDown && t < LongPressStartOffset)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!m_isPointerDown)
            {
                m_coLongPress = null;
                yield break;
            }

            m_isLongPressActive = true;
            OnLongPressBegin?.Invoke();

            while (m_isPointerDown)
            {
                OnLongPressTick?.Invoke();
                yield return null;
            }

            m_coLongPress = null;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            m_isPointerDown = false;
            m_isLongPressActive = false;
            m_wasCanceledByExit = false;
            m_consumeNextClick = false;
            
            if (m_coLongPress != null)
            {
                StopCoroutine(m_coLongPress);
                m_coLongPress = null;
            }
            m_activePED = null;
        }
    }
}
