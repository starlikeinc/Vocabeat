using System.Collections.Generic;
using LUIZ.UI;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LineDrawer))]
public class UIItemNoteFlowHold : UITemplateItemBase, IFlowHoldNote
{
    [Header("Images")]
    [SerializeField] private Image _imgHead;
    [SerializeField] private Image _imgTail;
    [SerializeField] private Image _imgCursor;

    [Header("Flow Line Drawer")]
    [SerializeField] private LineDrawer _lineDrawer;

    public Note NoteData { get; private set; }
    public RectTransform RectTrs { get; private set; }
    public ENoteType NoteType => NoteData.NoteType;

    public int StartTick => NoteData.Tick;
    public int EndTick => NoteData.Tick + NoteData.HoldTick;

    private RectTransform _spawnRect;

    private FlowLongMeta _flowMeta;

    private readonly List<Vector2> _points = new();

    // ========================================
    protected override void OnUIWidgetInitialize(UIFrameBase parent)
    {
        base.OnUIWidgetInitialize(parent);
        RectTrs = (RectTransform)transform;
    }

    // ========================================
    public void Setup(Note data, RectTransform spawn)
    {
        NoteData = data;
        _spawnRect = spawn;
        _flowMeta = data.FlowLongMeta;

        LayoutLongNote();   // 헤드/테일 기본 배치 + 라인 생성
    }

    public void UpdateCursor(int currentTick)
    {
        // 1) 구간 밖이면 Cursor 비활성화
        if (currentTick < StartTick || currentTick > EndTick)
        {
            if (_imgCursor.gameObject.activeSelf)
                _imgCursor.gameObject.SetActive(false);

            return;
        }

        // 2) 구간 안에 들어오면 Cursor 활성화
        if (!_imgCursor.gameObject.activeSelf)
            _imgCursor.gameObject.SetActive(true);

        // 3) 위치 계산
        Vector2 pos = GetLocalPositionAtTick(currentTick);
        _imgCursor.rectTransform.anchoredPosition = pos;
    }

    public Vector2 GetLocalPositionAtTick(int songTick)
    {
        if (_flowMeta == null || _flowMeta.CurvePoints == null || _flowMeta.CurvePoints.Count == 0)
        {
            // 직선 롱노트: Head/Tail 사이를 선형 보간
            float ratio = Mathf.InverseLerp(StartTick, EndTick, songTick);
            ratio = Mathf.Clamp01(ratio);

            float startX = _imgHead.rectTransform.anchoredPosition.x;
            float endX = _imgTail.rectTransform.anchoredPosition.x;
            float x = Mathf.Lerp(startX, endX, ratio);

            float y = _imgHead.rectTransform.anchoredPosition.y; // y 고정

            return new Vector2(x, y);
        }
        else
        {
            // FlowLong 버전: t 기준으로 y01 계산
            float t = Mathf.InverseLerp(StartTick, EndTick, songTick);
            t = Mathf.Clamp01(t);

            float y01 = NoteUtility.EvaluateFlowHoldY(_flowMeta, t);

            // x는 Tick 기반, y는 곡선 기반
            return GetNoteAnchoredPos(songTick, y01);
        }
    }

    // ========================================
    private void LayoutLongNote()
    {
        // 직선 롱노트 (커브 없음)
        if (_flowMeta == null || _flowMeta.CurvePoints == null || _flowMeta.CurvePoints.Count == 0)
        {
            float y01 = NoteData.Y;

            _imgHead.rectTransform.anchoredPosition =
                NoteUtility.GetNotePosition(_spawnRect, StartTick, y01);

            _imgTail.rectTransform.anchoredPosition =
                NoteUtility.GetNotePosition(_spawnRect, EndTick, y01);
        }
        else
        {
            // 커브 롱노트: t=0, t=1 기준으로 Head/Tail 배치
            float y01Head = NoteUtility.EvaluateFlowHoldY(_flowMeta, 0f);
            _imgHead.rectTransform.anchoredPosition =
                NoteUtility.GetNotePosition(_spawnRect, StartTick, y01Head);

            float y01Tail = NoteUtility.EvaluateFlowHoldY(_flowMeta, 1f);
            _imgTail.rectTransform.anchoredPosition =
                NoteUtility.GetNotePosition(_spawnRect, EndTick, y01Tail);
        }

        SetCurvePointAndDraw();
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

    private void SetCurvePointAndDraw()
    {
        _points.Clear();

        // 커브 데이터가 없으면 Head~Tail 두 점만으로 직선
        if (_flowMeta == null || _flowMeta.CurvePoints == null || _flowMeta.CurvePoints.Count == 0)
        {
            _points.Add(_imgHead.rectTransform.anchoredPosition);
            _points.Add(_imgTail.rectTransform.anchoredPosition);
        }
        else
        {
            int resolution = 40;

            for (int i = 0; i <= resolution; i++)
            {
                // 0 ~ 1 사이 비율 (FlowLongMeta 내에서의 진행도)
                float t01 = i / (float)resolution;

                // 1) 이 t01에 해당하는 Tick 계산
                int tick = Mathf.RoundToInt(Mathf.Lerp(StartTick, EndTick, t01));

                // 2) 이 t01에서의 y01(곡선 높이) 계산
                float y01 = NoteUtility.EvaluateFlowHoldY(_flowMeta, t01);

                // 3) Tick + y01 → 실제 UI anchoredPosition 로 변환
                Vector2 localPos = NoteUtility.GetNotePosition(_spawnRect, tick, y01);

                _points.Add(localPos);
            }
        }

        _lineDrawer.useExternalPoints = true;
        _lineDrawer.externalPoints = _points;
        _lineDrawer.SetVerticesDirty();
    }
}
