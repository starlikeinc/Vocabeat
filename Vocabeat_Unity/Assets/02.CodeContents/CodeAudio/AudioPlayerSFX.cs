using UnityEngine;

public class AudioPlayerSFX : AudioPlayerBase
{
    protected override void OnBaseChannelEnable()
    {
        base.OnBaseChannelEnable();
        if (ManagerAudio.Instance != null)
            ManagerAudio.Instance.OnSFXVolumeChanged += RefreshVolume;
    }

    protected override void OnBaseChannelDisable()
    {
        base.OnBaseChannelDisable();        
        if (ManagerAudio.Instance != null)
            ManagerAudio.Instance.OnSFXVolumeChanged -= RefreshVolume;
    }

    protected override void OnAudioEvent(AudioCueSO cue)
    {
        if (cue == null || _audioSource == null)
            return;

        var clip = cue.GetRandomClip();
        if (clip == null)
            return;

        // 믹서 그룹 세팅 (선택)
        if (cue.OutputMixerGroup != null)
            _audioSource.outputAudioMixerGroup = cue.OutputMixerGroup;

        _audioSource.pitch = cue.Pitch;

        float volume = cue.Volume;

        if (ManagerAudio.Instance != null)
        {
            volume *= ManagerAudio.Instance.SfxVolume;
        }

        _audioSource.PlayOneShot(clip, volume);        
    }

    private void ApplyVolumeFromSettings()
    {
        if (ManagerAudio.Instance == null || _audioSource == null)
            return;

        _audioSource.volume = ManagerAudio.Instance.BgmVolume;
    }

    private void RefreshVolume()
    {
        // 옵션 슬라이더 변경 시 호출됨
        ApplyVolumeFromSettings();
    }

    protected override void OnAudioPause()
    {
        // PlayOneShot 하기 때문에 따로 Pause 하지 않음.
    }

    protected override void OnAudioResume()
    {
        // PlayOneShot 하기 때문에 따로 Resume 하지 않음.
    }

    protected override void OnAudioStop()
    {
        // PlayOneShot 하기 때문에 따로 Stop 하지 않음.
    }
}
