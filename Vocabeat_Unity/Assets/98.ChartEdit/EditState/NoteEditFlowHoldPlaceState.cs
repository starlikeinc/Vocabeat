using UnityEngine;

/// <summary>
/// 시작 / 끝 Tick 지정하는 State
/// </summary>
public class NoteEditFlowHoldPlaceState : NoteEditFlowHoldSubState
{
    public NoteEditFlowHoldPlaceState(ChartEdit context, NoteEditStateFlowHold parent) : base(context, parent) { }

    public override void OnUpdate()
    {
        base.OnUpdate();
        CheckStateChange();
    }

    private void CheckStateChange()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && _context.HasCurveStartNote() == true)
            _parent.ChangeSubState(new NoteEditFlowHoldCurveState(_context, _parent));
    }

    protected override void OnLeftClickHandle()
    {
        base.OnLeftClickHandle();
        if (Input.GetMouseButtonDown(0))
        {
            if (_context.Visualizer.TryGetGhostNoteData(out int pageIndex, out int tick, out float yNorm))
                _context.OnRequestAddOrUpdateNote(tick, yNorm, pageIndex, ENoteType.Normal, EChartEditType.FlowPlace);
        }
    }
}
