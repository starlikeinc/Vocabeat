using PlasticGui.WorkspaceWindow;
using System.Collections.Generic;
using UnityEngine;

namespace LUIZ.UI
{
    public abstract class UIAnimatedGaugeSingleBase : UIAnimatedGaugeBase
    {
        [SerializeField, Range(0, 0.1f)]    private float FrontFillOffset = 0;
        [SerializeField, Range(0.9f, 1.0f)] private float EndFillOffset = 1;

        //---------------------------------------------------
        protected void ProtUIAnimatedGaugeSingleReset(float maxValue, float currentValue)
        {
            InterUIAnimatedGaugeReset(maxValue, currentValue);
        }

        //---------------------------------------------------
        protected override void OnUIAnimatedGaugeReset(float maxValue, float currentValue)
        {
            base.OnUIAnimatedGaugeReset(maxValue, currentValue);

            PrivCalculateAndApplyFill(maxValue, currentValue);
        }

        protected override void OnUIAnimatedGaugeMoveValueStart(float currentValue, float destValue)
        {
            base.OnUIAnimatedGaugeMoveValueStart(currentValue, destValue);

            PrivCalculateAndApplyFill(GetUIAnimatedGaugeMaxValue(), currentValue);
        }

        protected override void OnUIAnimatedGaugeMoveValue(float currentValue)
        {
            base.OnUIAnimatedGaugeMoveValue(currentValue);

            PrivCalculateAndApplyFill(GetUIAnimatedGaugeMaxValue(), currentValue);
        }

        protected override void OnUIAnimatedGaugeMoveValueEnd(float endValue)
        {
            base.OnUIAnimatedGaugeMoveValueEnd(endValue);

            PrivCalculateAndApplyFill(GetUIAnimatedGaugeMaxValue(), endValue);
        }

        //---------------------------------------------------
        private void PrivCalculateAndApplyFill(float maxValue, float currentValue)
        {
            float fillPercent = PrivCalculateFillPercentage(maxValue, currentValue);
            PrivApplyFill(fillPercent);
        }

        private float PrivCalculateFillPercentage(float maxValue, float currentValue)
        {
            //오프셋 까지 적용해서 리턴
            float percent = currentValue / maxValue;
            float fillPercent = Mathf.Lerp(FrontFillOffset, EndFillOffset, percent);

            return fillPercent;
        }

        private void PrivApplyFill(float fillValue)
        {
            GaugeSlider.value = fillValue;
        }

        //---------------------------------------------------
    }
}
