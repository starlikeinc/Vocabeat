using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LUIZ.UI
{
    //게이지의 value 만 조정하는 레이어. 실제 유아이 gauge 값은 하위 자식에서 조정하도록 한다.
    [RequireComponent(typeof(Slider))]
    public abstract class UIAnimatedGaugeBase : MonoBehaviour
    {
        [Header("[ AnimatedGauge ]")]
        [SerializeField] protected Slider GaugeSlider;
        
        [SerializeField] private float          MoveTime  = 2f;
        [SerializeField] private AnimationCurve MoveCurve = AnimationCurve.Linear(0, 0, 1f, 1f);
        
        private float m_maxValue = 0;           public float GetUIAnimatedGaugeMaxValue() { return m_maxValue; }
        private float m_minValue = 0;           //TODO:minValue도 설정 가능하도록 해야할 수 도 있음      
        private float m_currentValue = 0;       public float GetUIAnimatedGaugeCurrentValue() { return m_currentValue; }

        private Coroutine m_moveHandle = null;

        //---------------------------------------------------
        internal void InterUIAnimatedGaugeReset(float maxValue, float currentValue)
        {
            if (maxValue <= 0f)
            {
                Debug.LogError("[UIAnimatedGaugeBase] maxValue must be greater than 0");
                maxValue = 1f; // 최소 유효값 강제
            }

            PrivStopMoveCoroutine();

            m_maxValue = maxValue;
            m_minValue = 0f;

            m_currentValue = Mathf.Clamp(currentValue, m_minValue, m_maxValue);

            OnUIAnimatedGaugeReset(m_maxValue, m_currentValue);
        }

        protected void ProtUIAnimatedGaugeAddRemoveValue(float addRemoveValue)
        {
            float destValue = Mathf.Clamp(m_currentValue + addRemoveValue, 0, m_maxValue);
            ProtUIAnimatedGaugeMoveValue(destValue);
        }

        protected void ProtUIAnimatedGaugeMoveValue(float destValue)
        {
            PrivStopMoveCoroutine();
            //TODO: 나중에 그냥 Update로 돌리는게 나을거 같음
            m_moveHandle = StartCoroutine(PrivCOMoveGaugeValue(destValue));
        }

        //---------------------------------------------------
        private IEnumerator PrivCOMoveGaugeValue(float destValue)
        {
            OnUIAnimatedGaugeMoveValueStart(m_currentValue, destValue);

            float currentTime = 0;
            float currentPercent = 0;
            float currentCurve = 0;

            float startValue = m_currentValue;

            while (currentTime <= MoveTime)
            {
                currentPercent = currentTime / MoveTime;
                currentCurve = MoveCurve.Evaluate(currentPercent);

                m_currentValue = Mathf.Lerp(startValue, destValue, currentCurve);

                OnUIAnimatedGaugeMoveValue(m_currentValue);

                currentTime += Time.deltaTime;
                yield return null;
            }

            m_currentValue = destValue;

            m_moveHandle = null;

            OnUIAnimatedGaugeMoveValueEnd(m_currentValue);
        }

        private void PrivStopMoveCoroutine()
        {
            if (m_moveHandle != null)
                StopCoroutine(m_moveHandle);
        }

        //---------------------------------------------------
        protected virtual void OnUIAnimatedGaugeReset(float maxValue, float currentValue) { }

        protected virtual void OnUIAnimatedGaugeMoveValueStart(float currentValue, float destValue) { }
        protected virtual void OnUIAnimatedGaugeMoveValue(float currentValue) { }
        protected virtual void OnUIAnimatedGaugeMoveValueEnd(float endValue) { }
    }
}
