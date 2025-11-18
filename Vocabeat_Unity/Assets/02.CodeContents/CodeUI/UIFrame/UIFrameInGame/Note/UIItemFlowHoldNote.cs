using LUIZ.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIItemFlowHoldNote : UITemplateItemBase, IFlowHoldNote
{
    [SerializeField] private Image _imgHead;
    [SerializeField] private Image _imgTail;
    [SerializeField] private Image _imgBody;
    [SerializeField] private Image _imgCursor;

    public Note NoteData { get; private set; }
    public RectTransform RectTrs { get; private set; }
    public ENoteType NoteType => NoteData.NoteType;

    public int StartTick => NoteData.Tick;
    public int EndTick => NoteData.Tick + NoteData.HoldTick;

    private RectTransform _spawnRect;

    protected override void OnUIWidgetInitialize(UIFrameBase parent)
    {
        base.OnUIWidgetInitialize(parent);
        RectTrs = (RectTransform)transform;
    }

    public void Setup(Note data, RectTransform spawn)
    {
        NoteData = data;
        _spawnRect = spawn;

        LayoutLongNote();
    }

    public void UpdateCursor(int currentTick)
    {
        if (currentTick < StartTick) return;
        if (currentTick > EndTick) currentTick = EndTick;

        float ratio = (float)(currentTick - StartTick) / (EndTick - StartTick);
        ratio = Mathf.Clamp01(ratio);

        float startX = _imgHead.rectTransform.anchoredPosition.x;
        float endX = _imgTail.rectTransform.anchoredPosition.x;

        float cursorX = Mathf.Lerp(startX, endX, ratio);

        _imgCursor.rectTransform.anchoredPosition =
            new Vector2(cursorX, _imgHead.rectTransform.anchoredPosition.y);
    }

    private void LayoutLongNote()
    {
        float width = _spawnRect.rect.width;
        float height = _spawnRect.rect.height;
        int tickPerPage = ManagerRhythm.Instance.RTimeline.TicksPerPage;

        int tickStartInPage = StartTick % tickPerPage;
        int tickEndInPage = EndTick % tickPerPage;

        float x01_start = (float)tickStartInPage / tickPerPage;
        float x01_end = (float)tickEndInPage / tickPerPage;

        float startX = (x01_start - 0.5f) * width;
        float endX = (x01_end - 0.5f) * width;

        // Head 위치
        _imgHead.rectTransform.anchoredPosition = new Vector2(startX, NoteData.Y * height);

        // Tail 위치
        _imgTail.rectTransform.anchoredPosition = new Vector2(endX, NoteData.Y * height);

        // Body 중앙 배치
        float bodyX = (startX + endX) * 0.5f;
        float bodyWidth = Mathf.Abs(endX - startX);

        _imgBody.rectTransform.anchoredPosition = new Vector2(bodyX, NoteData.Y * height);
        _imgBody.rectTransform.sizeDelta = new Vector2(bodyWidth, _imgBody.rectTransform.sizeDelta.y);
    }

    private Vector2 GetNoteAnchoredPos(int tick, float y01)
    {
        float parentWidth = _spawnRect.rect.width;
        float parentHeight = _spawnRect.rect.height;

        int tickPerPage = ManagerRhythm.Instance.RTimeline.TicksPerPage;
        int tickInPage = tick % tickPerPage;

        float x01 = (float)tickInPage / tickPerPage;

        float posX = (x01 - 0.5f) * parentWidth;
        float posY = (y01 - 0.5f) * parentHeight;

        return new Vector2(posX, posY);
    }    
}
