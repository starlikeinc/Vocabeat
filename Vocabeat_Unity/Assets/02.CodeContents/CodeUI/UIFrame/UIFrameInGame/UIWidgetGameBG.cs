using LUIZ.UI;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class UIWidgetGameBG : UIWidgetBase
{
    [Header("배경")]
    [SerializeField] private Image _gameBG;
    [SerializeField] private Image _bgDimmer;

    [Header("디머")]
    [SerializeField] private float _bgDimAlphaValue;
    [SerializeField] private float _bgDimDuration;

    // ========================================
    public void DoUIGameBGSetting(Sprite bg, Action onDimComplete)
    {
        _gameBG.overrideSprite = bg;

        void OnDimComplete() => onDimComplete?.Invoke();

        _bgDimmer.DOFade(_bgDimAlphaValue, _bgDimDuration)
                 .SetEase(Ease.OutSine)
                 .OnComplete(OnDimComplete);
    }

    // ========================================   
    public void SetDimmerAlphaForce(float alphaValue)
    {
        Color color = _bgDimmer.color;
        color.a = alphaValue;

        _bgDimmer.color = color;
    }
}
