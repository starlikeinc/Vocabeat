using System;

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
public class Song
{
    public int SongID;
    public string SongName;
    public string ThumbName;
    public int BPM;    
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
