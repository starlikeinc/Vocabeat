using System;
using System.Collections.Generic;
using UnityEngine;

namespace LUIZ.UI
{
    public abstract class UIAnimatedGaugeMultiLayerBase : UIAnimatedGaugeBase
    {
        [Tooltip("실제 값은 0이더라도 UI상 Fill이 남아있도록 하여 가독성을 향상 시킬 수 있음")]
        [SerializeField, Range(0, 0.1f)]    private float FrontFillOffset = 0;
        [Tooltip("실제 값은 Max더라도 UI상 Fill이 남아있도록 하여 가독성을 향상 시킬 수 있음")]
        [SerializeField, Range(0.9f, 1.0f)] private float EndFillOffset = 1;

        private readonly List<float> m_listAccumulatedMax = new();//누적값 리스트
        
        private int m_layerCount = 0;
        
        private float m_totalMaxValue = 0;
        private int m_currentGaugeIndex = 0;

        private bool m_hasReachedFull = false;
        
        //--------------------------------------------------------
        /// <summary>
        /// 레이어 개수와 현재 값으로 게이지 초기화. 최대값은 하위 클래스에서 제공.
        /// </summary>
        protected void ProtUIAnimatedGaugeMultiReset(int layerCount, float currentValue)
        {
            if (layerCount <= 0)
            {
                Debug.LogError("[UIAnimatedGaugeMultiLayerBase] layerCount must be greater than zero.");
                return;
            }

            m_layerCount = layerCount;
            m_totalMaxValue = 0;
            m_listAccumulatedMax.Clear();

            for (int i = 0; i < m_layerCount; i++)
            {
                float max = OnUIAnimatedGaugeGetLayerMaxValue(i);
                if (max <= 0)
                {
                    Debug.LogWarning($"[UIAnimatedGaugeMultiLayerBase] Layer {i} has invalid max value: {max}. It must be > 0.");
                    max = 1f;//걍 1더함..
                }

                m_totalMaxValue += max;
                m_listAccumulatedMax.Add(m_totalMaxValue);
            }

            InterUIAnimatedGaugeReset(m_totalMaxValue, currentValue);
        }

        //--------------------------------------------------------
        protected override void OnUIAnimatedGaugeReset(float maxValue, float currentValue)
        {
            base.OnUIAnimatedGaugeReset(maxValue, currentValue);
            
            PrivSettingCurrentGaugeIndex(currentValue, true);
            PrivCalculateAndApplyFill(currentValue);
        }

        protected override void OnUIAnimatedGaugeMoveValueStart(float currentValue, float destValue)
        {
            base.OnUIAnimatedGaugeMoveValueStart(currentValue, destValue);
        }

        protected override void OnUIAnimatedGaugeMoveValue(float currentValue)
        {
            base.OnUIAnimatedGaugeMoveValue(currentValue);

            PrivSettingCurrentGaugeIndex(currentValue);
            PrivCalculateAndApplyFill(currentValue);
        }

        protected override void OnUIAnimatedGaugeMoveValueEnd(float endValue)
        {
            base.OnUIAnimatedGaugeMoveValueEnd(endValue);

            PrivSettingCurrentGaugeIndex(endValue);
            PrivCalculateAndApplyFill(endValue);
        }

        //--------------------------------------------------------
        /// <summary>
        /// 현재 누적 값 기준으로 몇 번째 레이어에 속하는지 판정
        /// </summary>
        private void PrivSettingCurrentGaugeIndex(float currentValue, bool isReset = false)
        {
            int currentIndex = PrivBinarySearchGaugeIndex(currentValue);

            if (isReset || m_currentGaugeIndex != currentIndex)
            {
                m_currentGaugeIndex = currentIndex;
                OnUIAnimatedGaugeMultiLayerChange(m_currentGaugeIndex);
            }
        }

        /// <summary>
        /// 현재 레이어에서의 상대적인 fill 값을 계산하여 슬라이더에 적용
        /// </summary>
        private void PrivCalculateAndApplyFill(float currentValue)
        {
            float accumulated = (m_currentGaugeIndex == 0) ? 0 : m_listAccumulatedMax[m_currentGaugeIndex - 1];
            float currentLayerMax = OnUIAnimatedGaugeGetLayerMaxValue(m_currentGaugeIndex);
            float currentLayerValue = currentValue - accumulated;

            float fillPercent = PrivCalculateFillPercentage(currentLayerMax, currentLayerValue);
            PrivApplyFill(fillPercent);
            
            //마지막 레이어에서만 최대값 도달 여부를 확인
            if (m_currentGaugeIndex == m_layerCount - 1)
            {
                if (Mathf.Approximately(currentValue, m_totalMaxValue))
                {
                    if (!m_hasReachedFull)
                    {
                        m_hasReachedFull = true;
                        OnUIAnimatedGaugeFullyFilled();
                    }
                }
                else m_hasReachedFull = false;
            }
            else m_hasReachedFull = false; //다른 레이어로 내려온 경우도 초기화
        }

        private float PrivCalculateFillPercentage(float maxValue, float currentValue)
        {
            float percent = currentValue / maxValue;
            return Mathf.Lerp(FrontFillOffset, EndFillOffset, percent);
        }

        private void PrivApplyFill(float fillValue)
        {
            GaugeSlider.value = fillValue;
        }
        
        private int PrivBinarySearchGaugeIndex(float currentValue)//현제 레이어 계산을 빠르게 하기 위한 이분탐색....
        {
            int left = 0;
            int right = m_listAccumulatedMax.Count - 1;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                float midValue = m_listAccumulatedMax[mid];

                if (currentValue < midValue)
                {
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }

            //left가 가리키는 위치가 currentValue보다 큰 최초 인덱스임...
            return Mathf.Clamp(left, 0, m_listAccumulatedMax.Count - 1);
        }

        //======================================================
        /// <summary> 하위 클래스에서 현재 레이어의 최대값을 동적으로 제공, layerIndex = 0 부터 시작 </summary>
        protected abstract float OnUIAnimatedGaugeGetLayerMaxValue(int layerIndex);
        /// <summary> 마지막 레이어의 최대값에 도달하였을 때 호출. 값이 내려갔다가 다시 도달되어도 호출됨 </summary>
        protected virtual void OnUIAnimatedGaugeFullyFilled(){}
        /// <summary> 레이어 전환 시 호출됨 </summary>
        protected virtual void OnUIAnimatedGaugeMultiLayerChange(int currentIndex) { }
    }
}
