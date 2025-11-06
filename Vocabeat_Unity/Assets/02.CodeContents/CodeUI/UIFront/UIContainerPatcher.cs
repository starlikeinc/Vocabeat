using LUIZ.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIContainerPatcher : UIContainerBase
{
    protected override void OnContainerInitialize()
    {
        base.OnContainerInitialize();

        UIChannel.UIShow<UIFramePatcher>();
    }
}