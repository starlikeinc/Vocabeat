using System;
using UnityEngine;

public abstract class NoteEditStateBase
{    
    protected ChartEdit _context;

    public NoteEditStateBase(ChartEdit chart)
    {
        _context = chart;
    }

    // ========================================    
    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void OnUpdate() { }
}
