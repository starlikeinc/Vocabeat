using System;
using System.Collections.Generic;
using UnityEngine;
using LUIZ;
public enum ESoundBasicType
{
    None = 0,
    
    BtnClickNormal,
    BtnClickCancel,
}

public class ManagerSound : ManagerSoundBase
{
    public new static ManagerSound Instance => ManagerSoundBase.Instance as ManagerSound;

    //---------------------------------------------------------------
    public enum ESoundChannelType
    {
        ChannelBGM      = 0,
        ChannelEffect   = 1,
        ChannelUI       = 2,
    }

    [System.Serializable]
    public class SoundBasicData
    {
        public ESoundChannelType SoundChannel;
        public ESoundBasicType SoundName;
        public ESoundPlayType PlayType;
        public AudioClip SoundClip;
        public float Volume = 1f;
        public bool Loop = false;
    }

    [Space(20)]
    [SerializeField] private AudioSource AudioUI;
    [SerializeField] private AudioSource AudioEffect;
    [SerializeField] private AudioSource AudioBGM;

    [Tooltip("동적으로 로드 하지 않고 런터임 동안 계속 메모리에 상주 시키는 리소스들이다. 홈 배경음악, 버튼 음이나 자주 사용하게 되는 효과음 등에만 사용할 것")]
    [SerializeField] private List<SoundBasicData> BasicSoundList;

    private readonly Dictionary<ESoundBasicType, SoundBasicData> m_dicSoundClipInfo = new();

    private bool m_isInitialized = false;

    //---------------------------------------------------------------
    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();
        ProtSoundChannelAdd((int)ESoundChannelType.ChannelUI, AudioUI);
        ProtSoundChannelAdd((int)ESoundChannelType.ChannelEffect, AudioEffect);
        ProtSoundChannelAdd((int)ESoundChannelType.ChannelBGM, AudioBGM);

        for (int i = 0; i < BasicSoundList.Count; i++)
        {
            m_dicSoundClipInfo[BasicSoundList[i].SoundName] = BasicSoundList[i];
        }

        m_isInitialized = true;
    }

    //---------------------------------------------------------------
    /// <summary>
    /// isInstant = false 일 경우 페이드 인아웃을 거친후에 재생됨
    /// </summary>
    public void DoSoundChangeBGM(string BGMClipName, bool isInstant, bool isLoop = true, Action delLoadFinish = null)
    {
        PrivSoundChangeBGM(BGMClipName, isInstant, isLoop, delLoadFinish);
    }

    public void DoSoundPauseBGM()
    {
        ProtSoundPause((int)ESoundChannelType.ChannelBGM);
    }

    public void DoSoundResumeBGM()
    {
        ProtSoundResume((int)ESoundChannelType.ChannelBGM);
    }

    public void DoSoundFadeInOutBGM(bool isFadeIn, float desVolume, Action<AudioSource> delFinish, bool isInstant)
    {
        ProtSoundFadeInOut((int)ESoundChannelType.ChannelBGM, isFadeIn, desVolume, delFinish, isInstant);
    }

    public void DoSoundPlayBasic(ESoundBasicType soundType)
    {
        if(PrivTryFindSoundBasicClip(soundType, out SoundBasicData basicData))
        {
            ProtSoundPlay((int)basicData.SoundChannel, basicData.SoundClip, basicData.PlayType, basicData.Volume, basicData.Loop);
        }
        else
        {
            //Error!
            Debug.Log($"[ManagerSound] {soundType} not found on basic sound");
        }
    }

    public override bool IsInitialized()
    {
        return m_isInitialized;
    }

    //---------------------------------------------------------------
    private void PrivSoundChangeBGM(string BGMClipName, bool isInstant, bool isLoop, Action delFinish)
    {
        if (string.IsNullOrWhiteSpace(BGMClipName))
        {
            delFinish?.Invoke();
            return;
        }

        if(AudioBGM.clip != null && AudioBGM.clip.name == BGMClipName)//이미 해당 bgm이 로드 되어있는 상태면 다시 재생
        {
            ProtSoundChange((int)ESoundChannelType.ChannelBGM, AudioBGM.clip, isInstant, isLoop);
            delFinish?.Invoke();
        }
        else
        {
            ManagerLoaderResource.Instance.DoLoadResourceAudioClip(BGMClipName, (AudioClip loadedClip) =>
            {
                if (loadedClip != null)
                {                    
                    //TODO : 메모리 내려가는거 확인해야함
                    if (AudioBGM.clip != null)//이전에 남아있는 BGM을 메모리에서 내린다.
                        ManagerLoaderResource.Instance.DoReleaseAsset(AudioBGM.clip);

                    ProtSoundChange((int)ESoundChannelType.ChannelBGM, loadedClip, isInstant, isLoop);
                }
                delFinish?.Invoke();
            });
        }
    }

    private bool PrivTryFindSoundBasicClip(ESoundBasicType basicSoundType, out SoundBasicData clipData)
    {
        bool isSuccess = false;

        if(m_dicSoundClipInfo.TryGetValue(basicSoundType, out clipData))
        {
            isSuccess = true;
        }
        
        return isSuccess;
    }
}
