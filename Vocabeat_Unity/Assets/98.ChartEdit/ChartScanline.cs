using UnityEngine;

public class ChartScanline : MonoBehaviour
{
    [SerializeField] private RectTransform _scanline;

    private float _leftX;
    private float _rightX;

    private void Awake()
    {
        RectTransform myRect = (RectTransform)transform;
        float width = myRect.rect.width;

        _leftX = -width * 0.5f;
        _rightX = width * 0.5f;
    }

    /// <summary>
    /// 외부에서 t(0~1)를 넘기면 스캔라인 위치 갱신
    /// </summary>
    public void SetProgress(float t)
    {
        t = Mathf.Clamp01(t);

        if (_scanline == null)
            return;

        Vector2 pos = _scanline.anchoredPosition;
        pos.x = Mathf.Lerp(_leftX, _rightX, t);
        _scanline.anchoredPosition = pos;
    }

    /// <summary>
    /// 초기화(노래 시작 시)
    /// </summary>
    public void ResetPosition()
    {
        SetProgress(0f);
    }
}
