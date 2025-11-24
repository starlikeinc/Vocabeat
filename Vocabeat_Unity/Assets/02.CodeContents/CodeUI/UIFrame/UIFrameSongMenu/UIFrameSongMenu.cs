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

    [Header("이벤트")]
    [SerializeField] private UnityEvent _onFrameShowFromMain;
    [SerializeField] private UnityEvent _onFrameShowFromInGame;

    private EDifficulty _difficulty;
    private SongDataSO _songData;

    public void DoFrameSongMenuSetting(bool bFromMain) // 메인 메뉴에서 접속하면 FreePlay 토스트 띄우기
    {
        if (bFromMain)
            _onFrameShowFromMain?.Invoke();
        else
            _onFrameShowFromInGame?.Invoke();
        _bgScroll.DoBGScrollInfinite();

        _carouselSong.Initialize();        
    }

    public void SongDifficultySetting()
    {

    }

    public void SetSelectedSong(int index)
    {
        _widgetSongInfo.WidgetSongInfoSetting(index);
    }

    public void OnPlay()
    {
        UIChannel.UIShow<UIFrameBlinder>().BlindWithNextStep(() =>
        {
            UIChannel.UIShow<UIFrameInGame>().BindSongData(_songData, _difficulty);
        });        
    }

    public void OnBack()
    {

    }

    public void OnOption()
    {

    }
}
