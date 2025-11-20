using LUIZ.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIItemNoteNormal : UITemplateItemBase, INote
{
    [SerializeField] private Image ImgNote;  // 실제 노트 이미지    

    [SerializeField] private UnityEvent OnNoteShow;

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

    protected override void OnUnityEnable()
    {
        base.OnUnityEnable();
        OnNoteShow?.Invoke();
    }

    // ========================================
    public void Setup(Note data, RectTransform spawnRect)
    {
        NoteData = data;
        NoteType = data.NoteType;
        Debug.Log($"[Tick:{NoteData.Tick}][ID:{NoteData.ID}]노트 셋팅됨.");

        _spawnRectTrs = spawnRect;

        RectTrs.anchoredPosition = NoteUtility.GetNotePosition(_spawnRectTrs, NoteData.Tick, NoteData.Y);
    }
}
