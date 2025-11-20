using LUIZ.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIWidgetSongProgress : UIWidgetBase
{
    [SerializeField] private Image _imgProgressBar;

    private float _totalTime;

    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);
        ManagerRhythm.Instance.RTimeline.OnSongProgress -= OnSongProgress;
        ManagerRhythm.Instance.RTimeline.OnSongProgress += OnSongProgress;
    }

    public void WidgetSongProgressSetting(AudioClip songClip)
    {
        _totalTime = songClip.length;
        _imgProgressBar.fillAmount = 0f;
    }

    private void OnSongProgress(float currentTime)
    {
        float progress01 = currentTime / _totalTime;

        _imgProgressBar.fillAmount = progress01;
    }
}
