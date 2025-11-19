using UnityEngine;

/// <summary>
/// 마지막으로 작성중인 롱 노트의 배지어 커브 편집하는 State
/// </summary>
public class NoteEditFlowHoldCurveState : NoteEditFlowHoldSubState
{
    public NoteEditFlowHoldCurveState(ChartEdit context, NoteEditStateFlowHold parent) : base(context, parent) { }

    public override void OnUpdate()
    {
        base.OnUpdate();
        CheckStateChange();
    }

    private void CheckStateChange()
    {
        if (Input.GetKeyUp(KeyCode.LeftShift))
            _parent.ChangeSubState(new NoteEditFlowHoldPlaceState(_context, _parent));
    }

    protected override void OnLeftClickHandle()
    {
        base.OnLeftClickHandle();
        if (Input.GetMouseButtonDown(0))
        {
            if (_context.Visualizer.TryGetGhostNoteData(out int pageIndex, out int tick, out float yNorm))
                _context.OnRequestAddOrUpdateNote(tick, yNorm, pageIndex, ENoteType.Normal, EChartEditType.FlowCurve);
        }
    }
}
