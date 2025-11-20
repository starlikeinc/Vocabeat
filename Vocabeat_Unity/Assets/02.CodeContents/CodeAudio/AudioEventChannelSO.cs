using System;
using UnityEngine;

public abstract class AudioEventChannelSO : ScriptableObject
{
    public event Action<AudioCueSO> OnEventRaised;
    public event Action OnEventStop;

    public event Action OnEventPause;
    public event Action OnEventResume;

    public void Raise(AudioCueSO cue)
    {
        OnEventRaised?.Invoke(cue);
    }

    public void StopAudio()
    {
        OnEventStop?.Invoke();
    }

    public void PauseAudio()
    {
        OnEventPause?.Invoke();
    }

    public void ResumeAudio()
    {
        OnEventResume?.Invoke();
    }
}