using UnityEngine;

public class AudioPlayerBGM : AudioPlayerBase
{
    protected override void OnBaseChannelEnable()
    {
        base.OnBaseChannelEnable();
        if (_eventChannel is BGMEventChannelSO bgmChannel)
            bgmChannel.OnPlayScheduled += OnPlayScheduled;
    }

    protected override void OnBaseChannelDisable()
    {
        base.OnBaseChannelDisable();
        if (_eventChannel is BGMEventChannelSO bgmChannel)
            bgmChannel.OnPlayScheduled -= OnPlayScheduled;
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
        _audioSource.clip = clip;
        _audioSource.loop = cue.Loop;
        _audioSource.volume = cue.Volume;
        
        // BGM쪽은 클립 설정만 하고 Play는 따로.
    }

    protected override void OnAudioStop()
    {
        _audioSource.Stop();
    }   

    protected override void OnAudioPause()
    {
        if (_audioSource != null && _audioSource.isPlaying)
            _audioSource.Pause();        
    }

    protected override void OnAudioResume()
    {
        if (_audioSource != null)
            _audioSource.UnPause();
    }

    private void OnPlayScheduled(double time, bool isLoop)
    {
        _audioSource.loop = isLoop;
        if(time == 0)
            _audioSource.Play();
        else
            _audioSource.PlayScheduled(time);
    }
}
