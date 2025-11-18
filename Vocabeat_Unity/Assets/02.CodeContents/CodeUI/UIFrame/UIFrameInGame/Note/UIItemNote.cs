using LUIZ.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIItemNote : UITemplateItemBase, INote
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
    public void DoUINoteVisualSetting(Note data, RectTransform spawnRect)
    {
        NoteData = data;
        NoteType = data.NoteType;

        _spawnRectTrs = spawnRect;

        RectTrs.anchoredPosition = GetNotePosition();
    }

    // ========================================
    private Vector2 GetNotePosition()
    {
        float parentWidth = _spawnRectTrs.rect.width;
        float parentHeight = _spawnRectTrs.rect.height;

        int tickPerPage = ManagerRhythm.Instance.RTimeline.TicksPerPage;
        int tickInPage = NoteData.Tick % tickPerPage; // 현재 노트의 페이지 내 Tick
        float x01 = (float)tickInPage / tickPerPage;

        float halfWidth = parentWidth * 0.5f;
        float posX = (x01 - 0.5f) * parentWidth;

        Vector2 pos = RectTrs.anchoredPosition;

        pos.x = posX;
        pos.y = (NoteData.Y - 0.5f) * parentHeight;

        return pos;
    }
}
