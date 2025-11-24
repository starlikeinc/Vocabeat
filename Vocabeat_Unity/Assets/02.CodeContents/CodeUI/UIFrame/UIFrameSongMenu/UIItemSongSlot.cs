using System;
using DG.Tweening;
using LUIZ.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIItemSongSlot : UITemplateCarouselItemBase
{
    [Header("[ Unit Slot ]")]
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _contLock;
    [SerializeField] private Image _icon;        

    [Header("HighlightOption")]
    [SerializeField] private float _focusScale = 1.2f;
    [SerializeField] private float _duration = 0.2f;

    public event Action<UIItemSongSlot> OnClick;

    private UIFrameSongMenu _frameSongMenu;

    private SongDataSO _songDataSO;

    private bool _isInitialized;   // 여러 번 초기화 방지용
    private bool _isCenter;

    protected override void OnUIWidgetInitializePost(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitializePost(parentFrame);

        _frameSongMenu = parentFrame as UIFrameSongMenu;

        if (_button == null)
            return;

        if (_isInitialized)
            return; 

        _isInitialized = true;

        _button.onClick.AddListener(OnButtonClicked);
    }

    protected override void OnApplyFocusState(bool isCenter)
    {
        transform.DOKill(true);

        _isCenter = isCenter;
        float targetScale = isCenter ? _focusScale : 1f;
        transform.DOScale(targetScale, _duration)
                 .SetEase(Ease.OutQuad);
    }       

    public void SetVisual(SongDataSO songData)
    {
        if (_icon != null) _icon.overrideSprite = songData.SongThumb;
        _contLock.SetActive(!ManagerUnlock.Instance.IsUnlocked(songData));
        _songDataSO = songData;
    }

    private void OnButtonClicked()
    {
        OnClick?.Invoke(this);
        if (!_isCenter)
        {
            _frameSongMenu.PlayFrameSfx(ESongMenuSfxKey.Slide);
            _frameSongMenu.ChangeFrameBGM(_songDataSO.BGMCue);
        }            
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnButtonClicked);
    }
}
