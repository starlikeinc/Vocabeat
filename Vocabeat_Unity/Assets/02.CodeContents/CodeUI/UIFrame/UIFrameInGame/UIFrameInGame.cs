using LUIZ.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFrameInGame : UIFrameBase
{
    [Header("UICam")]
    [SerializeField] private Camera _uiCam;

    [Header("Scanline")]
    [SerializeField] private UIWidgetScanLine _widgetScanLine;

    [Header("Spawners")]
    [SerializeField] private UINoteNormalSpawner _noteNormalSpawner;
    [SerializeField] private UINoteFlowHoldSpawner _noteFlowHoldSpawner;

    [Header("Top Info")]
    [SerializeField] private UIWidgetTopPanel _widgetTopPanel;    

    [Header("BG")]
    [SerializeField] private UIWidgetGameBG _widgetGameBG;

    [Header("Song Progress")]
    [SerializeField] private UIWidgetSongProgress _widgetSongProgress;    

    [Header("PausePopup")]
    [SerializeField] private UIWidgetPausePopup _widgetPausePopup;

    private SongDataSO _curSongDataSO;
    private EDifficulty _songDiff;    

    // ========================================
    protected override void OnUIFrameInitialize()
    {
        base.OnUIFrameInitialize();
        RegistEvents();
    }

    private void RegistEvents()
    {
        ManagerRhythm.Instance.OnTickUpdate -= OnTickUpdate;
        ManagerRhythm.Instance.OnTickUpdate += OnTickUpdate;
        ManagerRhythm.Instance.OnSongBinded -= OnSongDataBinded;
        ManagerRhythm.Instance.OnSongBinded += OnSongDataBinded;
        ManagerRhythm.Instance.OnSongStarted -= StartSong;
        ManagerRhythm.Instance.OnSongStarted += StartSong;
        ManagerRhythm.Instance.OnSongEnded -= EndSong;
        ManagerRhythm.Instance.OnSongEnded += EndSong;
    }

    private void OnSongDataBinded()
    {
        if(!UIChannel.IsUIFrameShow<UIFrameInGame>())
            UIChannel.UIShow<UIFrameInGame>();

        SetGameTopInfo();
        SetGameBG();
        SetSongProgress();
    }

    private void StartSong()
    {
        // 스캔라인은 RhythmTimeline.PageT를 보고 움직이기 때문에 
        if (_widgetScanLine != null)
            _widgetScanLine.ResetPosition();

        // 노트 스폰 초기화 및 바인딩
        if (_noteNormalSpawner != null && _noteFlowHoldSpawner != null && _curSongDataSO != null)
        {
            var listNoteDatas = _curSongDataSO.NoteDatasByDiff[_songDiff];

            if (listNoteDatas != null)
            {
                _noteNormalSpawner.Setup(listNoteDatas);
                _noteFlowHoldSpawner.Setup(listNoteDatas);
            }                
        }
    }

    private void EndSong()
    {
        if (_widgetScanLine != null)
            _widgetScanLine.ResetPosition();

        // 노트 모두 정리
        if (_noteNormalSpawner != null)
            _noteNormalSpawner.ResetSpawner();
        if (_noteFlowHoldSpawner != null)
            _noteFlowHoldSpawner.ResetSpawner();

        UIChannel.UIShow<UIFrameBlinder>().BlindWithNextStep(() =>
        {
            UIChannel.UIHide<UIFrameInGame>();
            UIChannel.UIShow<UIFrameResult>().DoFrameResultSetting();
        });
    }

    private void OnTickUpdate(float pageT)
    {
        _widgetScanLine.UpdateScanline(pageT);
        _noteNormalSpawner.TickUpdate();
        _noteFlowHoldSpawner.TickUpdate();
    }

    // ========================================
    public void BindSongData(SongDataSO songDataSO, EDifficulty diff)
    {
        _curSongDataSO = songDataSO;
        _songDiff = diff;

        RectTransform touchArea = (RectTransform)_widgetScanLine.transform;
        Camera uiCam = ManagerUI.Instance.GetRootCanvas().worldCamera;
        
        ManagerRhythm.Instance.BindSongData(_curSongDataSO, _songDiff, touchArea, uiCam);        
    }

    public void ReturnToSongMenu()
    {
        UIChannel.UIShow<UIFrameBlinder>().BlindWithNextStep(() =>
        {
            ManagerRhythm.Instance.ExitSong();
            UIChannel.UIHide<UIFrameInGame>();
            UIChannel.UIShow<UIFrameSongMenu>().DoFrameSongMenuSetting(false);
        });
    }

    // ========================================
    private void SetGameTopInfo()
    {
        _widgetTopPanel.DoWidgetTopPanelSetting(_curSongDataSO);
    }

    private void SetGameBG()
    {
        _widgetGameBG.DoUIGameBGSetting(_curSongDataSO.SongBG, OnDimComplete);
    }

    private void SetSongProgress()
    {
        _widgetSongProgress.WidgetSongProgressSetting(_curSongDataSO.BGMCue.GetRandomClip());
    }

    // ========================================
    private void OnDimComplete()
    {
        ManagerRhythm.Instance.PlaySong();
    }

    // ========================================
    public void OnGamePause()
    {
        ManagerRhythm.Instance.PauseSong();
        _widgetPausePopup.DoUIWidgetShow();
    }
}
