using LUIZ.UI;
using UnityEngine;
using UnityEngine.Events;

public class UIWidgetPausePopup : UIWidgetBase
{
    [SerializeField] private UnityEvent OnPopupEnable;

    protected override void OnUnityEnable()
    {
        base.OnUnityEnable();
        OnPopupEnable?.Invoke();
    }

    public void OnResume()
    {

    }

    public void OnRetry()
    {

    }

    public void OnExit()
    {

    }
}
