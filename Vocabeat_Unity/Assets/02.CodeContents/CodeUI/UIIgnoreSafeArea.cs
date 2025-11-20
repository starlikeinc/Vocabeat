using UnityEngine;

public class UIIgnoreSafeArea : MonoBehaviour
{
    private RectTransform rect;
    private Rect safeArea;
    private Vector2Int screenSize;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        UpdateTransform();
    }

    private void OnEnable()
    {
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        safeArea = Screen.safeArea;
        screenSize = new Vector2Int(Screen.width, Screen.height);

        // SafeArea를 기준으로 설정된 부모 anchor 값을 다시 원본 스크린 기준(0~1)으로 환산
        float minX = -safeArea.x / screenSize.x;
        float minY = -safeArea.y / screenSize.y;
        float maxX = (screenSize.x - safeArea.x - safeArea.width) / screenSize.x;
        float maxY = (screenSize.y - safeArea.y - safeArea.height) / screenSize.y;

        rect.anchorMin = new Vector2(minX, minY);
        rect.anchorMax = new Vector2(1 + maxX, 1 + maxY);

        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
