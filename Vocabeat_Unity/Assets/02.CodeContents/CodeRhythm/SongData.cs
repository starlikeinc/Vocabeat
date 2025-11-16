using System;
using System.Collections.Generic;
using UnityEngine;

public interface INote
{
    ENoteType NoteType { get; }
    Note NoteData { get; }
    RectTransform RectTrs { get; }
}

public enum EJudgementType
{
    BlueStar,   // Perfect
    WhiteStar,  // Great
    YellowStar, // Good
    RedStar,    // Bad

    Miss,
}

public enum EDifficulty
{
    Easy,
    Normal,
    Hard,
}

public enum ENoteType
{
    Normal,
    Long,
    Hold,
    Curve,
}

[Serializable]
public class DiffNoteData
{
    public EDifficulty Diff;
    public List<Note> Notes = new();
}

[Serializable]
public class Note
{
    public int ID;
    public int PageIndex;
    public ENoteType NoteType;
    public int Tick;
    public float Y;
    public bool HasSibling;
    public int HoldTick;
    public int NextID;
}
