using LUIZ.UI;
using UnityEngine;

public class UIScrollSnapTest : UIScrollRectSnapJumpBase
{
    [Header("[ TEST ]")]
    [SerializeField] private int ChildCount = 30;

    [SerializeField] private UITemplateItemBase JumpItem;
    //--------------------------------------------------
    protected override void OnUIWidgetInitializePost(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitializePost(parentFrame);

        for(int i = 0; i < ChildCount; i++)
        {
            UIScrollSnapItemTest item = DoTemplateRequestItem<UIScrollSnapItemTest>(m_scrollRect.content);
            item.gameObject.name = i.ToString();
        }
    }

    protected override void OnUIScrollSnapFinish()
    {
        base.OnUIScrollSnapFinish();

        Debug.Log("SNAP FIN!!!!");
    }

    //--------------------------------------------------
    [ContextMenu("JUMP")]
    public void DoTestJump()
    {
        DoScrollSnapJumpToItem(JumpItem, Vector2.zero);
    }
}
