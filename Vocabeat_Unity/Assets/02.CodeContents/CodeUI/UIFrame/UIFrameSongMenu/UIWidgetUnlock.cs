using LUIZ.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIWidgetUnlock : UIWidgetCanvasBase
{
    [SerializeField] private TMP_Text _textSongName;

    [SerializeField] private TMP_Text _textSongComposer;

    [SerializeField] private TMP_Text _textUnlockValue;

    [SerializeField] private TMP_Text _textKeyAmount;

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

        _textSongName.text = songDataSO.SongName;
        _textSongComposer.text = songDataSO.SongComposer;

        _textUnlockValue.text = songDataSO.UnlockCondition.CostAmount.ToString();
        _textKeyAmount.text = ManagerRhythm.Instance.MusicKey.ToString();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutRect);

        DoUIWidgetShow();
    }

    public void OnUnlock()
    {
        if(ManagerRhythm.Instance.MusicKey >= _songDataSO.UnlockCondition.CostAmount)
        {
            ManagerUnlock.Instance.Unlock(_songDataSO);
            _frameSongMenu.RefreshSongList();
            _frameSongMenu.PlayFrameSfx(ESongMenuSfxKey.Unlock);
            DoUIWidgetHide();
        }
        else
        {
            _frameSongMenu.PlayFrameSfx(ESongMenuSfxKey.BtnClick);
        }
    }

    public void OnCancel()
    {
        _frameSongMenu.PlayFrameSfx(ESongMenuSfxKey.BtnClick);
        DoUIWidgetHide();
    }
}
