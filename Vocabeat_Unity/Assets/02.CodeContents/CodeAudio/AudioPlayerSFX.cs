using UnityEngine;

public class AudioPlayerSFX : AudioPlayerBase
{
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
        _audioSource.PlayOneShot(clip, cue.Volume);
    }
}
