using System.Collections.Generic;
using LUIZ.UI;
using UnityEngine;

public abstract class UITemplateNoteSpawnerBase<T> : UITemplateBase where T : UITemplateItemBase, INote
{
    [Header("노트가 보여질 RectTrs")]
    [SerializeField] protected RectTransform _spawnRectTrs;

    [Header("Note Spawner")]
    [SerializeField] private int _appearOffsetTicks = 480;

    protected readonly List<T> _activeNotes = new();

    private Queue<Note> _pendingNotes;
    private RhythmTimeline _timeline;
    private int _preSongTicks;

    private readonly Dictionary<int, T> _dictNotesByNotId = new();

    public IReadOnlyList<T> ActiveNotes => _activeNotes;

    // ========================================
    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);
        var judge = ManagerRhythm.Instance.NoteJudegeSystem;
        judge.OnJudgeResult -= OnDisappearByJudgement;
        judge.OnJudgeResult += OnDisappearByJudgement;
    }

    // ========================================
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

    public void TickUpdate()
    {
        if (_timeline == null || _pendingNotes == null)
            return;

        int timelineTick = _timeline.CurTick;
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

        while (_pendingNotes.Count > 0)
        {
            var next = _pendingNotes.Peek();
            int appearTick = next.Tick - _appearOffsetTicks;

            if (appearTick < 0)
                appearTick = 0;

            if (songTick < appearTick)
                break;

            _pendingNotes.Dequeue();

            var item = GetUIItemNote(next);
            _activeNotes.Add(item);

            _dictNotesByNotId[next.ID] = item;
        }
    }

    private void DespawnNote(Note targetNote)
    {
        if (!_dictNotesByNotId.TryGetValue(targetNote.ID, out T item))
            return;

        DoUITemplateReturn(item);
        _activeNotes.Remove(item);
        _dictNotesByNotId.Remove(targetNote.ID);
    }

    private void OnDisappearByJudgement(Note note, EJudgementType _)
    {
        DespawnNote(note);
    }

    private void ClearAllNotes()
    {
        DoUITemplateReturnAll();
        _activeNotes.Clear();
        _dictNotesByNotId.Clear();
    }

    // ========================================
    protected abstract void SelectNotesByType(List<Note> notes, out List<Note> filteredNotes);
    protected abstract T GetUIItemNote(Note note);

    protected virtual void OnUpdateTick(int tick) { }
}
