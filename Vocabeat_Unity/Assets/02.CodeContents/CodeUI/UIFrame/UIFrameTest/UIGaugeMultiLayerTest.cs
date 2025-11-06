using LUIZ.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGaugeMultiLayerTest : UIAnimatedGaugeMultiLayerBase
{
    [Header("Test")]
    [SerializeField] private float SingleLayerValue = 300;
    [SerializeField] private float ResetValue;
    [SerializeField] private float MoveValue;

    [SerializeField] private int LayerCount;
    [SerializeField] private List<Color> LayerColors;

    [SerializeField] private Image FillImage;

    //-----------------------------------------------------
    protected override float OnUIAnimatedGaugeGetLayerMaxValue(int layerIndex)
    {
        return SingleLayerValue;
    }

    protected override void OnUIAnimatedGaugeMultiLayerChange(int currentIndex)
    {
        base.OnUIAnimatedGaugeMultiLayerChange(currentIndex);
        FillImage.color = LayerColors[currentIndex];
    }

    //-----------------------------------------------------
    public void HandleOnBtnGaugeReset()
    {
        ProtUIAnimatedGaugeMultiReset(LayerCount, ResetValue);
    }

    public void HandleOnBtnGaugeMove()
    {
        ProtUIAnimatedGaugeMoveValue(MoveValue);
    }
}
