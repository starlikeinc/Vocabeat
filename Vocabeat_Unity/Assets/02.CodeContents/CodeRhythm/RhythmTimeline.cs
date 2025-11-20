using System;
using UnityEngine;

public class RhythmTimeline : MonoBehaviour
{
    public event Action<float> OnSongProgress;
    public event Action OnSongComplete;

    [Header("BGM Channel")]
    [SerializeField] private BGMEventChannelSO _bgmEventChannel;

    [Header("Timing")]    
    [SerializeField] private int _ticksPerBeat = 240;
    [SerializeField] private int _ticksPerPage = 960;

    [Header("Pre-Roll")]
    [SerializeField] private int _preSongTicks = 1920; // 곡 시작 전에 2페이지 만큼 미리 움직이기

    public float Bpm => _bpm;
    public float SecPerBeat => _secPerBeat;

    /// <summary>오디오(곡) 재생이 시작된 후 경과 시간 (프리롤 도중에는 0)</summary>
    public float SongTime { get; private set; }

    /// <summary>프리롤을 포함한 전체 타임라인 시간(초)</summary>
    public float TimelineTime { get; private set; }

    /// <summary>프리롤을 포함한 전체 타임라인 Tick</summary>
    public int CurTick { get; private set; }

    /// <summary>현재 페이지 내 진행도(0~1) - 스캔라인이 보는 값</summary>
    public float PageT { get; private set; }

    /// <summary>한 페이지 당 Tick 값</summary>
    public int TicksPerPage => _ticksPerPage;

    /// <summary>곡이 시작하기 전 여유 Tick</summary>
    public int PreSongTicks => _preSongTicks;

    public bool IsPlaying => _playing;
    public bool IsPaused => _paused;

    private double _timelineStartDsp;
    private bool _playing;
    private bool _paused;

    private float _bpm;
    private float _secPerBeat;
    private float _secPerTick;

    private double _pauseDspTime;             // Pause 눌렀던 시점의 dspTime
    private double _accumulatedPauseDuration; // 지금까지 Pause로 멈춰있던 총 시간

    private float _songTotalTime;
    private bool _isCompleted;

    // ========================================            
    public void BindTimelineData(SongDataSO songDataSO)
    {
        _bgmEventChannel.Raise(songDataSO.BGMCue);
        _bpm = songDataSO.BPM;

        _songTotalTime = songDataSO.BGMCue.GetRandomClip().length;
    }

    public void Play()
    {
        RecalculateTiming();

        float preSongSec = _preSongTicks * _secPerTick;

        _timelineStartDsp = AudioSettings.dspTime;
        TimelineTime = 0f;
        SongTime = 0f;
        CurTick = 0;
        PageT = 0f;
        _isCompleted = false;

        if (_bgmEventChannel)
        {
            double songStartDsp = _timelineStartDsp + preSongSec;
            _bgmEventChannel.StopAudio();
            _bgmEventChannel.PlayScheduled(songStartDsp);
        }

        _playing = true;
    }

    public void Pause()
    {
        if (!_playing || _paused)
            return;

        _paused = true;
        _playing = false;

        _pauseDspTime = AudioSettings.dspTime;

        if (_bgmEventChannel)
            _bgmEventChannel.PauseAudio();
    }

    public void Resume()
    {
        if (_playing || !_paused)
            return;

        double now = AudioSettings.dspTime;
        // 이번에 멈춰있던 구간 시간 추가
        _accumulatedPauseDuration += (now - _pauseDspTime);

        _paused = false;
        _playing = true;

        if (_bgmEventChannel)
            _bgmEventChannel.ResumeAudio();
    }

    public void Stop()
    {
        _playing = false;

        if (_bgmEventChannel)
            _bgmEventChannel.StopAudio();

        TimelineTime = 0f;
        SongTime = 0f;
        CurTick = 0;
        PageT = 0f;
        _isCompleted = false;
    }

    public void UpdateTimeline()
    {
        if (!_playing) return;

        double now = AudioSettings.dspTime;

        // 타임라인 전체 시간 (프리롤 포함) 
        TimelineTime = (float)(now - _timelineStartDsp - _accumulatedPauseDuration);
        if (TimelineTime < 0f) TimelineTime = 0f;

        CurTick = Mathf.FloorToInt(TimelineTime / _secPerTick);

        // 곡(오디오) 기준 시간 = 전체 시간 - 프리롤
        float preSongSec = _preSongTicks * _secPerTick;
        float songTime = TimelineTime - preSongSec;
        if (songTime < 0f) songTime = 0f;
        SongTime = songTime;
        OnSongProgress?.Invoke(SongTime);

        // 스캔라인용 PageT
        int pageIndex = CurTick / _ticksPerPage;
        int pageTick = CurTick % _ticksPerPage;
        PageT = (float)pageTick / _ticksPerPage;

        if (!_isCompleted && _songTotalTime > 0f && SongTime >= _songTotalTime)
        {
            _isCompleted = true;
            _playing = false;

            if (_bgmEventChannel)
                _bgmEventChannel.StopAudio();

            Debug.Log($"<color=green>곡 종료</color>");
            OnSongComplete?.Invoke();
        }
    }

    // ========================================        
    private void RecalculateTiming()
    {
        _secPerBeat = 60f / _bpm;
        _secPerTick = _secPerBeat / _ticksPerBeat;
    }

    public float SecPerTick => _secPerTick;

    public float TicksToSeconds(int ticks) => ticks * _secPerTick;
    public int SecondsToTicks(float seconds) => Mathf.FloorToInt(seconds / _secPerTick);
}
