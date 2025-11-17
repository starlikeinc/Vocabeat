using DG.Tweening;
using LUIZ.UI;
using UnityEngine;

public class UIFrameInGame : UIFrameBase
{
    [Header("UICam")]
    [SerializeField] private Camera _uiCam;

    [Header("Scanline")]
    [SerializeField] private UIWidgetScanLine _widgetScanLine;

    [Header("Spawner")]
    [SerializeField] private UITemplateNoteSpawner _noteSpawner;

    [Header("Spawner")]
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
        if (_noteSpawner != null && _curSongDataSO != null)
        {
            var listNoteDatas = _curSongDataSO.NoteDatasByDiff[_songDiff];

            if (listNoteDatas != null)
                _noteSpawner.Setup(listNoteDatas);
        }
    }

    private void StopSong()
    {
        if (_widgetScanLine != null)
            _widgetScanLine.ResetPosition();

        // 노트 모두 정리
        if (_noteSpawner != null)
            _noteSpawner.ResetSpawner();
    }    

    private void OnTickUpdate(float pageT)
    {
        _widgetScanLine.UpdateScanline(pageT);
        _noteSpawner.TickUpdate();
    }

    // ========================================
    public void BindSongData(SongDataSO songDataSO, EDifficulty diff)
    {
        _curSongDataSO = songDataSO;
        _songDiff = diff;                

        _widgetGameBG.DoUIGameBGSetting(songDataSO.SongThumb, OnDimComplete);        
    }

    // ========================================
    private void OnDimComplete()
    {
        RectTransform touchArea = (RectTransform)_widgetScanLine.transform;
        ManagerRhythm.Instance.BindSongData(_curSongDataSO, touchArea, _uiCam, _noteSpawner.ActiveNotes);
    }
}
