using DG.Tweening;
using LUIZ.UI;
using UnityEngine;
using UnityEngine.UI;

public class UISongBGScroll : UIWidgetBase
{
    [SerializeField] private RectTransform _bgRect;
    [SerializeField] private Image _bgImg;
    [SerializeField] private float _duration = 10f;

    private Tween _bgScrollTween;

    private float _bgHalfOffset = 50f;

    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);
        InitBGScrollSizeSetup();
    }

    public void DoBGChange(Sprite bg)
    {
        _bgImg.overrideSprite = bg;
    }

    public void DoBGScrollInfinite()
    {        
        _bgRect.anchoredPosition = new Vector2(-_bgHalfOffset, _bgRect.anchoredPosition.y);
        
        _bgScrollTween = _bgRect.DOAnchorPosX(_bgHalfOffset, _duration)
                       .SetEase(Ease.InOutSine)
                       .SetLoops(-1, LoopType.Yoyo);
    }

    public void StopBGScroll()
    {
        if (_bgScrollTween != null)
            _bgScrollTween.Kill();
    }

    private void InitBGScrollSizeSetup()
    {
        var parent = _bgRect.parent as RectTransform;

        // 이미지가 부모보다 얼마나 더 큰가?
        float scaleMultiplier = _bgRect.localScale.x;
        float diff = (_bgRect.rect.width * scaleMultiplier) - parent.rect.width;

        // diff/2 만큼 좌우 이동
        //_bgHalfOffset = diff * 0.5f;
    }
}
