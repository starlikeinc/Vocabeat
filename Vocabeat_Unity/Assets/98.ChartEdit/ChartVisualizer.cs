using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChartVisualizer : MonoBehaviour
{
    [SerializeField] private RectTransform _targetRect; // 선/노트가 그려질 영역

    [Header("Page Tick Settings")]
    [SerializeField] private int _ticksPerPage = 960; // 1 페이지당 Tick 수

    [Header("Grid Settings")]
    [Range(1,16)]
    [SerializeField] private int _horizonDivisionCount = 8;   // 가로 선 개수    
    [Range(1,16)]
    [SerializeField] private int _verticalDivisionCount = 16;  // 세로 선 개수

    [Header("Song Info")]
    [SerializeField] private TMP_Text TextSongName;
    [SerializeField] private TMP_Text TextSongBPM;
    [SerializeField] private TMP_Text TextDifficulty;

    [Header("Editor Info")]
    [SerializeField] private TMP_Text TextCurPage;
    [SerializeField] private TMP_Text TextLastPage;

    [Header("Line Prefab")]
    [SerializeField] private LineHorizontal _horizontalLinePrefab; // 가로 선
    [SerializeField] private LineVertical _verticalLinePrefab;   // 세로 선

    [Header("Note Ghost")]
    [SerializeField] private RectTransform _noteGhostPrefab; // 노트 미리보기용 프리팹(반투명 이미지 등)

    private readonly List<LineHorizontal> _spawnedHorizonLines = new();
    private readonly List<LineVertical> _spawnedVertLines = new();

    // 스냅용 그리드 좌표
    private readonly List<float> _gridXs = new(); // 세로선들의 X
    private readonly List<float> _gridYs = new(); // 가로선들의 Y

    private RectTransform _noteGhostInstance;

    // ========================================
    private void OnValidate()
    {
        if (_targetRect == null)
            _targetRect = transform as RectTransform;
    }

    private void Start()
    {
        if (!Application.isPlaying)
            return;

        RegenerateLines();
        EnsureGhostInstance();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (!Application.isPlaying)
            return;

        RegenerateLines();
    }

    private void Update()
    {
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

        // ---- 가로선 생성 (_horizonDivisionCount - 1 개) ----
        if (_horizontalLinePrefab != null && _horizonDivisionCount > 0)
        {
            for (int i = 1; i <= _horizonDivisionCount - 1; i++)
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

        // ---- 세로선 생성 (_verticalDivisionCount - 1 개) ----
        if (_verticalLinePrefab != null && _verticalDivisionCount > 0)
        {
            for (int i = 1; i <= _verticalDivisionCount - 1; i++)
            {
                float t = i / (float)(_verticalDivisionCount);
                float x = Mathf.Lerp(-width * 0.5f, width * 0.5f, t);

                var line = Instantiate(_verticalLinePrefab, _targetRect);
                var rt = (RectTransform)line.transform;

                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);

                // 세로로 꽉 채우고, 가로 두께는 프리팹 값 유지
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
                rt.anchoredPosition = new Vector2(x, 0f);

                _spawnedVertLines.Add(line);
                _gridXs.Add(x);

                int tick = Mathf.RoundToInt(_ticksPerPage * t);
                line.VertLineSetting(tick);
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
    private void EnsureGhostInstance()
    {
        if (!Application.isPlaying)
            return;

        if (_noteGhostPrefab == null || _targetRect == null)
            return;

        if (_noteGhostInstance == null)
        {
            _noteGhostInstance = Instantiate(_noteGhostPrefab, _targetRect);
        }

        _noteGhostInstance.gameObject.SetActive(false);
    }

    private void UpdateGhost()
    {
        if (!Application.isPlaying)
            return;

        if (_targetRect == null || _noteGhostPrefab == null)
            return;

        if (_noteGhostInstance == null)
            EnsureGhostInstance();

        var canvas = _targetRect.GetComponentInParent<Canvas>();
        var cam = canvas != null ? canvas.worldCamera : null;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _targetRect,
                Input.mousePosition,
                cam,
                out var localPoint))
        {
            if (_noteGhostInstance != null)
                _noteGhostInstance.gameObject.SetActive(false);
            return;
        }

        // 타겟 Rect 안쪽인지 체크
        var rect = _targetRect.rect;
        if (!rect.Contains(localPoint))
        {
            _noteGhostInstance.gameObject.SetActive(false);
            return;
        }

        float snappedX = _gridXs.Count > 0 ? FindNearest(_gridXs, localPoint.x) : localPoint.x;
        float snappedY = _gridYs.Count > 0 ? FindNearest(_gridYs, localPoint.y) : localPoint.y;

        _noteGhostInstance.anchoredPosition = new Vector2(snappedX, snappedY);
        _noteGhostInstance.gameObject.SetActive(true);
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
    // 곡 정보 세팅 (제목 / BPM)
    public void VisualizerSetting(SongDataSO songDataSO)
    {
        if (songDataSO == null)
            return;

        if (TextSongName != null)
            TextSongName.text = songDataSO.SongName;
        
        if (TextSongBPM != null)
            TextSongBPM.text = $"{songDataSO.BPM} BPM";
    }

    // 에디터 정보 세팅 (난이도 / 현재 페이지 / 마지막 페이지)
    public void SetEditorInfo(EDifficulty difficulty, int curPageIndex, int lastPageIndex)
    {
        if (TextDifficulty != null)
            TextDifficulty.text = difficulty.ToString();

        if (TextCurPage != null)
            TextCurPage.text = (curPageIndex + 1).ToString();  // 0-based -> 1-based

        if (TextLastPage != null)
            TextLastPage.text = (lastPageIndex + 1).ToString();
    }
}
