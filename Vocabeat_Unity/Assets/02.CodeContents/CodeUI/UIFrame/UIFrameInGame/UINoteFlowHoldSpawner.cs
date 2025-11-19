using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UINoteFlowHoldSpawner : UITemplateNoteSpawnerBase<UIItemNoteFlowHold>
{
    protected override UIItemNoteFlowHold GetUIItemNote(Note note)
    {
        UIItemNoteFlowHold item = DoTemplateRequestItem<UIItemNoteFlowHold>(transform);
        item.Setup(note, _spawnRectTrs);
        return item;
    }

    protected override void SelectNotesByType(List<Note> notes, out List<Note> filteredNotes)
    {
        filteredNotes = notes.Where(note => note.NoteType == ENoteType.FlowHold).ToList();
    }

    protected override void OnUpdateTick(int tick)
    {
        base.OnUpdateTick(tick);
        foreach(var uiNote in _activeNotes)
        {
            uiNote.UpdateCursor(tick);
        }
    }
}
