using UnityEngine;

public abstract class NoteEditStateBase
{
    protected ChartEdit _context;

    public NoteEditStateBase(ChartEdit chart)
    {
        _context = chart;
    }

    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void OnUpdate() { }


    public virtual void UpdateGhost()
    {
        if (_context.Visualizer == null)
            return;

        var ghost = _context.Visualizer.GetGhost();
        if (ghost == null)
            return;

        // 기본 행동: 현재 타입으로 표시
        ghost.NoteEditVisualSetting(_context.CurrentNoteType);
    }
}
