using System;
using DG.Tweening;
using LUIZ.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIItemSongSlot : UITemplateCarouselItemBase
{
    [Header("[ Unit Slot ]")]
    [SerializeField] private Button _button;
    [SerializeField] private Image _icon;        

    [Header("HighlightOption")]
    [SerializeField] private float _focusScale = 1.2f;
    [SerializeField] private float _duration = 0.2f;

    public event Action<UIItemSongSlot> OnClick;

    protected override void OnUIWidgetInitializePost(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitializePost(parentFrame);

        if (_button != null)
            _button.onClick.AddListener(() => OnClick?.Invoke(this));
    }

    protected override void OnApplyFocusState(bool isCenter)
    {
        transform.DOKill(true);

        float targetScale = isCenter ? _focusScale : 1f;
        transform.DOScale(targetScale, _duration)
                 .SetEase(Ease.OutQuad);
    }

    public void SetVisual(Sprite thumb)
    {
        if (_icon != null) _icon.overrideSprite = thumb;        
    }
}
