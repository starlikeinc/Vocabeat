using UnityEngine;
using UnityEngine.UI;

public class NotePreview : NoteEditBase
{
    public int ID { get; private set; }         = -1;
    public int Tick { get; private set; }       = -1;
    public int PageIndex { get; private set; }  = -1;

    public void SetNotePreviewData(int id, int tick, int pageIndex)
    {
        ID = id;
        Tick = tick;
        PageIndex = pageIndex;
    }
}
