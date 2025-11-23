using System;
using System.Collections.Generic;
using UnityEngine;

public interface INote
{
    ENoteType NoteType { get; }
    Note NoteData { get; }
    RectTransform RectTrs { get; }
}

public interface IFlowHoldNote : INote
{
    int StartTick { get; }
    int EndTick { get; }

    Vector2 GetLocalPositionAtTick(int songTick);
}

public enum EJudgementType
{
    BlueStar,   // Perfect
    WhiteStar,  // Great
    YellowStar, // Good
    RedStar,    // Bad or Miss
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
    FlowHold,
    LongHold,    
}

[Serializable]
public class DiffNoteData
{
    public EDifficulty Diff;
    public List<Note> Notes = new();
}

[Serializable]
public class FlowCurvePoint
{
    public float t;   // A, B 사이 진행비율
    public float y01; // 화면 상 보이는 세로 위치
}

[Serializable]
public class FlowLongMeta
{
    // 사용자가 직접 찍은 컨트롤 포인트 (Start/End + CurvePoint들)
    public List<FlowCurvePoint> CurvePoints = new();

    // Spline에서 샘플링한 촘촘한 점들 (런타임 평가용)
    public List<FlowCurvePoint> SampledPoints = new();
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
    public FlowLongMeta FlowLongMeta;
}
