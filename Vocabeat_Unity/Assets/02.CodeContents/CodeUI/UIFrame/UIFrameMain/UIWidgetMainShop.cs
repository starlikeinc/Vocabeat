using LUIZ.UI;
using TMPro;
using UnityEngine;

public class UIWidgetMainShop : UIWidgetCanvasBase
{
    [Header("Key")]
    [SerializeField] private TMP_Text _textKeyValue;

    [Header("Point")]
    [SerializeField] private TMP_Text _textPointValue;

    private UIFrameMain _frameMain;

    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);
        _frameMain = parentFrame as UIFrameMain;
    }

    protected override void OnUIWidgetShow()
    {
        base.OnUIWidgetShow();
        RefreshCurrency();
    }

    public void RefreshCurrency()
    {
        _textKeyValue.text = $"{ManagerRhythm.Instance.MusicKey}";
        _textPointValue.text = $"{ManagerRhythm.Instance.MusicPoint}";
    }

    public void OnClose()
    {
        _frameMain.DoShowCarousel();
        DoUIWidgetHide();
    }
}
