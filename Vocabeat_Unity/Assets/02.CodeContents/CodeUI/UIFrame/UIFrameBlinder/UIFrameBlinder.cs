using System;
using DG.Tweening;
using LUIZ.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIFrameBlinder : UIFrameBase
{
    [Header("Blind Options")]
    [SerializeField] private Image _blinder;

    [SerializeField] private float _duration;
    [SerializeField] private float _blindWait;

    private Sequence _blindSequence;

    private Action _onNextStep;

    protected override void OnUIFrameInitialize()
    {
        base.OnUIFrameInitialize();
        CacheSequence();
    }

    public void BlindWithNextStep(Action onNextStep)
    {
        _onNextStep = onNextStep;

        ResetSequenceTargets();

        _blindSequence.Restart();
    }   

    private void ResetSequenceTargets()
    {
        Color color = _blinder.color;
        color.a = 0f;
        _blinder.color = color;
    }

    private void CacheSequence()
    {
        _blindSequence = DOTween.Sequence()
        .SetAutoKill(false)
        .Pause();

        _blindSequence
            .Append(_blinder.DOFade(1f, _duration).SetEase(Ease.InSine))
            .AppendCallback(() => _onNextStep?.Invoke())
            .AppendInterval(_blindWait)
            .Append(_blinder.DOFade(0f, _duration).SetEase(Ease.InSine));
    }
}
