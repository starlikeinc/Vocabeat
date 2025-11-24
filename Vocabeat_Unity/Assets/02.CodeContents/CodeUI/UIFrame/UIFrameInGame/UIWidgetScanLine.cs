using LUIZ.UI;
using UnityEngine;

public class UIWidgetScanLine : UIWidgetCanvasBase
{
    [Header("Refs")]
    [SerializeField] private RectTransform ScanLine;    

    private float _leftX;
    private float _rightX;

    private bool _isMoveLine;

    // ========================================        
    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);
        ManagerRhythm.Instance.OnSongStarted -= HandleInitScanline;
        ManagerRhythm.Instance.OnSongStarted += HandleInitScanline;
        ManagerRhythm.Instance.OnSongEnded -= HandleClearScanline;
        ManagerRhythm.Instance.OnSongEnded += HandleClearScanline;
    }

    // ========================================        
    private void Awake()
    {
        if (ScanLine == null) return;

        RectTransform myRectTrs = (RectTransform)transform;        

        float width = myRectTrs.rect.width;
        _leftX = -width * 0.5f;
        _rightX = width * 0.5f;
    }

    // ========================================        
    public void UpdateScanline(float pageT)
    {
        if (!_isMoveLine)
            return;

        SetPageProgress(pageT);
    }    

    public void ResetPosition()
    {
        SetPageProgress(0f);
    }

    // ========================================        
    private void SetPageProgress(float pageT)
    {
        float t = Mathf.Clamp01(pageT);
        var pos = ScanLine.anchoredPosition;
        pos.x = Mathf.Lerp(_leftX, _rightX, t);
        ScanLine.anchoredPosition = pos;
    }

    private void HandleInitScanline()
    {
        _isMoveLine = true;
        ResetPosition();
        DoUIWidgetShow();
    }

    private void HandleClearScanline()
    {
        _isMoveLine = false;
        DoUIWidgetHide();
    }
}
