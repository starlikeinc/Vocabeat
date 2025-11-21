using UnityEngine;

/// <summary>
/// 롱 노트 (따라가기) 편집 State
/// 
/// State 진입 후 첫, 끝 노트를 찍을 수 있음.
/// 끝 노트가 찍힌 상황에서 Left Shift 키 누르는 동안 해당 롱노트의 커브 지점을 찍을 수 있음.
/// </summary>
public class NoteEditStateFlowHold : NoteEditStateBase
{
    private NoteEditFlowHoldSubStateBase _subState;

    public NoteEditStateFlowHold(ChartEdit chart) : base(chart) 
    {
        _subState = new NoteEditFlowHoldPlaceState(chart, this);        
    }

    // ========================================
    public override void OnEnter()
    {
        base.OnEnter();
        _subState.OnEnter();
    }

    public override void OnExit()
    {
        _subState.OnExit();
        base.OnExit();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        HandleInputRightClick();

        _subState.OnUpdate();
    }    

    // ========================================    
    public void ChangeSubState(NoteEditFlowHoldSubStateBase newSubState)
    {
        _subState.OnExit();
        _subState = newSubState;
        _subState.OnEnter();
    }

    // ========================================    
    private void HandleInputRightClick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (_context.Visualizer.TryGetGhostNoteData(out int pageIndex, out int tick, out float yNorm))
                _context.OnRequestRemoveNote(tick, yNorm, pageIndex);
        }
    }
}
