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

    private FlowLongMeta _flowMeta;

    protected override void OnUIWidgetInitialize(UIFrameBase parent)
    {
        base.OnUIWidgetInitialize(parent);
        RectTrs = (RectTransform)transform;
    }

    public void Setup(Note data, RectTransform spawn, FlowLongMeta flowMeta = null)
    {
        NoteData = data;
        _spawnRect = spawn;
        _flowMeta = flowMeta;

        LayoutLongNote();   // 헤드/테일 기본 배치
    }

    public void UpdateCursor(int currentTick)
    {
        Vector2 pos = GetLocalPositionAtTick(currentTick);
        _imgCursor.rectTransform.anchoredPosition = pos;
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

    public Vector2 GetLocalPositionAtTick(int songTick)
    {        
        if (_flowMeta == null)
        {
            // 아직 시작 전이면 Head 위치
            if (songTick <= StartTick)
                return _imgHead.rectTransform.anchoredPosition;

            // 끝난 뒤면 Tail 위치
            if (songTick >= EndTick)
                return _imgTail.rectTransform.anchoredPosition;

            float ratio = (float)(songTick - StartTick) / (EndTick - StartTick);
            ratio = Mathf.Clamp01(ratio);

            float startX = _imgHead.rectTransform.anchoredPosition.x;
            float endX = _imgTail.rectTransform.anchoredPosition.x;

            float x = Mathf.Lerp(startX, endX, ratio);
            float y = _imgHead.rectTransform.anchoredPosition.y; // 직선 롱노트라 y는 고정

            return new Vector2(x, y);
        }
        else
        {
            // FlowLong 버전
            float t = Mathf.InverseLerp(StartTick, EndTick, songTick);
            t = Mathf.Clamp01(t);

            // 곡선 y01 계산 (FlowLongUtil.EvaluateY01 같은 헬퍼 사용)
            float y01 = FlowLongUtil.EvaluateY01(_flowMeta, t);

            // x는 Tick 기반, y는 곡선 기반
            return GetNoteAnchoredPos(songTick, y01);
        }
    }
}
