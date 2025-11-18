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
    [SerializeField] private List<DiffFlowLongData> _diffFlowLongs = new();

    private Dictionary<EDifficulty, List<Note>> _noteDatasByDiff;
    private Dictionary<EDifficulty, List<FlowLongMeta>> _flowLongDatasByDiff;

    public Dictionary<EDifficulty, List<Note>> NoteDatasByDiff
    {
        get
        {
            if (_noteDatasByDiff == null)
                BuildNoteDataDict();
            return _noteDatasByDiff;
        }
    }
    public Dictionary<EDifficulty, List<FlowLongMeta>> FlowLongDatasByDiff
    {
        get
        {
            if (_flowLongDatasByDiff == null)
                BuildNoteDataDict();
            return _flowLongDatasByDiff;
        }
    }

    public string SongName => _songName;
    public int BPM => _bpm;
    public Sprite SongThumb => _songThumb;
    public AudioCueSO BGMCue => _bgmCue;

    public List<DiffNoteData> DiffNotes => _diffNotes;

    // ========================================    
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

    private void BuildFlowLongDataDict()
    {
        _flowLongDatasByDiff = new();

        foreach(var flowLongData in _diffFlowLongs)
        {
            if (flowLongData == null)
                continue;

            if (!_flowLongDatasByDiff.ContainsKey(flowLongData.Diff))
                _flowLongDatasByDiff.Add(flowLongData.Diff, flowLongData.FlowLongs);
            else
                _flowLongDatasByDiff[flowLongData.Diff] = flowLongData.FlowLongs;
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

    public void SaveFlowLongDatas(EDifficulty diff, IList<FlowLongMeta> src)
    {
        var diffFlowLongData = _diffFlowLongs.Find(x => x.Diff == diff);

        if(diffFlowLongData == null)
        {
            diffFlowLongData = new DiffFlowLongData { Diff = diff };
            _diffFlowLongs.Add(diffFlowLongData);
        }

        if (diffFlowLongData.FlowLongs == null)
            diffFlowLongData.FlowLongs = new();
        else
            diffFlowLongData.FlowLongs.Clear();

        foreach(var flowLongData in src)
        {
            diffFlowLongData.FlowLongs.Add(new FlowLongMeta
            {
                StartNoteID = flowLongData.StartNoteID,
                EndNoteID = flowLongData.EndNoteID,
                CurvePoints = flowLongData.CurvePoints,
            });
        }

        BuildFlowLongDataDict();
    }
}