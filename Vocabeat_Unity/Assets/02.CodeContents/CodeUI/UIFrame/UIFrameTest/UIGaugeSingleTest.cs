using UnityEngine;
using LUIZ.UI;

public class UIGaugeSingleTest : UIAnimatedGaugeSingleBase
{
    [Header("Test")]
    [SerializeField] private float MaxValue;
    [SerializeField] private float MoveValue;

    //-----------------------------------------------------
    public void HandleOnBtnGaugeReset()
    {
        ProtUIAnimatedGaugeSingleReset(MaxValue, 0);
    }

    public void HandleOnBtnGaugeMove()
    {
        ProtUIAnimatedGaugeMoveValue(MoveValue);
    }
}
