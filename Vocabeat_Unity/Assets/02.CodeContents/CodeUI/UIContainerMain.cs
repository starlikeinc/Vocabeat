using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LUIZ.UI;

public class UIContainerMain : UIContainerBase
{
    protected override void OnContainerInitialize()
    {
        base.OnContainerInitialize();

        UIChannel.UIShow<UIFrameMain>();
    }
}
