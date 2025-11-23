using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum ENoteVisualType
{
    Normal,
    Place_Start,
    Place_End,
    Curve,
}

public class ChartVisualizer : MonoBehaviour
{
    [SerializeField] private RectTransform _targetRect; // 선/노트가 그려질 영역

    [SerializeField] private Canvas _targetRectCanvas;

    [Header("Grid Settings")]
    [Min(1)]
    [SerializeField] private int _horizonDivisionCount = 16;
    [Min(1)]
    [SerializeField] private int _verticalDivisionCount = 16;

    [Header("Page Settings")]
    [SerializeField] private int _ticksPerPage = 960;

    [Header("Song Info")]
    [SerializeField] private TMP_Text TextSongName;
    [SerializeField] private TMP_Text TextSongBPM;
    [SerializeField] private TMP_Text TextSongDiffLevel;

    [Header("Editor Info")]
    [SerializeField] private TMP_Text TextCurPage;
    [SerializeField] private TMP_Text TextLastPage;
    [SerializeField] private TMP_Text TextDifficulty;
    [SerializeField] private TMP_Text TextEditMode;

    [Header("Line Prefab")]
    [SerializeField] private LineHorizontal _horizontalLinePrefab;
    [SerializeField] private LineVertical _verticalLinePrefab;

    [Header("Note Visual")]
    [SerializeField] private NotePreview _notePreView;  // 실제 노트 표시용
    [SerializeField] private NoteGhost _noteGhost;      // 마우스 따라다니는 고스트

    [Header("FlowHold Preview")]
    [SerializeField] private LineDrawer _flowHoldCurvePrefab; // 곡선 라인용

    [Header("Edit Link")]
    [SerializeField] private ChartEdit _chartEdit;     

    private readonly List<LineHorizontal> _spawnedHorizonLines = new();
    private readonly List<LineVertical> _spawnedVertLines = new();
    private readonly List<NotePreview> _spawnedNotes = new();
    private readonly List<LineDrawer> _spawnedFlowCurves = new();

    private readonly List<float> _gridXs = new();
    private readonly List<float> _gridYs = new();

    public int TicksPerPage => _ticksPerPage;

    private EEditState _curEditState;
    private int _currentPageIndex = 0;

    // ========================================
    private void OnValidate()
    {
        if (_targetRect == null)
            _targetRect = transform as RectTransform;
        // 에디터 모드에선 아무 것도 생성하지 않음
    }

    private void Start()
    {
        if (!Application.isPlaying)
            return;        

        RegenerateLines();
        EnsureGhostInstance();
    }

    public void Initialize(ChartEdit editor)
    {
        _chartEdit = editor;

        if (_chartEdit != null)
        {
            _chartEdit.OnEditStateChanged += OnEditStateChanged;
        }
    }

    private void OnEditStateChanged(EEditState state)
    {
        if (TextEditMode != null)
            TextEditMode.text = $"EditMode\n{state}";

        // 에디트 모드 바뀔 때마다 고스트 비주얼도 갱신
        SetGhostNoteType(state);
    }

    private void OnRectTransformDimensionsChange()
    {
        if (!Application.isPlaying)
            return;

        RegenerateLines();        
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        UpdateGhost();        
    }

