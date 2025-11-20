using LUIZ;
using System;
using UnityEngine;

public class ManagerRhythm : SingletonBase<ManagerRhythm>, IManagerInstance
{    
    public event Action<float> OnTickUpdate;
    public event Action OnSongStarted;
    public event Action OnSongEnded;

    public event Action<int> OnScoreChanged;

    [Header("Refs")]
    [SerializeField] private RhythmTimeline _rTimeline;
    [SerializeField] private NoteTouchJudgeSystem _noteJudgeSystem;

    [Header("Test - Metronome")]
    [SerializeField] private AudioSource _metronomSrc;

    public RhythmTimeline RTimeline => _rTimeline;
    public NoteTouchJudgeSystem NoteJudegeSystem => _noteJudgeSystem;

    public int CurrentScore
    {
        get => _currentScore;
        set
        {
            int prevScore = _currentScore;
            _currentScore = Mathf.Clamp(prevScore + value, 0, int.MaxValue);
            OnScoreChanged?.Invoke(prevScore);
        }
    }
    private int _currentScore;

    private SongDataSO _curSongDataSO;
    private EDifficulty _curDiff;

    public bool IsPlaying => _rTimeline.IsPlaying;

    // ========================================        
    private int _nextBeatIndex; // Editor 전용

    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();
        if (RTimeline != null)
            RTimeline.OnSongComplete += HandleSongComplete;
    }

    protected override void OnUnityDestroy()
    {
        base.OnUnityDestroy();
        if (RTimeline != null)
            RTimeline.OnSongComplete -= HandleSongComplete;
    }

    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (!_rTimeline.IsPlaying)
                StartTestPlay();
            else
                StopTestPlay();
        }
#endif
        if (_rTimeline == null || !_rTimeline.IsPlaying)
            return;

        _rTimeline.UpdateTimeline();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        UpdateMetronome();
#endif

        OnTickUpdate?.Invoke(_rTimeline.PageT);        
    }

    private void StartTestPlay()
    {
        if (_rTimeline == null) return;

        if (_noteJudgeSystem != null) _noteJudgeSystem.ResetForNewSong();
        _nextBeatIndex = 0; // 추가

        _rTimeline.Play();
        OnSongStarted?.Invoke();       
    }

    private void StopTestPlay()
    {
        if (_rTimeline == null) return;

        _rTimeline.Stop();
        if (_metronomSrc != null) _metronomSrc.Stop();
        if (_noteJudgeSystem != null) _noteJudgeSystem.ResetForNewSong();
        _nextBeatIndex = 0; // 추가

        OnSongEnded?.Invoke();
    }

    private void UpdateMetronome()
    {
        if (_metronomSrc == null) return;

        float songTime = _rTimeline.SongTime;
        float secPerBeat = _rTimeline.SecPerBeat;

        // songTime이 n * secPerBeat를 지날 때마다 메트로놈 1번 재생
        while (songTime >= _nextBeatIndex * secPerBeat)
        {
            _metronomSrc.Play();
            _nextBeatIndex++;
        }
    }

    private void HandleSongComplete()
    {
        OnSongEnded?.Invoke();
    }

    // ========================================
    public bool IsInitialized()
    {
        return Instance != null;
    }

    public void BindSongData(SongDataSO songDataSO, EDifficulty diff, RectTransform touchArea, Camera uiCam)
    {
        _curSongDataSO = songDataSO;
        _curDiff = diff;

        RTimeline.BindTimelineData(songDataSO);

        NoteJudegeSystem.InitJudgementSystem(this, touchArea, uiCam);
        
        var listNoteDatas = songDataSO.NoteDatasByDiff[diff];
        NoteJudegeSystem.BindJudgementNoteDatas(listNoteDatas);
    }

    public void PlaySong()
    {
        if (_rTimeline == null) return;

        if (_noteJudgeSystem != null) _noteJudgeSystem.ResetForNewSong();
        _nextBeatIndex = 0; // 추가

        _rTimeline.Play();
        OnSongStarted?.Invoke();
    }

    public void PauseSong()
    {
        RTimeline.Pause();
    }

    public void ResumeSong()
    {

    }

    public void RetrySong()
    {

    }

    public void ExitSong()
    {

    }

    // ======================================== 점수관련 - 어차피 기획 상 따로 저장 안 하는 거 같아서 그냥 여기다 함
    public void SetScoreValueByJudgeType(EJudgementType judgeType)
    {
        int getPoint = GameConstant.GetPointByJudgement(judgeType);
        CurrentScore = getPoint; // 프로퍼티에서 prev + getPoint 해둠
    }
}
