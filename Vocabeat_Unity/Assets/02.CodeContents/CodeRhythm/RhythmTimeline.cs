using UnityEngine;

public class RhythmTimeline : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private AudioSource bgm;
    [SerializeField] private float bpm = 120f;
    [SerializeField] private int ticksPerBeat = 240;
    [SerializeField] private int ticksPerPage = 960;

    [Header("Pre-Roll")]
    [SerializeField] private int preSongTicks = 1920; // 곡 시작 전에 2페이지 만큼 미리 움직이기

    public float Bpm => bpm;
    public float SecPerBeat => _secPerBeat;

    /// <summary>오디오(곡) 재생이 시작된 후 경과 시간 (프리롤 도중에는 0)</summary>
    public float SongTime { get; private set; }

    /// <summary>프리롤을 포함한 전체 타임라인 시간(초)</summary>
    public float TimelineTime { get; private set; }

    /// <summary>프리롤을 포함한 전체 타임라인 Tick</summary>
    public int CurTick { get; private set; }

    /// <summary>현재 페이지 내 진행도(0~1) - 스캔라인이 보는 값</summary>
    public float PageT { get; private set; }

    public int TicksPerPage => ticksPerPage;
    public int PreSongTicks => preSongTicks;

    public bool IsPlaying => _playing;

    private double _timelineStartDsp;
    private bool _playing;
    private float _secPerBeat;
    private float _secPerTick;

    private void Awake()
    {
        RecalculateTiming();
    }

    private void RecalculateTiming()
    {
        _secPerBeat = 60f / bpm;
        _secPerTick = _secPerBeat / ticksPerBeat;
    }

    private void OnValidate()
    {
        RecalculateTiming();
    }

    public void Play()
    {
        RecalculateTiming();

        float preSongSec = preSongTicks * _secPerTick;

        _timelineStartDsp = AudioSettings.dspTime;
        TimelineTime = 0f;
        SongTime = 0f;
        CurTick = 0;
        PageT = 0f;

        if (bgm)
        {
            double songStartDsp = _timelineStartDsp + preSongSec;
            bgm.Stop();
            bgm.PlayScheduled(songStartDsp);
        }

        _playing = true;
    }

    public void Stop()
    {
        _playing = false;

        if (bgm)
            bgm.Stop();

        TimelineTime = 0f;
        SongTime = 0f;
        CurTick = 0;
        PageT = 0f;
    }

    private void Update()
    {
        if (!_playing) return;

        double now = AudioSettings.dspTime;

        // 타임라인 전체 시간 (프리롤 포함) 
        TimelineTime = (float)(now - _timelineStartDsp);
        CurTick = Mathf.FloorToInt(TimelineTime / _secPerTick);

        // 곡(오디오) 기준 시간 = 전체 시간 - 프리롤
        float preSongSec = preSongTicks * _secPerTick;
        float songTime = TimelineTime - preSongSec;
        if (songTime < 0f) songTime = 0f;
        SongTime = songTime;

        // 스캔라인용 PageT
        int pageIndex = CurTick / ticksPerPage;
        int pageTick = CurTick % ticksPerPage;
        PageT = (float)pageTick / ticksPerPage;
    }
}
