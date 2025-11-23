using LUIZ.UI;
using System.Collections.Generic;
using UnityEngine;

public class SceneStepIngameTest : SceneStepBase
{
    [SerializeField] private Camera DevelopCamera;
    [SerializeField] private UIContainerBase UIContainerDevelop;

    [Header("Test")]
    [SerializeField] private SongDataSO TestSongData;
    [SerializeField] private EDifficulty Diff;

    //-------------------------------------------------------------
    protected override void OnUnityStart()
    {
        ProtLoadBasicDependencies(() =>
        {
            ManagerLoaderScene.Instance.DoMoveToSceneHome(() =>
            {
                DevelopCamera.gameObject.SetActive(false);
                UIContainerDevelop.DoRegisterContainer();

                PrivSceneStepFinish();
            });            
        });
    }

    //-------------------------------------------------------------
    private void PrivSceneStepFinish()
    {
        UIChannel.UIShow<UIFrameInGame>().BindSongData(TestSongData, Diff);
        //UIChannel.UIShow<UIFrameSongMenu>().DoFrameSongMenuSetting(true);
    }
}
