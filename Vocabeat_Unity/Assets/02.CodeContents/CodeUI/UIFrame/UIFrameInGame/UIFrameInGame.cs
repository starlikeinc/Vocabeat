using System.Collections.Generic;
using LUIZ.UI;
using UnityEngine;

public class UIFrameInGame : UIFrameBase
{
    [Header("UICam")]
    [SerializeField] private Camera _uiCam;

    [Header("Scanline")]
    [SerializeField] private UIWidgetScanLine _widgetScanLine;

    [Header("Spawners")]
    [SerializeField] private UINoteNormalSpawner _noteNormalSpawner;
    [SerializeField] private UINoteFlowHoldSpawner _noteFlowHoldSpawner;

    [Header("BG")]
    [SerializeField] private UIWidgetGameBG _widgetGameBG;

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
        ManagerRhythm.Instance.OnSongStarted -= StartSong;
        ManagerRhythm.Instance.OnSongStarted += StartSong;
        ManagerRhythm.Instance.OnSongEnded -= StopSong;
        ManagerRhythm.Instance.OnSongEnded += StopSong;
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

    private void StopSong()
    {
        if (_widgetScanLine != null)
            _widgetScanLine.ResetPosition();

        // 노트 모두 정리
        if (_noteNormalSpawner != null)
            _noteNormalSpawner.ResetSpawner();
        if (_noteFlowHoldSpawner != null)
            _noteFlowHoldSpawner.ResetSpawner();
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

        _widgetGameBG.DoUIGameBGSetting(songDataSO.SongThumb, OnDimComplete);
    }

    // ========================================
    private void OnDimComplete()
    {
        ManagerRhythm.Instance.PlaySong();
    }
}
