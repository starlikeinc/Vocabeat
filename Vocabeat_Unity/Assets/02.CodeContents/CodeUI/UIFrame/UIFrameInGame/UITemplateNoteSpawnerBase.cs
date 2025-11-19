using System.Collections.Generic;
using LUIZ.UI;
using UnityEngine;

public abstract class UITemplateNoteSpawnerBase<T> : UITemplateBase where T : UITemplateItemBase
{
    [Header("노트가 보여질 RectTrs")] // WidgetScanline으로 두면 됨.
    [SerializeField] protected RectTransform _spawnRectTrs;

    [Header("Note Spawner")]    
    [SerializeField] private int _appearOffsetTicks = 480;  // 자신의 Tick 기준 몇 Tick 전에 등장할지 - 이건 나중에 따로 빼야됨.        

    protected readonly List<T> _activeNotes = new List<T>();

    private Queue<Note> _pendingNotes;
    private RhythmTimeline _timeline;
    private int _preSongTicks;    

    public IReadOnlyList<T> ActiveNotes => _activeNotes;

    // ========================================
    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);
        ManagerRhythm.Instance.NoteJudegeSystem.OnJudgeResult -= OnDisappearByJudgement;
        ManagerRhythm.Instance.NoteJudegeSystem.OnJudgeResult += OnDisappearByJudgement;
    }

    // ========================================
    /// <summary>
    /// 노트 시트 + 타임라인을 바인딩하고, 내부 상태 초기화
    /// </summary>
    public void Setup(List<Note> listNote)
    {
        _timeline = ManagerRhythm.Instance.RTimeline;
        _preSongTicks = _timeline?.PreSongTicks ?? 0;

        ClearAllNotes();

        if (listNote == null)
        {
            _pendingNotes = null;
            return;
        }

        SelectNotesByType(listNote, out var filteredNotes);

        var notes = filteredNotes;
        notes.Sort((a, b) => a.Tick.CompareTo(b.Tick));
        
        _pendingNotes = new Queue<Note>(notes);
    }

    /// <summary>
    /// 매 프레임 Tick 기준으로 스폰/삭제 처리    
    /// </summary>
    public void TickUpdate()
    {
        if (_timeline == null || _pendingNotes == null)
            return;

        int timelineTick = _timeline.CurTick;
        // 곡 기준 Tick(0 = 곡 시작 시점)
        int songTick = timelineTick - _preSongTicks;

        SpawnNotes(songTick);
        OnUpdateTick(songTick);
    }

    public void ResetSpawner()
    {
        ClearAllNotes();
        _pendingNotes = null;
        _timeline = null;
    }

    // ========================================
    private void SpawnNotes(int songTick)
    {
        if (_pendingNotes == null) return;

        // 등장해야 할 노트들 다 뽑기 
        while (_pendingNotes.Count > 0)
        {
            var next = _pendingNotes.Peek();
            int appearTick = next.Tick - _appearOffsetTicks;
            
            if (appearTick < 0)
                appearTick = 0;

            if (songTick < appearTick)
                break;

            // 등장 시간 지났으니 실제 생성
            _pendingNotes.Dequeue();

            var item = GetUIItemNote(next);
            _activeNotes.Add(item);
        }
    }

    private void DespawnNote(INote targetNote)
    {
        // TODO : 사라지는 연출 등

        T item = targetNote as T;
        if (item == null)
            return;

        DoUITemplateReturn(item);
        _activeNotes.Remove(item);
    }

    private void OnDisappearByJudgement(INote note, EJudgementType _)
    {
        DespawnNote(note);
    }

    private void ClearAllNotes()
    {
        DoUITemplateReturnAll();
        _activeNotes.Clear();
    }

    // ========================================
    protected abstract void SelectNotesByType(List<Note> notes, out List<Note> filteredNotes);
    protected abstract T GetUIItemNote(Note note);

    protected virtual void OnUpdateTick(int tick) { }
}
