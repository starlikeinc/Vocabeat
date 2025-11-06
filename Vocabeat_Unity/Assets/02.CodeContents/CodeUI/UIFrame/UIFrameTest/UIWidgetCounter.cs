using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LUIZ.UI;

public class UIWidgetCounter : UIWidgetBase
{
    [SerializeField] private Text Counter;

    private float m_currentCounter = 0f;
    private bool m_isPlay = true;

    //------------------------------------------------------
    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);
        Debug.Log("UIWidgetCounter : INIT");
    }

    protected override void OnUIWidgetParentFrameShow()
    {
        base.OnUIWidgetParentFrameShow();
        Debug.Log("UIWidgetCounter : ParentShow");
    }

    protected override void OnUIWidgetParentFrameHide()
    {
        base.OnUIWidgetParentFrameHide();
        Debug.Log("UIWidgetCounter : ParentHide");
    }

    protected override void OnUIWidgetShow()
    {
        base.OnUIWidgetShow();
        Debug.Log("UIWidgetCounter : Show");
    }

    protected override void OnUIWidgetHide()
    {
        base.OnUIWidgetHide();
        Debug.Log("UIWidgetCounter : Hide");
    }

    //------------------------------------------------------
    private void Update()
    {
        UpdateCounter();
    }

    private void UpdateCounter()
    {
        if (m_isPlay == false)
            return;

        m_currentCounter += Time.deltaTime;

        Counter.text = m_currentCounter.ToString();
    }

    //------------------------------------------------------
    public void OnBtnCounterPause()
    {
        m_isPlay = false;
    }
    public void OnBtnCounterPlay()
    {
        m_isPlay = true;
    }
}
