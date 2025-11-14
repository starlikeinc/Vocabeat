using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UISafeArea : MonoBehaviour
{
    [SerializeField]
    [HideInInspector]
    private RectTransform rectTransfrom;

    [SerializeField]
    private bool conformX = true;
    [SerializeField]
    private bool conformY = true;

    private Rect lastSafeArea = new Rect(0, 0, 0, 0);
    private Vector2Int lastScreenSize = new Vector2Int(0, 0);

    #region UNITY_EDITOR
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (rectTransfrom == null)
        {
            rectTransfrom = GetComponent<RectTransform>();
        }
    }
#endif
    #endregion

    private void Awake()
    {
        Refresh();
    }

    private void OnRectTransformDimensionsChange()
    {
        Refresh();
    }

    private void Refresh()
    {
        Rect safeArea = Screen.safeArea;
        Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);

        if (safeArea == lastSafeArea && screenSize == lastScreenSize)
            return;

        lastSafeArea = safeArea;
        lastScreenSize = screenSize;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        if (!conformX)
        {
            anchorMin.x = 0;
            anchorMax.x = Screen.width;
        }
        if (!conformY)
        {
            anchorMin.y = 0;
            anchorMax.y = Screen.height;
        }

        if (Screen.width > 0 && Screen.height > 0)
        {
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rectTransfrom.anchorMin = anchorMin;
            rectTransfrom.anchorMax = anchorMax;
        }
    }
}
