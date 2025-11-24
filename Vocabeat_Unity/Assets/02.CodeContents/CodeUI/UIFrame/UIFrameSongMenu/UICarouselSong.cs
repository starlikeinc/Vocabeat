using System;
using System.Collections.Generic;
using LUIZ.UI;
using UnityEngine;

public class UICarouselSong : UITemplateCarouselBase<UIItemSongSlot, SongDataSO>
{    
    private UIFrameSongMenu _frameSongMenu;
    private SongDatabaseSO _songDatabase;

    #region 데이터/바인딩 구현

    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);
        _frameSongMenu = parentFrame as UIFrameSongMenu;
    }

    protected override int GetItemCount()
    {
        return _songDatabase != null && _songDatabase.Songs != null
            ? _songDatabase.Songs.Count
            : 0;
    }

    protected override SongDataSO GetItemData(int index)
    {
        if (_songDatabase == null || _songDatabase.Songs == null)
            return null;

        // 래핑해서 안전하게
        index = WrapIndex(index, _songDatabase.Songs.Count);
        return _songDatabase.GetSong(index);
    }

    protected override void BindItemVisual(UIItemSongSlot slot, int itemIndex, SongDataSO data, bool isCenter)
    {
        if (data == null) return;
        slot.SetVisual(data);
    }

    protected override void OnSlotCreated(UIItemSongSlot slot)
    {
        slot.OnClick += HandleSlotClick;
    }

    protected override void OnUnsubscribeSlotEvents(UIItemSongSlot slot)
    {
        slot.OnClick -= HandleSlotClick;
    }

    private void HandleSlotClick(UIItemSongSlot slot)
    {
        OnClickSlot(slot);
    }

    protected override void OnCenterIndexChanged(int newIndex, SongDataSO data)
    {
        if (_frameSongMenu == null || data == null)
            return;
        
        _frameSongMenu.SetSelectedSong(newIndex);
    }

    #endregion

    protected override void OnClickNext()
    {
        base.OnClickNext();
        _frameSongMenu.PlayFrameSfx(ESongMenuSfxKey.Slide);
    }

    protected override void OnClickPrev()
    {
        base.OnClickPrev();
        _frameSongMenu.PlayFrameSfx(ESongMenuSfxKey.Slide);
    }

    #region 외부 초기화

    /// <summary>
    /// 외부에서 SongDB를 설정해주고 싶을 때 사용.
    /// (인스펙터에 바로 할당해도 되지만, 코드로도 가능)
    /// </summary>
    public void Initialize(int initialCenterIndex = 0)
    {
        _songDatabase = ManagerRhythm.Instance.SongDB;
        RefreshAll(initialCenterIndex);
    }

    #endregion
}
