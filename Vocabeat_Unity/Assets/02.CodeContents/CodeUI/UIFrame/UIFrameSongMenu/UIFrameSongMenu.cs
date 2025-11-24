using LUIZ.UI;
using UnityEngine;
using UnityEngine.Events;

public enum ESongMenuSfxKey
{    
    DifficultySelect,
    Slide,
    Play,
    Unlock,
    BtnClick,    
}

public class UIFrameSongMenu : UIFrameUsage<ESongMenuSfxKey>
{
    [Header("곡 배경")]
    [SerializeField] private UISongBGScroll _bgScroll;

    [Header("캐러셀")]
    [SerializeField] private UICarouselSong _carouselSong;

    [Header("곡 정보")]
    [SerializeField] private UIWidgetSongInfo _widgetSongInfo;

    [Header("언락")]
    [SerializeField] private UIWidgetUnlock _widgetUnlock;

    [Header("이벤트")]
    [SerializeField] private UnityEvent _onFrameShowFromMain;
    [SerializeField] private UnityEvent _onFrameShowFromInGame;

    private EDifficulty _difficulty;
    private SongDataSO _songData;

    private int _lastSongIndex;

    protected override void OnUIFrameInitialize()
    {
        base.OnUIFrameInitialize();
        _carouselSong.OnCenterChanged -= HandleCenterChanged;
        _carouselSong.OnCenterChanged += HandleCenterChanged;
    }

    public void DoFrameSongMenuSetting(bool bFromMain) // 메인 메뉴에서 접속하면 FreePlay 토스트 띄우기
    {
        if (bFromMain)
        {
            _onFrameShowFromMain?.Invoke();
            _carouselSong.Initialize();
            _bgScroll.DoBGScrollInfinite();
        }
        else
        {
            _onFrameShowFromInGame?.Invoke();
            _carouselSong.RefreshAll(_lastSongIndex);
        }
    }

    public void SongDifficultySetting()
    {
        _carouselSong.RefreshAll();
    }

    public void SetSelectedSong(int index)
    {
        _widgetSongInfo.WidgetSongInfoSetting(index);
    }

    public void RefreshSongList()
    {
        _carouselSong.RefreshSlotsVisualOnly();
    }

    public void OnPlay()
    {
        if (_songData == null)
            return;

        if (!ManagerUnlock.Instance.IsUnlocked(_songData))
        {
            _widgetUnlock.DoWidgetUnlockSetting(_songData);
            return;
        }

        UIChannel.UIShow<UIFrameBlinder>().BlindWithNextStep(() =>
        {
            StopFrameBgm();
            UIChannel.UIHide<UIFrameSongMenu>();
            UIChannel.UIShow<UIFrameInGame>().BindSongData(_songData, _difficulty);
        });        
    }

    public void OnBack()
    {
        // TODO : 메인 메뉴로
    }

    public void OnOption()
    {

    }

    private void HandleCenterChanged(int index, SongDataSO songData)
    {
        _songData = songData;
        _lastSongIndex = index;
        ChangeFrameBGM(songData.BGMCue);
        _bgScroll.DoBGChange(songData.SongBG);
    }
}
