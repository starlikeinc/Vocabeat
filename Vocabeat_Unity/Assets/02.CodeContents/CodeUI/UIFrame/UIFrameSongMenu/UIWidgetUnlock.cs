using LUIZ.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIWidgetUnlock : UIWidgetCanvasBase
{
    [SerializeField] private Image _imgSongThumb;
    [SerializeField] private TMP_Text _textUnlockValue;

    [SerializeField] private RectTransform _layoutRect;

    private SongDataSO _songDataSO;

    private UIFrameSongMenu _frameSongMenu;

    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);
        _frameSongMenu = parentFrame as UIFrameSongMenu;
    }

    public void DoWidgetUnlockSetting(SongDataSO songDataSO)
    {
        _songDataSO = songDataSO;
        _imgSongThumb.overrideSprite = songDataSO.SongThumb;
        _textUnlockValue.text = songDataSO.UnlockCondition.CostAmount.ToString();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutRect);

        DoUIWidgetShow();
    }

    public void OnUnlock()
    {
        if(ManagerRhythm.Instance.MusicKey >= _songDataSO.UnlockCondition.CostAmount)
        {
            ManagerUnlock.Instance.Unlock(_songDataSO);
            _frameSongMenu.RefreshSongList();
            DoUIWidgetHide();
        }        
    }

    public void OnAddKey()
    {
        ManagerRhythm.Instance.AddMusicKey();
    }

    public void OnCancel()
    {
        DoUIWidgetHide();
    }
}
