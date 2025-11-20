using LUIZ.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIItemGameScore : UITemplateItemBase
{
    [SerializeField] private Image _imgScore;

    [SerializeField] private LayoutElement _layoutElement;

    [SerializeField] private Vector2 _sizeNumber;
    [SerializeField] private Vector2 _sizeComma;    

    public void SetScoreImage(Sprite sprite, bool isComma)
    {
        _imgScore.overrideSprite = sprite;

        if (isComma)
            ResizeComma();
        else
            ResizeNumber();
    }

    private void ResizeComma()
    {

    }

    private void ResizeNumber()
    {

    }
}
