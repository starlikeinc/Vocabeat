using LUIZ.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIWidgetMainShop : UIWidgetCanvasBase
{
    [SerializeField] private RectTransform _keyLayout;
    [SerializeField] private TMP_Text _textKeyValue;

    private UIFrameMain _frameMain;

    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);
        _frameMain = parentFrame as UIFrameMain;
    }

    protected override void OnUIWidgetShow()
    {
        base.OnUIWidgetShow();
        RefreshKey();
    }

    public void RefreshKey()
    {
        _textKeyValue.text = $"{ManagerRhythm.Instance.MusicKey}";
        LayoutRebuilder.ForceRebuildLayoutImmediate(_keyLayout);
    }

    public void OnClose()
    {
        _frameMain.DoShowCarousel();
        DoUIWidgetHide();
    }
}
