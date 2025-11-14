using LUIZ.UI;
using UnityEngine;

public class UIFrameInGame : UIFrameBase
{
    [Header("Test")]
    [SerializeField] private NoteDataSheet NoteData;
    [SerializeField] private AudioSource MetronomeSrc;

    [Header("R_Timeline")]
    [SerializeField] private RhythmTimeline RTimeline;

    [Header("Scanline")]
    [SerializeField] private UIWidgetScanLine WidgetScanLine;

    [Header("Spawner")]
    [SerializeField] private UITemplateNoteSpawner NoteSpawner;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private int _nextBeatIndex;
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (RTimeline.IsPlaying)
                StopTestPlay();
            else
                StartTestPlay();
        }

        if (RTimeline == null || !RTimeline.IsPlaying)
            return;
        
        if (NoteSpawner != null)
            NoteSpawner.TickUpdate();

        UpdateMetronome();
    }
#endif

    private void StartTestPlay()
    {
        if (RTimeline == null) return;

        RTimeline.Play();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        _nextBeatIndex = 1;
#endif

        // 스캔라인은 RhythmTimeline.PageT를 보고 움직이기 때문에 
        if (WidgetScanLine != null)
            WidgetScanLine.ResetPosition();

        // 노트 스폰 초기화 및 바인딩
        if (NoteSpawner != null && NoteData != null)
        {
            NoteSpawner.Setup(NoteData, RTimeline);
        }
    }

    private void StopTestPlay()
    {
        if (RTimeline == null) return;

        RTimeline.Stop();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        _nextBeatIndex = 0;
#endif

        if (MetronomeSrc != null)
            MetronomeSrc.Stop();

        if (WidgetScanLine != null)
            WidgetScanLine.ResetPosition();

        // 노트 모두 정리
        if (NoteSpawner != null)
            NoteSpawner.ResetSpawner();
    }

    private void UpdateMetronome()
    {
        if (MetronomeSrc == null) return;

        float songTime = RTimeline.SongTime;
        float secPerBeat = RTimeline.SecPerBeat;

        // songTime이 n * secPerBeat를 지날 때마다 메트로놈 1번 재생
        while (songTime >= _nextBeatIndex * secPerBeat)
        {
            PlayMetronome();
            _nextBeatIndex++;
        }
    }

    public void PlayMetronome()
    {
        MetronomeSrc.Play();
    }

    public void BindSongData(int songID, EDifficulty diff)
    {
        // TODO : 곡 SO 파일 불러와서 난이도에 맞는 NoteData 불러오기
    }
}
