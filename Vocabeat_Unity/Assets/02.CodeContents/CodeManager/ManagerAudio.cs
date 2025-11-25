using System;
using LUIZ;
using UnityEngine;

public class ManagerAudio : SingletonBase<ManagerAudio>, IManagerInstance
{    
    private const string KEY_BGM = "Audio_Vol_BGM";
    private const string KEY_SFX = "Audio_Vol_SFX";

    public event Action OnBGMVolumeChanged;
    public event Action OnSFXVolumeChanged;

    [SerializeField] private AudioEventChannelSO _bgmChannel;
    [SerializeField] private AudioEventChannelSO _sfxChannel;    
    
    [Range(0f, 1f)][SerializeField] private float _bgm = 1f;
    [Range(0f, 1f)][SerializeField] private float _sfx = 1f;
    
    public float BgmVolume => _bgm;
    public float SfxVolume => _sfx;

    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();        
        _bgm = PlayerPrefs.GetFloat(KEY_BGM, 1f);
        _sfx = PlayerPrefs.GetFloat(KEY_SFX, 1f);
    }

    public bool IsInitialized()
    {
        return Instance != null;
    }

    public void SetBgmVolume(float value)
    {
        _bgm = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(KEY_BGM, _bgm);
        OnBGMVolumeChanged?.Invoke();
    }

    public void SetSfxVolume(float value)
    {
        _sfx = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(KEY_SFX, _sfx);
        OnSFXVolumeChanged?.Invoke();
    }
}
