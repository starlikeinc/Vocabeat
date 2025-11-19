using UnityEngine;

public abstract class NoteEditFlowHoldSubState
{
    protected ChartEdit _context;
    protected NoteEditStateFlowHold _parent;

    public NoteEditFlowHoldSubState(ChartEdit context, NoteEditStateFlowHold parent)
    {
        _context = context;
        _parent = parent;        
    }

    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void OnUpdate()
    {
        OnLeftClickHandle();
    }

    protected virtual void OnLeftClickHandle() { }    
}
