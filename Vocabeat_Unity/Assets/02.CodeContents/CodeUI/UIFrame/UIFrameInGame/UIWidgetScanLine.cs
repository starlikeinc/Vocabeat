using LUIZ.UI;
using UnityEngine;

public class UIWidgetScanLine : UIWidgetBase
{
    [Header("Refs")]
    [SerializeField] private RectTransform ScanLine;
    [SerializeField] private RhythmTimeline Timeline;

    private float _leftX;
    private float _rightX;

    private void Awake()
    {
        if (ScanLine == null) return;

        var parent = ScanLine.parent as RectTransform;
        if (parent == null) return;

        float width = parent.rect.width;
        _leftX = -width * 0.5f;
        _rightX = width * 0.5f;
    }

    private void Update()
    {
        if (ScanLine == null || Timeline == null || !Timeline.IsPlaying)
            return;

        SetPageProgress(Timeline.PageT);
    }

    public void SetPageProgress(float pageT)
    {
        float t = Mathf.Clamp01(pageT);
        var pos = ScanLine.anchoredPosition;
        pos.x = Mathf.Lerp(_leftX, _rightX, t);
        ScanLine.anchoredPosition = pos;
    }

    public void ResetPosition()
    {
        SetPageProgress(0f);
    }
}
