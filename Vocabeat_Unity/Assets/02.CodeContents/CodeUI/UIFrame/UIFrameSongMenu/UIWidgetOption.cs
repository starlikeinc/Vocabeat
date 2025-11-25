using LUIZ.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIWidgetOption : UIWidgetCanvasBase
{    
    [SerializeField] private Slider _sliderBgm;
    [SerializeField] private Slider _sliderSfx;

    private UIFrameSongMenu _frameSongMenu;

    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);
        _frameSongMenu = parentFrame as UIFrameSongMenu;
    }

    protected override void OnUnityStart()
    {
        base.OnUnityStart();
        var audioMgr = ManagerAudio.Instance;
        if (audioMgr == null)
        {
            Debug.LogError("ManagerAudio 인스턴스 없음");
            return;
        }

        // 초기값 세팅 (저장된 값 불러옴)        
        _sliderBgm.SetValueWithoutNotify(audioMgr.BgmVolume);
        _sliderSfx.SetValueWithoutNotify(audioMgr.SfxVolume);

        _sliderBgm.onValueChanged.AddListener(v => audioMgr.SetBgmVolume(v));
        _sliderSfx.onValueChanged.AddListener(v => audioMgr.SetSfxVolume(v));
    }    

    public void OnClose()
    {
        _frameSongMenu.PlayFrameSfx(ESongMenuSfxKey.BtnClick);
        DoUIWidgetHide();
    }
}
