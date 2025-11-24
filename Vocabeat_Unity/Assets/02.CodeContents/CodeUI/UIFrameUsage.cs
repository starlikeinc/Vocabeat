using LUIZ.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIFrameUsage<TSfxKey> : UIFrameBase where TSfxKey : struct, Enum
{
    [Header("BGM")]
    [SerializeField] private BGMEventChannelSO _bgmChannel;
    [SerializeField] private AudioCueSO _bgmCue;

    [Header("SFX")]
    [SerializeField] private SFXEventChannelSO _sfxChannel;

    [Serializable]
    public struct SfxEntry
    {
        public TSfxKey Key;
        public AudioCueSO Cue;
    }

    [SerializeField] private List<SfxEntry> _sfxEntries = new();

    private Dictionary<TSfxKey, AudioCueSO> _sfxDict;

    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();
        BuildSfxDict();
    }

    private void BuildSfxDict()
    {
        _sfxDict = new Dictionary<TSfxKey, AudioCueSO>();

        foreach (var entry in _sfxEntries)
        {
            if (entry.Cue == null)
                continue;

            _sfxDict[entry.Key] = entry.Cue; // 중복 키는 마지막 것으로 덮어쓰기
        }
    }

    public void PlayFrameSfx(TSfxKey key)
    {
        if (_sfxChannel == null || _sfxDict == null)
            return;

        if (_sfxDict.TryGetValue(key, out var cue) && cue != null)
        {
            _sfxChannel.Raise(cue);
        }
    }

    public void ChangeFrameBGM(AudioCueSO cue)
    {
        _bgmChannel.Raise(cue);
        _bgmChannel.PlayScheduled(0.2f, true);
    }

    protected void PlayFrameBgm(bool isLoop = true)
    {
        if (_bgmChannel == null || _bgmCue == null)
            return;

        _bgmChannel.Raise(_bgmCue);
        _bgmChannel.PlayScheduled(0, isLoop);
    }
}
