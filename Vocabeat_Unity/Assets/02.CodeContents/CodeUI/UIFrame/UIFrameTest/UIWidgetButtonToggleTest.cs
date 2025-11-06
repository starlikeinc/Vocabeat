using UnityEngine;
using LUIZ.UI;

public class UIWidgetButtonToggleTest : UIWidgetBase
{
    public void OnBtnClick()
    {
        Debug.Log("Clicked!");
    }

    public void OnBtnLongPress()
    {
        Debug.Log("LongPress!");
    }

    public void OnToggleOn()
    {
        Debug.Log("Toggle ON!");
    }

    public void OnToggleOff()
    {
        Debug.Log("Toggle OFF!");
    }

    //-----------------------------------------
    public void OnRadio_1_On()
    {
        Debug.Log("1!");
    }
    public void OnRadio_2_On()
    {
        Debug.Log("2!");
    }
    public void OnRadio_3_On()
    {
        Debug.Log("3!");
    }
    public void OnRadio_4_On()
    {
        Debug.Log("4!");
    }

    public void OnRadio_1_Off()
    {
        Debug.Log("1! OFF");
    }
    public void OnRadio_2_Off()
    {
        Debug.Log("2! OFF");
    }
    public void OnRadio_3_Off()
    {
        Debug.Log("3! OFF");
    }
    public void OnRadio_4_Off()
    {
        Debug.Log("4! OFF");
    }
}
