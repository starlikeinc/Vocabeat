using System;
using DG.Tweening;
using LUIZ.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIFrameBlinder : UIFrameBase
{
    [SerializeField] private Image _blinder;

    [SerializeField] private float _duration;
    [SerializeField] private float _blindWait;

    protected override void OnUIFrameInitialize()
    {
        base.OnUIFrameInitialize();
        if (_blinder == null)
            _blinder = GetComponentInChildren<Image>();
        _blinder.DOFade(0f, 0f);
    }

    public void BlindWithNextStep(Action onNextStep)
    {
        void OnNextStep() => onNextStep?.Invoke();

        _blinder.DOFade(1f, _duration)
                .SetEase(Ease.InSine)
                .OnComplete(OnNextStep);

        DOVirtual.DelayedCall(_blindWait, () =>
        {
            _blinder.DOFade(0f, _duration)
                    .SetEase(Ease.OutSine);
        });
    }   
}
