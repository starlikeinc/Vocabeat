using LUIZ.UI;
using UnityEngine;

public class UIWidgetScanLine : UIWidgetBase
{
    [Header("Refs")]
    [SerializeField] private RectTransform ScanLine;    

    private float _leftX;
    private float _rightX;    

    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);
    }

    private void Awake()
    {
        if (ScanLine == null) return;

        RectTransform myRectTrs = (RectTransform)transform;        

        float width = myRectTrs.rect.width;
        _leftX = -width * 0.5f;
        _rightX = width * 0.5f;
    }

    public void UpdateScanline(float pageT)
    {
        SetPageProgress(pageT);
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
