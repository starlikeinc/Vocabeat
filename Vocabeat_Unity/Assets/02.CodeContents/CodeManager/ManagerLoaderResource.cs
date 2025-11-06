using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using LUIZ.AddressableSupport;

// Texture, AudioClip과 같은 일반적인 Object 리소스 객체를 로드하기 위한 매니저
// GameObject 나 프리팹을 로드하고 싶으면 PrefabPool 을 이용할 것
public class ManagerLoaderResource : ManagerAddressableResourceBase
{
    public static new ManagerLoaderResource Instance { get { return ManagerAddressableResourceBase.Instance as ManagerLoaderResource; } }

    //------------------------------------------------------------

    public void DoLoadResourceAudioClip(string audioClipName, Action<AudioClip> delFinish)
    {
        ProtLoadResource<AudioClip>(audioClipName, delFinish);
    }

    public void DoLoadResourceVideoClip(string videoClipName, Action<VideoClip> delFinish)
    {
        ProtLoadResource<VideoClip>(videoClipName, delFinish);
    }

    public void DoLoadResourceTexture(string textureName, Action<Texture> delFinish)
    {
        ProtLoadResource<Texture>(textureName, delFinish);
    }
}
