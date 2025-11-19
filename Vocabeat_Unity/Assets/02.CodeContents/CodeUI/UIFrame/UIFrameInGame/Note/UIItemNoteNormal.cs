using LUIZ.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIItemNoteNormal : UITemplateItemBase, INote
{
    [SerializeField] private Image ImgNote;  // 실제 노트 이미지    

    public Note NoteData { get; private set; }
    public RectTransform RectTrs { get; private set; }
    public ENoteType NoteType { get; private set; }    

    private RectTransform _spawnRectTrs;    

    // ========================================
    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);
        RectTrs = (RectTransform)transform;        
    }

    // ========================================
    public void Setup(Note data, RectTransform spawnRect)
    {
        NoteData = data;
        NoteType = data.NoteType;

        _spawnRectTrs = spawnRect;

        RectTrs.anchoredPosition = NoteUtility.GetNotePosition(_spawnRectTrs, NoteData.Tick, NoteData.Y);
    }
}
