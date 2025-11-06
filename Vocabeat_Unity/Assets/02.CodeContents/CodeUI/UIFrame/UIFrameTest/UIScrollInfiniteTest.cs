using UnityEngine;
using LUIZ.UI;

public class UIScrollInfiniteTest : UIScrollRectVirtualBase
{
    private int m_index = 0;
    //----------------------------------------------
    protected override void OnUIWidgetInitializePost(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitializePost(parentFrame);

        ProtScrollInfiniteInitialize(51);
    }

    protected override void OnUITemplateRequestItem(UITemplateItemBase item)
    {
        base.OnUITemplateRequestItem(item);

        item.name = m_index.ToString();
        m_index++;
    }

    protected override void OnUIScrollVirtualRefreshItem(int itemIndex, UITemplateItemBase item)
    {
        base.OnUIScrollVirtualRefreshItem(itemIndex, item);

        UITemplateItemTest testItem = (UITemplateItemTest)item;
        testItem.DoTestItemSetting(itemIndex);
    }
}
