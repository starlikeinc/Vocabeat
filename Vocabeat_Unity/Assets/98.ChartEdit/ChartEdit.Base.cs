using System.Collections.Generic;
using UnityEngine;

public partial class ChartEdit
{
    [Header("Target SO")]
    [SerializeField] private SongDataSO TargetSongData;

    [Header("Visualizer")]
    [SerializeField] private ChartVisualizer _visualizer;

    [Header("Audio")]
    [SerializeField] private AudioSource _bgmAudioSource;

    [Header("Scanline")]
    [SerializeField] private ChartScanline _scanline;    

    [Header("Edit State")]
    [SerializeField] private EDifficulty _currentDifficulty = EDifficulty.Easy;
    [SerializeField] private int _currentPageIndex = 0;

    // 이 값은 "존재 가능한 페이지 수"
    // 0 ~ (_pageCount-1) 까지 이동 가능
    [SerializeField] private int _pageCount = 1;

    [Header("Note Edit")]
    [SerializeField] private ENoteType _currentNoteType = ENoteType.Normal;

    public ChartVisualizer Visualizer => _visualizer;

    private readonly Dictionary<EDifficulty, List<Note>> EditNotesDict = new();
    // Undo 스택 (현재 난이도용)
    private readonly Stack<List<Note>> _undoStack = new();    

    private NoteEditStateBase _currentState = null;

    public ENoteType CurrentNoteType => _currentNoteType;

    private bool _isPlayingFromPage;

    // ========================================
    private void Start()
    {
        if (!Application.isPlaying)
            return;

        InitFromSO();

        if (_visualizer != null)
        {
            _visualizer.Initialize(this);

            if (TargetSongData != null)
                _visualizer.VisualizerSetting(TargetSongData);

            // 초기 고스트 노트 타입 반영
            _visualizer.SetGhostNoteType(_currentNoteType);
        }

        InitEditState();

        RecalculatePageCount();
        RefreshPageView();

        SetupTiming();
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        if (_currentState != null)
        {
            _currentState.OnUpdate();
            _currentState.UpdateGhost();
        }            

        // 마우스 휠로 페이지 이동
        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0.1f)
        {
            ChangePage(-1);
        }
        else if (scroll < -0.1f)
        {
            ChangePage(1);
        }

        UpdateScanlineByMusic();
    }

    private void InitEditState()
    {
        _currentState = new NoteEditStateNormal(this);
        _currentState.OnEnter();
    }

    private void ChangeState(NoteEditStateBase newState)
    {
        if (_currentState != null)
            _currentState.OnExit();

        _currentState = newState;

        if (_currentState != null)
            _currentState.OnEnter();
    }

    // ========================================    
    private int _playStartPageTick; // 플레이 시작지점 기록

    // 재생 버튼
    public void PlayBGM()
    {
        if (!Application.isPlaying || _bgmAudioSource == null)
            return;

        int startTick = _currentPageIndex * _visualizer.TicksPerPage;
        _playStartPageTick = startTick;

        // Tick -> Time 변환
        float startTime = startTick * _secPerTick;

        // BGM 시작 위치 조정
        if (_bgmAudioSource.clip == null)
            _bgmAudioSource.clip = TargetSongData.BGMCue.GetRandomClip();

        _bgmAudioSource.time = Mathf.Clamp(startTime, 0f, _bgmAudioSource.clip.length);
        _bgmAudioSource.Play();

        _isPlayingFromPage = true;

        // 스캔라인 초기화 (페이지 시작 기준)
        _scanline.SetProgress(0f);
    }

    // 일시정지 버튼
    public void PauseBGM()
    {
        if (!Application.isPlaying)
            return;

        if (_bgmAudioSource == null)
            return;

        if (_bgmAudioSource.isPlaying)
        {
            _isPlayingFromPage = false;
            _bgmAudioSource.Pause();
        }            
    }

    // 멈춤(정지 + 처음으로)
    public void StopBGM()
    {
        if (!Application.isPlaying)
            return;

        if (_bgmAudioSource == null)
            return;

        _scanline.ResetPosition();

        _bgmAudioSource.Stop();
        _bgmAudioSource.time = 0f;

        // 정지할 때 1페이지로 돌아가고 싶으면 유지
        _currentPageIndex = 0;
        _isPlayingFromPage = false;
        RefreshPageView();
    }
}
