using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICarouselItemMenu : UITemplateCarouselItemBase
{
    [Header("UI")]
    [SerializeField] private Image _iconImage;
    //[SerializeField] private TMP_Text _nameText;

    public void SetData(MainMenuData data, bool isCenter)
    {
        if (data == null) return;

        if (_iconImage) _iconImage.sprite = data.MenuIcon;
        //if (_nameText) _nameText.text = data.MenuName;
        
        OnApplyFocusState(isCenter);
    }

    protected override void OnApplyFocusState(bool isCenter)
    {
        // 예: 중앙일 때만 완전 불투명, 나머지는 살짝 투명
        if (_iconImage)
        {
            var c = _iconImage.color;
            c.a = isCenter ? 1f : 0.6f;
            _iconImage.color = c;
        }

        //if (_nameText)
        //{
        //    _nameText.alpha = isCenter ? 1f : 0.6f;
        //}
    }
}