    // ========================================
    public void RegenerateLines()
    {
        if (!Application.isPlaying)
            return;

        if (_targetRect == null)
            return;

        ClearLines();
        _gridXs.Clear();
        _gridYs.Clear();

        var rect = _targetRect.rect;
        float width = rect.width;
        float height = rect.height;

        // ---- 가로선 ----
        if (_horizontalLinePrefab != null && _horizonDivisionCount > 0)
        {
            for (int i = 0; i <= _horizonDivisionCount; i++)
            {
                float t = i / (float)(_horizonDivisionCount);
                float y = Mathf.Lerp(-height * 0.5f, height * 0.5f, t);

                var line = Instantiate(_horizontalLinePrefab, _targetRect);
                var rt = (RectTransform)line.transform;

                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);

                rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);
                rt.anchoredPosition = new Vector2(0f, y);

                _spawnedHorizonLines.Add(line);
                _gridYs.Add(y);

                float normalizedY = Mathf.InverseLerp(-height * 0.5f, height * 0.5f, y);
                line.LineHorizonSetting(normalizedY);
            }
        }

        int startTickOfPage = _currentPageIndex * _ticksPerPage;

        // ---- 세로선 ----
        if (_verticalLinePrefab != null && _verticalDivisionCount > 0)
        {
            for (int i = 0; i <= _verticalDivisionCount; i++)
            {
                float t = i / (float)(_verticalDivisionCount);

                float x = Mathf.Lerp(-width * 0.5f, width * 0.5f, t);

                var line = Instantiate(_verticalLinePrefab, _targetRect);
                var rt = (RectTransform)line.transform;

                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);

                rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
                rt.anchoredPosition = new Vector2(x, 0f);

                _spawnedVertLines.Add(line);
                _gridXs.Add(x);

                // 수정된 부분!
                int localTick = Mathf.RoundToInt(_ticksPerPage * t);
                int pageTick = startTickOfPage + localTick;

                line.VertLineSetting(pageTick);
            }
        }
    }

    private void ClearLines()
    {
        foreach (var h in _spawnedHorizonLines)
        {
            if (h == null) continue;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(h.gameObject);
            else
                Destroy(h.gameObject);
#else
            Destroy(h.gameObject);
#endif
        }
        _spawnedHorizonLines.Clear();

        foreach (var v in _spawnedVertLines)
        {
            if (v == null) continue;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(v.gameObject);
            else
                Destroy(v.gameObject);
#else
            Destroy(v.gameObject);
#endif
        }
        _spawnedVertLines.Clear();
    }

    // ========================================
    private void ClearNotes()
    {
        // 노트 아이콘들 삭제
        foreach (var n in _spawnedNotes)
        {
            if (n == null) continue;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(n.gameObject);
            else
                Destroy(n.gameObject);
#else
        Destroy(n.gameObject);
#endif
        }
        _spawnedNotes.Clear();

        // FlowHold 곡선 라인들 삭제
        foreach (var line in _spawnedFlowCurves)
        {
            if (line == null) continue;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(line.gameObject);
            else
                Destroy(line.gameObject);
#else
        Destroy(line.gameObject);
#endif
        }
        _spawnedFlowCurves.Clear();
    }

    private void EnsureGhostInstance()
    {
        if (!Application.isPlaying)
            return;

        if (_noteGhost == null || _targetRect == null)
            return;

        _noteGhost.gameObject.SetActive(false);
    }

    private void UpdateGhost()
    {
        if (!Application.isPlaying)
            return;

        if (_targetRect == null || _noteGhost == null)
            return;
        
        var cam = _targetRectCanvas != null ? _targetRectCanvas.worldCamera : null;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_targetRect, Input.mousePosition, cam, out var localPoint))
        {
            if (_noteGhost != null)
                _noteGhost.gameObject.SetActive(false);
            return;
        }

        var rect = _targetRect.rect;
        if (!rect.Contains(localPoint))
        {
            _noteGhost.gameObject.SetActive(false);
            return;
        }

        Vector2 centeredPoint = localPoint - rect.center;

        float snappedX = _gridXs.Count > 0 ? FindNearest(_gridXs, centeredPoint.x) : centeredPoint.x;
        float snappedY = _gridYs.Count > 0 ? FindNearest(_gridYs, centeredPoint.y) : centeredPoint.y;

        _noteGhost.RectTrs.anchoredPosition = new Vector2(snappedX, snappedY);
        _noteGhost.gameObject.SetActive(true);
    }

    public bool TryGetGhostNoteData(out int pageIndex, out int tick, out float yNorm)
    {
        pageIndex = 0;
        tick = 0;
        yNorm = 0f;

        if (_targetRect == null || _noteGhost == null)
            return false;

        if (!_noteGhost.gameObject.activeSelf)
            return false;

        var rect = _targetRect.rect;
        Vector2 anchored = _noteGhost.RectTrs.anchoredPosition;
        
        float tX = Mathf.InverseLerp(rect.xMin, rect.xMax, anchored.x);
        tX = Mathf.Clamp01(tX);

        int localTick = Mathf.RoundToInt(_ticksPerPage * tX);
        int startTickOfPage = _currentPageIndex * _ticksPerPage;
        tick = startTickOfPage + localTick;

        float halfH = rect.height * 0.5f;
        
        float tY = Mathf.InverseLerp(-halfH, +halfH, anchored.y);
        yNorm = Mathf.Clamp01(tY);

        pageIndex = _currentPageIndex;
        return true;
    }

    private float FindNearest(List<float> list, float value)
    {
        if (list == null || list.Count == 0)
            return value;

        float best = list[0];
        float bestDist = Mathf.Abs(best - value);

        for (int i = 1; i < list.Count; i++)
        {
            float d = Mathf.Abs(list[i] - value);
            if (d < bestDist)
            {
                bestDist = d;
                best = list[i];
            }
        }
        return best;
    }

    // ========================================
    public void VisualizerSetting(SongDataSO songDataSO)
    {
        if (songDataSO == null)
            return;

        if (TextSongName != null)
            TextSongName.text = songDataSO.SongName;

        if (TextSongBPM != null)
            TextSongBPM.text = $"{songDataSO.BPM} BPM";
    }

    public void UpdateDifficultyDisplay(EDifficulty diff, int diffValue)
    {
        if (TextDifficulty != null)
            TextDifficulty.text = diff.ToString();

        if (TextSongDiffLevel != null)
            TextSongDiffLevel.text = $"Lv. {Mathf.Max(1, diffValue)}";
    }

    // ChartEdit에서 매 페이지 이동마다 호출
    public void RefreshPageView(EDifficulty difficulty, int curPageIndex, int lastPageIndexWithNote, IList<Note> notesForDiff)
    {
        if (!Application.isPlaying)
            return;

        if (_targetRect == null)
            return;

        _currentPageIndex = curPageIndex;
        RegenerateLines();

        // 에디터 정보 표시
        int level = 1;
        if (_chartEdit != null)
            level = Mathf.Max(1, _chartEdit.CurrentDifficultyLevel);

        UpdateDifficultyDisplay(difficulty, level);

        if (TextCurPage != null)
            TextCurPage.text = (curPageIndex + 1).ToString();

        if (TextLastPage != null)
            TextLastPage.text = (lastPageIndexWithNote + 1).ToString();

        ClearNotes();

        if (_notePreView == null || notesForDiff == null)
            return;

        var rect = _targetRect.rect;
        float xMin = rect.xMin;
        float xMax = rect.xMax;
        float yMin = rect.yMin;
        float yMax = rect.yMax;

        int startTickOfPage = curPageIndex * _ticksPerPage;

        foreach (var n in notesForDiff)
        {
            if (n == null)
                continue;
            if (n.PageIndex != curPageIndex)
                continue;

            // Normal / 기타 단일 노트
            if (n.NoteType != ENoteType.FlowHold)
            {
                SpawnSingleNotePreview(n, rect, startTickOfPage);
            }
            else
            {
                // FlowHold 노트 전용: Start/End/CurvePoint + 곡선까지 그리기
                SpawnFlowHoldPreview(n, rect, startTickOfPage);
            }
        }
    }

    private void SpawnSingleNotePreview(Note n, Rect rect, int startTickOfPage)
    {
        if (_notePreView == null)
            return;

        int localTick = n.Tick - startTickOfPage;
        float t = Mathf.Clamp01((float)localTick / _ticksPerPage);
        float x = Mathf.Lerp(rect.xMin, rect.xMax, t);

        float yNorm = Mathf.Clamp01(n.Y);
        float y = Mathf.Lerp(rect.yMin, rect.yMax, yNorm);
        float centeredY = y - rect.center.y;

        var inst = Instantiate(_notePreView, _targetRect);
        var rt = inst.RectTrs;

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, centeredY);

        // Normal 포함한 단일 노트들은 기존 로직 사용
        inst.NoteEditVisualSetting(n);

        _spawnedNotes.Add(inst);
    }

    private void SpawnFlowHoldPreview(Note n, Rect rect, int startTickOfPage)
    {
        if (_notePreView == null)
            return;

        int startTick = n.Tick;
        int endTick = n.Tick + Mathf.Max(0, n.HoldTick);
        if (endTick <= startTick)
        {
            // HoldTick 없는 경우: 그냥 시작점 하나만 보여줌
            SpawnNoteMarker(startTick, n.Y, rect, startTickOfPage, ENoteVisualType.Place_Start);
            return;
        }

        FlowLongMeta meta = n.FlowLongMeta;

        var control = (meta != null) ? meta.CurvePoints : null;

        // --- Start 마커 ---
        float startY01 = n.Y;
        if (meta != null && meta.CurvePoints != null && meta.CurvePoints.Count > 0)
        {
            // t가 0에 가장 가까운 포인트 사용
            float bestT = float.MaxValue;
            float bestY = startY01;
            foreach (var cp in meta.CurvePoints)
            {
                float dt = Mathf.Abs(cp.t - 0f);
                if (dt < bestT)
                {
                    bestT = dt;
                    bestY = cp.y01;
                }
            }
            startY01 = bestY;
        }
        SpawnNoteMarker(startTick, startY01, rect, startTickOfPage, ENoteVisualType.Place_Start);

        // --- End 마커 ---
        float endY01 = n.Y;
        if (meta != null && meta.CurvePoints != null && meta.CurvePoints.Count > 0)
        {
            float bestT = float.MaxValue;
            float bestY = endY01;
            foreach (var cp in meta.CurvePoints)
            {
                float dt = Mathf.Abs(cp.t - 1f);
                if (dt < bestT)
                {
                    bestT = dt;
                    bestY = cp.y01;
                }
            }
            endY01 = bestY;
        }
        SpawnNoteMarker(endTick, endY01, rect, startTickOfPage, ENoteVisualType.Place_End);

        // --- CurvePoint 마커들 ---
        if (meta != null && meta.CurvePoints != null)
        {
            foreach (var cp in meta.CurvePoints)
            {
                if (cp.t <= 0f || cp.t >= 1f)
                    continue; // 0/1 근처는 Start/End가 이미 표시

                int cpTick = Mathf.RoundToInt(Mathf.Lerp(startTick, endTick, cp.t));
                SpawnNoteMarker(cpTick, cp.y01, rect, startTickOfPage, ENoteVisualType.Curve);
            }
        }

        // --- 곡선 라인 프리뷰 (옵션) ---
        SpawnFlowHoldCurveLine(n, rect, startTickOfPage);
    }

    private NotePreview SpawnNoteMarker(int tick, float y01, Rect rect, int startTickOfPage, ENoteVisualType visualType)
    {
        if (_notePreView == null)
            return null;

        int localTick = tick - startTickOfPage;
        if (localTick < 0 || localTick > _ticksPerPage)
            return null; // 이 페이지 밖이면 스킵

        float t = Mathf.Clamp01((float)localTick / _ticksPerPage);
        float x = Mathf.Lerp(rect.xMin, rect.xMax, t);

        y01 = Mathf.Clamp01(y01);
        float y = Mathf.Lerp(rect.yMin, rect.yMax, y01);
        float centeredY = y - rect.center.y;

        var inst = Instantiate(_notePreView, _targetRect);
        var rt = inst.RectTrs;

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, centeredY);

        inst.SetVisualByType(visualType);

        _spawnedNotes.Add(inst);
        return inst;
    }

    private void SpawnFlowHoldCurveLine(Note n, Rect rect, int startTickOfPage)
    {
        if (_flowHoldCurvePrefab == null)
            return;

        int startTick = n.Tick;
        int endTick = n.Tick + Mathf.Max(0, n.HoldTick);
        if (endTick <= startTick)
            return;

        FlowLongMeta meta = n.FlowLongMeta;

        var line = Instantiate(_flowHoldCurvePrefab, _targetRect);
        var rt = (RectTransform)line.transform;

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        List<Vector2> points = new List<Vector2>();
        const int resolution = 32; // 샘플 포인트 개수

        for (int i = 0; i <= resolution; i++)
        {
            float t01 = i / (float)resolution;

            int tick = Mathf.RoundToInt(Mathf.Lerp(startTick, endTick, t01));
            int localTick = tick - startTickOfPage;
            if (localTick < 0 || localTick > _ticksPerPage)
                continue; // 페이지 밖이면 그리지 않음

            float xT = Mathf.Clamp01((float)localTick / _ticksPerPage);
            float x = Mathf.Lerp(rect.xMin, rect.xMax, xT);

            float y01;
            if (meta != null && meta.CurvePoints != null && meta.CurvePoints.Count > 0)
            {
                y01 = NoteUtility.EvaluateFlowHoldY(meta, t01);
            }
            else
            {
                y01 = Mathf.Clamp01(n.Y);
            }

            float y = Mathf.Lerp(rect.yMin, rect.yMax, y01);
            float centeredY = y - rect.center.y;

            points.Add(new Vector2(x, centeredY));
        }

        line.useExternalPoints = true;
        line.externalPoints = points;
        line.SetVerticesDirty();

        _spawnedFlowCurves.Add(line);
    }

    // === 노트 타입 비주얼 컨트롤 =====================
    public void SetGhostNoteType(EEditState editState)
    {
        if (!Application.isPlaying)
            return;

        if (_noteGhost == null)
            return;

        _curEditState = editState;
        ChangeGhostNoteType();
    }

    private void ChangeGhostNoteType()
    {
        if (_noteGhost == null)
            return;

        ENoteVisualType visualType = ENoteVisualType.Normal;

        switch (_curEditState)
        {
            case EEditState.Nomral:
            case EEditState.None:
            default:
                visualType = ENoteVisualType.Normal;
                break;

            case EEditState.Long_Place:
                // FlowHold Place 모드:
                // 아직 Start 안 찍었으면 → Start 고스트
                // Start 찍었으면 → End 고스트
                if (_chartEdit != null && !_chartEdit.IsNoStartNote)
                    visualType = ENoteVisualType.Place_End;
                else
                    visualType = ENoteVisualType.Place_Start;
                break;

            case EEditState.Long_Curve:
                visualType = ENoteVisualType.Curve;
                break;
        }

        _noteGhost.SetVisualByType(visualType);
    }

    public NoteGhost GetGhost() => _noteGhost;
}
