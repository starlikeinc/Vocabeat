using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChartVisualizer : MonoBehaviour
{
    [SerializeField] private RectTransform _targetRect; // 선을 그릴 캔버스/패널

    [Header("Settings")]
    [Min(1)]
    [SerializeField] private int _divisionCount = 16;   // n등분 (구간 개수)
    [SerializeField] private bool _includeEdges = false; // 0, 1 위치도 선을 그릴지

    [Header("Line Prefab")]
    [SerializeField] private Image _verticalLinePrefab;  // 세로 선
    [SerializeField] private Image _horizontalLinePrefab;// 가로 선

    private readonly List<RectTransform> _spawnedLines = new List<RectTransform>();

    // ========================================
    private void OnValidate()
    {
        if (_targetRect == null)
            _targetRect = transform as RectTransform;

        // 에디터에서 값 바꾸면 바로 갱신
        RegenerateLines();
    }

    private void Start()
    {
        RegenerateLines();
    }

    private void OnRectTransformDimensionsChange()
    {
        // 해상도 / 부모 Rect 변경 시 위치 다시 계산
        RegenerateLines();
    }

    // ========================================
    public void RegenerateLines()
    {
        
    }
}
