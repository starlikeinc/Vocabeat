using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UINoteNormalSpawner : UITemplateNoteSpawnerBase<UIItemNoteNormal>
{
    protected override void SelectNotesByType(List<Note> notes, out List<Note> filteredNotes)
    {
        filteredNotes = notes.Where(note => note.NoteType == ENoteType.Normal).ToList();
    }

    protected override UIItemNoteNormal GetUIItemNote(Note note)
    {
        UIItemNoteNormal item = DoTemplateRequestItem<UIItemNoteNormal>(transform);
        item.Setup(note, _spawnRectTrs);
        return item;
    }
}
