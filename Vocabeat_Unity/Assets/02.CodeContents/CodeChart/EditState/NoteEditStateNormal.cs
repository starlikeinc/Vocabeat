using UnityEngine;

/// <summary>
/// 일반 노트 편집 State
/// </summary>
public class NoteEditStateNormal : NoteEditStateBase
{
    public NoteEditStateNormal(ChartEdit chart) : base(chart) { }

    // ========================================    
    public override void OnEnter()
    {
        base.OnEnter();
        _context.EditState = EEditState.Nomral;
        Debug.Log("Normal State 진입");
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log("Normal State 해제");
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        HandleInputLeftClick();
        HandleInputRightClick();
    }

    // ========================================    
    private void HandleInputLeftClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (_context.Visualizer.TryGetGhostNoteData(out int pageIndex, out int tick, out float yNorm))
                _context.OnRequestAddOrUpdateNote(tick, yNorm, pageIndex, EChartEditType.Normal);
        }
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
