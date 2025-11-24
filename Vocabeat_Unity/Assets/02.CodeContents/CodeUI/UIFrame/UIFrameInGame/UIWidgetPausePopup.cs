using LUIZ.UI;
using UnityEngine;
using UnityEngine.Events;

public class UIWidgetPausePopup : UIWidgetCanvasBase
{
    [SerializeField] private UnityEvent OnPopupEnable;

    private UIFrameInGame _frameInGame;

    // ========================================
    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);
        _frameInGame = parentFrame as UIFrameInGame;
    }

    protected override void OnUnityEnable()
    {
        base.OnUnityEnable();
        OnPopupEnable?.Invoke();
    }

    // ========================================
    public void OnResume()
    {
        DoUIWidgetHide();
        ManagerRhythm.Instance.ResumeSong();
    }

    public void OnRetry()
    {
        DoUIWidgetHide();
        ManagerRhythm.Instance.RetrySong();
    }

    public void OnExit()
    {
        DoUIWidgetHide();
        _frameInGame.ReturnToSongMenu();
    }
}
