using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChartVisualizer : MonoBehaviour
{
    [SerializeField] private RectTransform _targetRect; // 선/노트가 그려질 영역

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

    [Header("Editor Info")]
    [SerializeField] private TMP_Text TextCurPage;
    [SerializeField] private TMP_Text TextLastPage;
    [SerializeField] private TMP_Text TextDifficulty;

    [Header("Line Prefab")]
    [SerializeField] private LineHorizontal _horizontalLinePrefab;
    [SerializeField] private LineVertical _verticalLinePrefab;

    [Header("Note Visual")]
    [SerializeField] private NotePreview _notePreView;  // 실제 노트 표시용
    [SerializeField] private NoteGhost _noteGhost;      // 마우스 따라다니는 고스트

    [Header("Edit Link")]
    [SerializeField] private ChartEdit _chartEdit;     

    private readonly List<LineHorizontal> _spawnedHorizonLines = new();
    private readonly List<LineVertical> _spawnedVertLines = new();
    private readonly List<NotePreview> _spawnedNotes = new();

    private readonly List<float> _gridXs = new();
    private readonly List<float> _gridYs = new();

    private ENoteType _curNoteType;
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
        HandleMouseInput();
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

        var canvas = _targetRect.GetComponentInParent<Canvas>();
        var cam = canvas != null ? canvas.worldCamera : null;

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

    private void HandleMouseInput()
    {
        if (!Application.isPlaying)
            return;

        if (_chartEdit == null)
            return;

        // 좌클릭: 노트 추가 / 타입 변경
        if (Input.GetMouseButtonDown(0))
        {
            if (TryGetGhostNoteData(out int pageIndex, out int tick, out float yNorm))
            {
                _chartEdit.OnRequestAddOrUpdateNote(tick, yNorm, pageIndex, _curNoteType);
            }
        }

        // 우클릭: 노트 삭제
        if (Input.GetMouseButtonDown(1))
        {
            if (TryGetGhostNoteData(out int pageIndex, out int tick, out float yNorm))
            {
                _chartEdit.OnRequestRemoveNote(tick, yNorm, pageIndex);
            }
        }
    }

    private bool TryGetGhostNoteData(out int pageIndex, out int tick, out float yNorm)
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
        if (TextDifficulty != null)
            TextDifficulty.text = difficulty.ToString();

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

            int localTick = n.Tick - startTickOfPage;
            float t = Mathf.Clamp01((float)localTick / _ticksPerPage);
            float x = Mathf.Lerp(xMin, xMax, t);
            
            float yNorm = Mathf.Clamp01(n.Y);
            float y = Mathf.Lerp(rect.yMin, rect.yMax, yNorm);
            
            float centeredY = y - rect.center.y;

            if (_gridYs != null && _gridYs.Count > 0)
                centeredY = FindNearest(_gridYs, centeredY);
            
            float finalY = centeredY;

            var inst = Instantiate(_notePreView, _targetRect);
            var rt = inst.RectTrs;

            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, finalY);

            inst.NoteEditVisualSetting(n.NoteType);

            _spawnedNotes.Add(inst);
        }
    }

    // === 노트 타입 비주얼 컨트롤 =====================
    public void SetGhostNoteType(ENoteType noteType)
    {
        if (!Application.isPlaying)
            return;
        
        if (_noteGhost == null)
            return;

        _curNoteType = noteType;
        ChangeGhostNoteType();
    }

    public void OnChangeNoteNormal()
    {
        _curNoteType = ENoteType.Normal;
        ChangeGhostNoteType();
    }

    public void OnChangeNoteLongFollow()
    {
        _curNoteType = ENoteType.FlowHold;
        ChangeGhostNoteType();
    }

    public void OnChangeNoteLongHold()
    {
        _curNoteType = ENoteType.LongHold;
        ChangeGhostNoteType();
    }

    private void ChangeGhostNoteType()
    {
        _noteGhost.NoteEditVisualSetting(_curNoteType);
    }
}
