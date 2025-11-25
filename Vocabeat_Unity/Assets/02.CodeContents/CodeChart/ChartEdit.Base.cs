using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum EEditState { None, Nomral, Long_Place, Long_Curve }

public partial class ChartEdit
{
    private static readonly EDifficulty[] s_diffOrder =
{
    EDifficulty.Easy,
    EDifficulty.Normal,
    EDifficulty.Hard,
};

    public event Action<EEditState> OnEditStateChanged;

    [Header("FlowHold Spline (Editor Only)")]
    [SerializeField] private FlowHoldSplineContext _flowSplineContext;

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
    [SerializeField] private int _currentDifficultyLevel = 1;
    [SerializeField] private int _currentPageIndex = 0;

    // 이 값은 "존재 가능한 페이지 수"
    // 0 ~ (_pageCount-1) 까지 이동 가능
    [SerializeField] private int _pageCount = 1;

    [Header("Note Edit")]
    [SerializeField] private ENoteType _currentNoteType = ENoteType.Normal;

    public ChartVisualizer Visualizer => _visualizer;

    public EDifficulty CurrentDifficulty => _currentDifficulty;
    public int CurrentDifficultyLevel => _currentDifficultyLevel;

    private readonly Dictionary<EDifficulty, List<Note>> EditNotesDict = new();
    // Undo 스택 (현재 난이도용)
    private readonly Stack<List<Note>> _undoStack = new();

    protected EEditState _editState;
    public EEditState EditState
    {
        get => _editState;
        set
        {
            _editState = value;
            OnEditStateChanged?.Invoke(_editState);
        }
    }

    public NoteEditStateBase CurrentState => _currentState;
    private NoteEditStateBase _currentState = null;
    
    public ENoteType CurrentNoteType => _currentNoteType;

    private bool _isPlayingFromPage;

    // ========================================
    private void Start()
    {
        if (!Application.isPlaying)
            return;

        void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                Debug.Log("플레이모드 종료. 현재 난이도 SO 저장");
                SaveCurrentDifficulty();
            }
        }
        EditorApplication.playModeStateChanged += OnPlayModeChanged;

        InitFromSO();
        InitEditState();

        if (_visualizer != null)
        {
            _visualizer.Initialize(this);

            if (TargetSongData != null)
                _visualizer.VisualizerSetting(TargetSongData);

            // 초기 고스트 노트 타입 반영
            EditState = EEditState.Nomral;
            _visualizer.SetGhostNoteType(EditState);
        }        

        RecalculatePageCount();

        RefreshDifficultyUI();

        RefreshPageView();

        SetupTiming();
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        if (_currentState != null)
            _currentState.OnUpdate();

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

    private void RefreshDifficultyUI()
    {
        if (_visualizer != null)
        {
            _visualizer.UpdateDifficultyDisplay(_currentDifficulty, _currentDifficultyLevel);
        }
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

    #region 난이도
    private void SetDifficultyInternal(EDifficulty newDiff)
    {
        if (_currentDifficulty == newDiff)
            return;

        _currentDifficulty = newDiff;

        // SO에 저장돼 있던 레벨 있으면 가져오고, 없으면 최소 1
        _currentDifficultyLevel = TargetSongData != null
            ? TargetSongData.GetDifficultyLevel(_currentDifficulty, 1)
            : Math.Max(1, _currentDifficultyLevel);

        // 현재 난이도의 노트로 화면 다시 그림
        RefreshPageView();

        // 텍스트 갱신
        RefreshDifficultyUI();
    }

    public void OnClick_DifficultyPrev()
    {
        int idx = Array.IndexOf(s_diffOrder, _currentDifficulty);
        if (idx < 0) idx = 0;

        idx = (idx - 1 + s_diffOrder.Length) % s_diffOrder.Length;
        SetDifficultyInternal(s_diffOrder[idx]);
    }

    public void OnClick_DifficultyNext()
    {
        int idx = Array.IndexOf(s_diffOrder, _currentDifficulty);
        if (idx < 0) idx = 0;

        idx = (idx + 1) % s_diffOrder.Length;
        SetDifficultyInternal(s_diffOrder[idx]);
    }

    private void ClampDifficultyLevel()
    {
        if (_currentDifficultyLevel < 1)
            _currentDifficultyLevel = 1;
    }

    public void OnClick_LevelUp()
    {
        _currentDifficultyLevel++;
        ClampDifficultyLevel();
        RefreshDifficultyUI();
    }

    public void OnClick_LevelDown()
    {
        _currentDifficultyLevel--;
        ClampDifficultyLevel();
        RefreshDifficultyUI();
    }

    public void OnClick_SetDifficultyEasy() => SetDifficultyInternal(EDifficulty.Easy);
    public void OnClick_SetDifficultyNormal() => SetDifficultyInternal(EDifficulty.Normal);
    public void OnClick_SetDifficultyHard() => SetDifficultyInternal(EDifficulty.Hard);
    #endregion
}
