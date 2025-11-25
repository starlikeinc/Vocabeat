using LUIZ.UI;
using System.Collections.Generic;
using UnityEngine;

public class SceneStepIngameTest : SceneStepBase
{
    private enum EEntryType { SongMenu, InGame }

    [SerializeField] private Camera DevelopCamera;
    [SerializeField] private UIContainerBase UIContainerDevelop;

    [Header("Test")]
    [SerializeField] private SongDataSO TestSongData;
    [SerializeField] private EDifficulty Diff;

    [Header("Options - 진입점 설정. 인게임은 테스트 곡으로 바로 플레이")]
    [SerializeField] private EEntryType _entryType;

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
        if (_entryType == EEntryType.SongMenu)
            UIChannel.UIShow<UIFrameSongMenu>().DoFrameSongMenuSetting(true);
        else if (_entryType == EEntryType.InGame)
            UIChannel.UIShow<UIFrameInGame>().BindSongData(TestSongData, Diff);
    }
}
