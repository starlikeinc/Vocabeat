using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIFullScreen : MonoBehaviour
{
    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;

    [SerializeField] 
    bool ignoreTopOffset;
    [SerializeField] 
    bool ignoreBottomOffset;

    private bool isRefreshing; // 무한 루프 방지용

    private void OnValidate()
    {
        if (GetComponent<AspectRatioFitter>() != null)
        {
            Debug.LogWarning($"OtherComponent가 이미 있으므로 {gameObject.name}는 비활성화됩니다.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                DestroyImmediate(this, true);
            };
#endif
        }
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (ManagerUI.Instance != null)
            canvasRectTransform = ManagerUI.Instance.GetRootCanvas().transform as RectTransform;
        else
            canvasRectTransform = GetComponent<RectTransform>();

        Refresh();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (!isRefreshing) // 내부 변경으로 인한 호출은 무시
            Refresh();
    }

    private void Refresh()
    {
        if (canvasRectTransform == null) return;

        isRefreshing = true; // 루프 방지 플래그 설정

        Vector2 canvasSize = canvasRectTransform.rect.size;

        rectTransform.position = canvasRectTransform.position;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, canvasSize.x);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, canvasSize.y);
        
        if (ignoreTopOffset)
        {
            Vector2 max = rectTransform.offsetMax;
            max.y = 0f;
            rectTransform.offsetMax = max;
        }

        if (ignoreBottomOffset)
        {
            Vector2 min = rectTransform.offsetMin;
            min.y = 0f;
            rectTransform.offsetMin = min;
        }

        isRefreshing = false; // 다시 허용
    }
}
