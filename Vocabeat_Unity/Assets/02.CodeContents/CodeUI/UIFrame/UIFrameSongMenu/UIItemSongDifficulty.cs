using LUIZ.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItemSongDifficulty : UITemplateItemBase
{
    [SerializeField] private Image _imgDiffIcon;
    [SerializeField] private TMP_Text _textDiffValue;

    public void DoSongDifficultySetting(Sprite icon, int value)
    {
        _imgDiffIcon.overrideSprite = icon;
        _textDiffValue.text = value.ToString();
    }
}
