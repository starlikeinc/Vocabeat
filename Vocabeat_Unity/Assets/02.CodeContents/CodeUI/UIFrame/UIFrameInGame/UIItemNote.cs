using LUIZ.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIItemNote : UITemplateItemBase
{
    [SerializeField] private Image _noteImage;  // 실제 노트 이미지
    [SerializeField] private RectTransform _rect; // 이 노트의 RectTransform

    public Note Data { get; private set; }

    public void Init(Note data, RectTransform parentRect)
    {
        Data = data;

        if (_rect == null)
            _rect = (RectTransform)transform;

        // Y는 0~1 비율이라고 했으니까, 부모 높이에 맞게 위치 계산
        float parentHeight = parentRect.rect.height;
        Vector2 pos = _rect.anchoredPosition;
        pos.y = (data.Y - 0.5f) * parentHeight; // 가운데 기준으로 하고 싶으면 이런 식 
        _rect.anchoredPosition = pos;

        // 타입에 따라 색을 다르게 한다든지, 이미지 바꾸고 싶으면 여기서
        // if (Data.NoteType == ENoteType.Normal) { ... }
    }
}
