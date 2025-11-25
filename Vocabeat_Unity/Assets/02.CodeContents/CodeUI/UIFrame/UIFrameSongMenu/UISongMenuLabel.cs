using DG.Tweening;
using LUIZ.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISongMenuLabel : UIWidgetBase
{
    [SerializeField] private RectTransform _labelRectTrs;
    [SerializeField] private float _scaleTweenDuration;

    [SerializeField] private Image _backBlind;
    [SerializeField] private GameObject _textGO;

    private Sequence _labelSequence;

    private UIFrameSongMenu _frameSongMenu;

    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);
        _frameSongMenu = parentFrame as UIFrameSongMenu;
        CacheSequnce();
    }

    public void OnLabelShow()
    {
        ResetSequnceTargets();
        _frameSongMenu.PlayFrameSfx(ESongMenuSfxKey.FreeLabel);
        _labelSequence.Restart();                
    }

    private void CacheSequnce()
    {
        _labelSequence = DOTween.Sequence()
        .SetAutoKill(false)
        .Pause();

        _labelSequence
            .Append(_labelRectTrs.DOScaleY(1f, _scaleTweenDuration).SetEase(Ease.OutQuad))
            .AppendCallback(() => _textGO.SetActive(true))
            .AppendInterval(1f)
            .AppendCallback(() => _textGO.SetActive(false))
            .Append(_labelRectTrs.DOScaleY(0f, _scaleTweenDuration).SetEase(Ease.OutQuad))
            .Append(_backBlind.DOFade(0f, _scaleTweenDuration).SetEase(Ease.OutQuad))
            .AppendCallback(() => _backBlind.gameObject.SetActive(false));
    }    

    private void ResetSequnceTargets()
    {
        Color color = _backBlind.color;
        color.a = 0.5f;
        _backBlind.color = color;
        _labelRectTrs.localScale = new Vector2(1f, 0f);
        _backBlind.gameObject.SetActive(true);
        _textGO.SetActive(false);
    }
}
