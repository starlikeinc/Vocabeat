using LUIZ.UI;
using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class MainMenuData
{
    public Sprite MenuIcon;
    public string MenuName;

    public UnityEvent OnClicked;
}

public class UICarouselMainMenu : UITemplateCarouselBase<UICarouselItemMenu, MainMenuData>
{
    [SerializeField] private MainMenuData[] _menuDatas;

    private UIFrameMain _frameMain;

    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);
        _frameMain = parentFrame as UIFrameMain;
    }

    protected override void BindItemVisual(UICarouselItemMenu slot, int itemIndex, MainMenuData data, bool isCenter)
    {
        if (slot == null) return;
        slot.SetData(data, isCenter);
    }

    protected override int GetItemCount()
    {
        return _menuDatas?.Length ?? 0;
    }

    protected override MainMenuData GetItemData(int index)
    {
        return _menuDatas[index];
    }

    protected override void OnCenterIndexChanged(int newIndex, MainMenuData data)
    {
        base.OnCenterIndexChanged(newIndex, data);
        // 예: 현재 선택된 메뉴에 따라 설명 텍스트나 다른 UI 갱신
    }

    protected override void OnClickNext()
    {
        base.OnClickNext();
        _frameMain.PlayFrameSfx(EMainSfxKey.Slide);
    }

    protected override void OnClickPrev()
    {
        base.OnClickPrev();
        _frameMain.PlayFrameSfx(EMainSfxKey.Slide);
    }

    public void OnClickCenterMenu()
    {
        if (m_itemCount <= 0) return; // 캐러셀 베이스의 protected 필드

        int index = GetCurrentCenterIndex();
        if (index < 0 || index >= _menuDatas.Length) return;

        var data = _menuDatas[index];
        if (data != null && data.OnClicked != null)
        {
            data.OnClicked.Invoke();
            _frameMain.PlayFrameSfx(EMainSfxKey.BtnClick);
        }
    }
}
