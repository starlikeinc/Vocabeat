using System;
using UnityEngine;

public abstract class AudioEventChannelSO : ScriptableObject
{
    public event Action<AudioCueSO> OnEventRaised;

    public void Raise(AudioCueSO cue)
    {
        OnEventRaised?.Invoke(cue);
    }
}