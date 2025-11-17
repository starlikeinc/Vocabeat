using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Song_", menuName = "SongData/New SongData")]
public class SongDataSO : ScriptableObject
{
    [SerializeField] private string _songName;
    [SerializeField] private int _bpm;
    [SerializeField] private Sprite _songThumb;
    [SerializeField] private AudioCueSO _bgmCue;

    [SerializeField] private List<DiffNoteData> _diffNotes = new();

    private Dictionary<EDifficulty, List<Note>> _noteDatasByDiff;
    public Dictionary<EDifficulty, List<Note>> NoteDatasByDiff
    {
        get
        {
            if (_noteDatasByDiff == null)
                BuildNoteDataDict();
            return _noteDatasByDiff;
        }
    }

    public string SongName => _songName;
    public int BPM => _bpm;
    public Sprite SongThumb => _songThumb;
    public AudioCueSO BGMCue => _bgmCue;

    public List<DiffNoteData> DiffNotes => _diffNotes;

    // ========================================
    //private void OnEnable()
    //{
    //    Debug.Log($"<color=green> 노트 딕셔너리 초기화 </color>");
    //    BuildNoteDataDict();
    //}

    private void BuildNoteDataDict()
    {
        _noteDatasByDiff = new();

        foreach (var noteData in _diffNotes)
        {
            if (noteData == null)
                continue;

            if (!_noteDatasByDiff.ContainsKey(noteData.Diff))
                _noteDatasByDiff.Add(noteData.Diff, noteData.Notes);
            else
                _noteDatasByDiff[noteData.Diff] = noteData.Notes;
        }
    }

    // ========================================
    public void SaveNoteDatas(EDifficulty diff, IList<Note> src)
    {
        var diffNoteData = _diffNotes.Find(x => x.Diff == diff);

        if (diffNoteData == null)
        {
            diffNoteData = new DiffNoteData { Diff = diff };
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
            });
        }

        BuildNoteDataDict();
    }
}