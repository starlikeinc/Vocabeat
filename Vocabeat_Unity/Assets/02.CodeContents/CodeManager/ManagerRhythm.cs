using LUIZ;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ManagerRhythm : SingletonBase<ManagerRhythm>
{    
    public event Action<float> OnTickUpdate;
    public event Action OnSongStarted;
    public event Action OnSongEnded;

    [Header("Refs")]
    [SerializeField] private RhythmTimeline _rTimeline;
    [SerializeField] private NoteTouchJudgeSystem _noteJudgeSystem;

    [Header("Test - Metronome")]
    [SerializeField] private AudioSource _metronomSrc;

    public RhythmTimeline RTimeline => _rTimeline;
    public NoteTouchJudgeSystem NoteJudegeSystem => _noteJudgeSystem;

    public bool IsPlaying => _rTimeline.IsPlaying;

    // ========================================    
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    #region Test
    private int _nextBeatIndex;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (!_rTimeline.IsPlaying)
                StartTestPlay();
            else
                StopTestPlay();
        }

        if (_rTimeline == null || !_rTimeline.IsPlaying)
            return;

        _rTimeline.UpdateTimeline();        
        UpdateMetronome();

        OnTickUpdate?.Invoke(_rTimeline.PageT);        
    }

    private void StartTestPlay()
    {
        if (_rTimeline == null) return;

        _rTimeline.Play();

        OnSongStarted?.Invoke();       
    }

    private void StopTestPlay()
    {
        if (_rTimeline == null) return;

        _rTimeline.Stop();

        if (_metronomSrc != null)
            _metronomSrc.Stop();

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

    #endregion
#endif

    // ========================================        
    public void BindSongData(SongData_SO songDataSO, RectTransform touchArea, Camera uiCam, IReadOnlyList<INote> listNotes)
    {        
        _rTimeline.InitTimeline(songDataSO);
        _noteJudgeSystem.InitJudgementSystem(this, touchArea, uiCam, listNotes);
    }

    public void PlaySong()
    {

    }

    public void PauseSong()
    {

    }

    public void RetrySong()
    {

    }

    public void ExitSong()
    {

    }
}
