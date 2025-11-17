using System;
using UnityEngine;

public abstract class AudioEventChannelSO : ScriptableObject
{
    public event Action<AudioCueSO> OnEventRaised;
    public event Action OnEventStop;

    public void Raise(AudioCueSO cue)
    {
        OnEventRaised?.Invoke(cue);
    }

    public void StopAudio()
    {
        OnEventStop?.Invoke();
    }
}