using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Song_", menuName = "SongData/New SongData")]
public class SongDataSO : ScriptableObject
{
    [SerializeField] private string _songName;
    [SerializeField] private string _songComposer;
    [SerializeField] private int _bpm;
    [SerializeField] private Sprite _songThumb;
    [SerializeField] private Sprite _songBG;
    [SerializeField] private AudioCueSO _bgmCue;

    [SerializeField] private List<DiffNoteData> _diffNotes = new();    

    private Dictionary<EDifficulty, List<Note>> _noteDatasByDiff;
    private Dictionary<EDifficulty, int> _difficultyValueByDiff;

    public Dictionary<EDifficulty, List<Note>> NoteDatasByDiff
    {
        get
        {
            if (_noteDatasByDiff == null)
                BuildNoteDataDict();
            return _noteDatasByDiff;
        }
    }

    public Dictionary<EDifficulty, int> DifficultyValueByDiff
    {
        get
        {
            if (_difficultyValueByDiff == null)
                BuildNoteDataDict();
            return _difficultyValueByDiff;
        }
    }

    public string SongName => _songName;
    public string SongComposer => _songComposer;
    public int BPM => _bpm;
    public Sprite SongThumb => _songThumb;
    public Sprite SongBG => _songBG;
    public AudioCueSO BGMCue => _bgmCue;

    public List<DiffNoteData> DiffNotes => _diffNotes;

    // ========================================    
    private void BuildNoteDataDict()
    {
        _noteDatasByDiff = new();
        _difficultyValueByDiff = new();

        foreach (var noteData in _diffNotes)
        {
            if (noteData == null)
                continue;

            _noteDatasByDiff[noteData.Diff] = noteData.Notes;
            _difficultyValueByDiff[noteData.Diff] = noteData.DifficultyValue;
        }
    }

    // ========================================
    public int GetDifficultyLevel(EDifficulty diff, int defaultLevel = 1)
    {
        if (DifficultyValueByDiff != null &&
            DifficultyValueByDiff.TryGetValue(diff, out int level) &&
            level > 0)
        {
            return level;
        }

        return defaultLevel;
    }

    // ========================================
    public void SaveNoteDatas(EDifficulty diff, IList<Note> src, int level)
    {
        var diffNoteData = _diffNotes.Find(x => x.Diff == diff);

        if (diffNoteData == null)
        {
            diffNoteData = new DiffNoteData { Diff = diff, DifficultyValue = level };
            _diffNotes.Add(diffNoteData);
        }

        if (diffNoteData.Notes == null)
            diffNoteData.Notes = new();
        else
            diffNoteData.Notes.Clear();

        foreach (var noteData in src)
        {
            diffNoteData.Notes.Add(new Note
            {
                ID = noteData.ID,
                PageIndex = noteData.PageIndex,
                NoteType = noteData.NoteType,
                Tick = noteData.Tick,
                Y = noteData.Y,
                HasSibling = noteData.HasSibling,
                HoldTick = noteData.HoldTick,
                NextID = noteData.NextID,
                FlowLongMeta = noteData.FlowLongMeta,
            });
        }

        BuildNoteDataDict();
    }
}