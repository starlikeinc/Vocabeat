using UnityEngine;

/// <summary>
/// 일반 노트 편집 State
/// </summary>
public class NoteEditStateNormal : NoteEditStateBase
{
    public NoteEditStateNormal(ChartEdit chart) : base(chart) { }

    public override void OnUpdate()
    {
        base.OnUpdate();

        HandleInputLeftClick();
        HandleInputRightClick();
    }

    private void HandleInputLeftClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (_context.Visualizer.TryGetGhostNoteData(out int pageIndex, out int tick, out float yNorm))
                _context.OnRequestAddOrUpdateNote(tick, yNorm, pageIndex, ENoteType.Normal);
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
