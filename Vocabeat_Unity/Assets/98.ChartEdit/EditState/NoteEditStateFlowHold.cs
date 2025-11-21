using UnityEngine;

/// <summary>
/// 롱 노트 (따라가기) 편집 State
/// </summary>
public class NoteEditStateFlowHold : NoteEditStateBase
{
    private NoteEditFlowHoldSubState _subState;

    public NoteEditStateFlowHold(ChartEdit chart) : base(chart) 
    {
        _subState = new NoteEditFlowHoldPlaceState(chart, this);
        _subState.OnEnter();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        HandleInputRightClick();

        _subState.OnUpdate();
    }

    public void ChangeSubState(NoteEditFlowHoldSubState newSubState)
    {
        _subState.OnExit();
        _subState = newSubState;
        _subState.OnEnter();
    }

    public override void UpdateGhost()
    {
        var ghost = _context.Visualizer.GetGhost();
        if (ghost == null) return;

        // Place 모드
        if (_subState is NoteEditFlowHoldPlaceState)
            ghost.NoteEditVisualSetting(ENoteType.FlowHold);

        // Curve 모드
        else if (_subState is NoteEditFlowHoldCurveState)
            ghost.NoteEditVisualSetting(ENoteType.FlowHold);
    }

    private void HandleInputRightClick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (_context.Visualizer.TryGetGhostNoteData(out int pageIndex, out int tick, out float yNorm))
                _context.OnRequestRemoveNote(tick, yNorm, pageIndex);
        }
    }
}
