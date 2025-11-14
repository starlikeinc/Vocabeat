using UnityEngine;

public class FPSVisualizer : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [Range(0.01f, 1f)] public float smoothing = 0.1f; // EMA 가중치
    public bool showMilliseconds = true;

    private float _avgDelta;
    private GUIStyle _style;

    private void Awake()
    {
        _avgDelta = Time.unscaledDeltaTime; // timescale 영향 제거
        _style = new GUIStyle
        {
            fontSize = Mathf.RoundToInt(Screen.dpi > 0 ? Screen.dpi / 6f : 16),
            alignment = TextAnchor.UpperLeft
        };
    }

    private void Update()
    {
        _avgDelta = Mathf.Lerp(_avgDelta, Time.unscaledDeltaTime, smoothing);
    }

    private void OnGUI()
    {
        if (Event.current.type != EventType.Repaint) return;

        float fps = 1f / _avgDelta;
        float ms = _avgDelta * 1000f;
        string text = showMilliseconds ? $"{fps:0.0} FPS  ({ms:0.0} ms)" : $"{fps:0.0} FPS";

        var rect = new Rect(24, 24, 300, 40);
        // 가독성을 위해 아웃라인 한 번 그려주기
        var old = fps > 50f ? Color.green : fps > 40f ? Color.yellow : Color.red;
        _style.normal.textColor = Color.black;
        GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), text, _style);
        _style.normal.textColor = old;
        GUI.Label(rect, text, _style);
    }
#endif
}