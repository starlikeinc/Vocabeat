using LUIZ.UI;
using UnityEngine;

public class UITemplateItemTest : UITemplateItemBase
{
    [SerializeField] private LText Index;

    //---------------------------------------------------
    protected override void OnUITemplateItemActivate()
    {
        base.OnUITemplateItemActivate();

        Debug.Log($"TemplateItem 요청 {this.gameObject.name} / {this.gameObject.GetInstanceID()}");
    }

    protected override void OnUITemplateItemReturn()
    {
        base.OnUITemplateItemReturn();

        Debug.Log($"TemplateItem 반환 {this.gameObject.name} / {this.gameObject.GetInstanceID()}");
    }

    //----------------------------------------------------
    public void DoTestItemSetting(int index)
    {
        Index.text = index.ToString();
    }
}
