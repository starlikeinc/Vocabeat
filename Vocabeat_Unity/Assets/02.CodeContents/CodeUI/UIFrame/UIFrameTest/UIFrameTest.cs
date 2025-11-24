using System;
using System.Collections.Generic;
using LUIZ.InputSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LUIZ.UI;

public class UIFrameTest : UIFrameBase
{
    [SerializeField] private UIWidgetCounter WidgetCounter;

    [SerializeField] private RawImage RawImage;

    [SerializeField] private string RawName;
    [SerializeField] private string BGMName;
    [SerializeField] private string StageBackGroundName;

    private InputSO InputChannel => ManagerInput.Instance.GetInputChannel;
    
    private StageBackground m_stageBackground;

    //----------------------------------------------------
    protected override void OnUIFrameInitialize()
    {
        base.OnUIFrameInitialize();
        Debug.Log("UIFrameTest INIT");
    }

    protected override void OnUIFrameShow()
    {
        InputChannel.DoSubscribeAction<Vector2>("Move", HandleMoveInput, true);
        //InputChannel.DoSubscribeAction<bool>("CompositeTest", HandleCompositeInput);    
    }

    protected override void OnUIFrameHide()
    {
        InputChannel.DoUnsubscribeAction<Vector2>("Move", HandleMoveInput, true);
        //InputChannel.DoUnsubscribeAction<bool>("CompositeTest", HandleCompositeInput);
    }
    
    //-----------------------------------------------------
    private void HandleMoveInput(Vector2 vec2)
    {
        Debug.Log($"OnInputMove : {vec2}");
    }
    private void HandleCompositeInput(bool isPressed)
    {
        Debug.Log($"OnComposite : {isPressed}");
    }
    
    private void PrivLoadAssets()
    {
        ManagerLoaderResource.Instance.DoLoadResourceTexture(RawName, (Texture loadedTexture) =>
        {
            RawImage.texture = loadedTexture;
            Debug.Log($" LOADED : {loadedTexture.name}");
        });

        ManagerSound.Instance.DoSoundChangeBGM(BGMName, false, true, () => 
        {
            Debug.Log($" LOADED : {BGMName}");
        });

        ManagerLoaderPrefabPool.Instance.DoLoadComponent<StageBackground>(
            ManagerLoaderPrefabPool.EPoolType.StageProp,
            StageBackGroundName,
            (StageBackground loadedStage) =>
            {
                m_stageBackground = loadedStage;

                Debug.Log($" LOADED : {loadedStage.name}");

                loadedStage.transform.SetParent(null);
                loadedStage.gameObject.SetActive(true);

                loadedStage.DoResizeScaleByScreenSize();

                //ManagerLoaderPrefabPool.Instance.ReturnClone(m_stageBackground.gameObject, true);
            });
    }

    //-----------------------------------------------------
    public void OnBtnShowHidePopup()
    {
        //var settings = ManagerLocalData.Instance.Settings.GetSettingsData();
        if (UIChannel.IsUIFrameShow<UIFrameTestPopup>() == true)
        {
            UIChannel.UIHide<UIFrameTestPopup>();
        }
        else
        {
            UIChannel.UIShow<UIFrameTestPopup>();
        }
    }

    public void OnBtnLoadAssets()
    {
        PrivLoadAssets();
    }

    public void OnBtnReleaseAssets()
    {
        Texture texture = RawImage.texture;
        ManagerLoaderResource.Instance.DoReleaseAsset(texture);
        RawImage.texture = null;

        ManagerLoaderPrefabPool.Instance.DoReturnClone(m_stageBackground.gameObject, true);
        m_stageBackground = null;
    }


    public void OnBtnCounterShow()
    {
        WidgetCounter.DoUIWidgetShow();
    }
    public void OnBtnCounterHide()
    {
        WidgetCounter.DoUIWidgetHide();
    }

    public void OnBtnCounterShowGraphic()
    {
        WidgetCounter.DoUIWidgetShow(true);
    }
    public void OnBtnCounterHideGraphic()
    {
        WidgetCounter.DoUIWidgetHide(true);
    }
}
