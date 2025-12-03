using UnityEngine;

public enum EMainSfxKey
{    
    Slide,        
    BtnClick,    
    MenuSelect,
}

public class UIFrameMain : UIFrameUsage<EMainSfxKey>
{
    [SerializeField] private UIWidgetMainShop _widgetMainShop;

    [SerializeField] private UICarouselMainMenu _carouselMainMenu;

    protected override void OnUIFrameShow()
    {
        base.OnUIFrameShow();
        Debug.Log($"{name} Show");

        PlayFrameBgm();
    }

    public void DoShowCarousel()
    {
        _carouselMainMenu.DoUIWidgetShow();
    }

    public void OnGoToSongMenu()
    {
        StopFrameBgm();
        UIChannel.UIShow<UIFrameBlinder>().BlindWithNextStep(() =>
        {            
            UIChannel.UIHide<UIFrameMain>();
            UIChannel.UIShow<UIFrameSongMenu>().DoFrameSongMenuSetting(true);
        });        
    }

    public void OnShopPopupOpen()
    {
        _carouselMainMenu.DoUIWidgetHide();
        _widgetMainShop.DoUIWidgetShow();
    }
}
