using UnityEngine;

public class RhythmTimeline : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private AudioSource _bgmSrc;    
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

    private double _timelineStartDsp;
    private bool _playing;
    private float _bpm;
    private float _secPerBeat;
    private float _secPerTick;

    // ========================================            
    public void InitTimeline(SongData_SO songDataSO)
    {
        _bgmSrc.clip = songDataSO.BGMClip;
        _bpm = songDataSO.BPM;
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

        if (_bgmSrc)
        {
            double songStartDsp = _timelineStartDsp + preSongSec;
            _bgmSrc.Stop();
            _bgmSrc.PlayScheduled(songStartDsp);
        }

        _playing = true;
    }

    public void Stop()
    {
        _playing = false;

        if (_bgmSrc)
            _bgmSrc.Stop();

        TimelineTime = 0f;
        SongTime = 0f;
        CurTick = 0;
        PageT = 0f;
    }

    public void UpdateTimeline()
    {
        if (!_playing) return;

        double now = AudioSettings.dspTime;

        // 타임라인 전체 시간 (프리롤 포함) 
        TimelineTime = (float)(now - _timelineStartDsp);
        CurTick = Mathf.FloorToInt(TimelineTime / _secPerTick);

        // 곡(오디오) 기준 시간 = 전체 시간 - 프리롤
        float preSongSec = _preSongTicks * _secPerTick;
        float songTime = TimelineTime - preSongSec;
        if (songTime < 0f) songTime = 0f;
        SongTime = songTime;

        // 스캔라인용 PageT
        int pageIndex = CurTick / _ticksPerPage;
        int pageTick = CurTick % _ticksPerPage;
        PageT = (float)pageTick / _ticksPerPage;
    }

    // ========================================        
    private void RecalculateTiming()
    {
        _secPerBeat = 60f / _bpm;
        _secPerTick = _secPerBeat / _ticksPerBeat;
    }
}
