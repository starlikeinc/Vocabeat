using LUIZ.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIWidgetOption : UIWidgetCanvasBase
{    
    [SerializeField] private Slider _sliderBgm;
    [SerializeField] private Slider _sliderSfx;

    [SerializeField] private TMP_Text _textBGMValue;
    [SerializeField] private TMP_Text _textSFXValue;

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

        _textBGMValue.text = $"{audioMgr.BgmVolume * 100:N0}";
        _textSFXValue.text = $"{audioMgr.SfxVolume * 100:N0}";

        _sliderBgm.onValueChanged.AddListener(v =>
        {
            audioMgr.SetBgmVolume(v);
            _textBGMValue.text = $"{v * 100:N0}";
        });
        _sliderSfx.onValueChanged.AddListener(v =>
        {
            audioMgr.SetSfxVolume(v);
            _textSFXValue.text = $"{v * 100:N0}";
        });
    }    

    public void OnClose()
    {
        _frameSongMenu.PlayFrameSfx(ESongMenuSfxKey.BtnClick);
        DoUIWidgetHide();
    }
}
