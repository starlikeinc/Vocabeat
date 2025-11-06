using UnityEngine;
using LUIZ.UI;

public class UITemplateTest : UITemplateBase
{
    [Header("Test")]
    [SerializeField] private RectTransform LayoutPivot;

    //-------------------------------------------------

    [ContextMenu("RequestTest")]
    public void DoRequestTestItem()
    {
        UITemplateItemTest item = DoTemplateRequestItem<UITemplateItemTest>(LayoutPivot);

        if (item != null)
            Debug.Log("SUCCESS");
        else
            Debug.Log("FAIL");
    }

    [ContextMenu("ReturnTest")]
    public void DoReturnTestItemAll()
    {
        DoUITemplateReturnAll();
    }
    //-------------------------------------------------
    public void OnBtnLImageEmptyTest()
    {
        Debug.Log("CLICKED!!");
    }
}
