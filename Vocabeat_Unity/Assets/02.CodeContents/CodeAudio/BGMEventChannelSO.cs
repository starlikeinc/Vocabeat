using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BGM_EventChannel", menuName = "AudioChannel/BGM Event Channel")]
public class BGMEventChannelSO : AudioEventChannelSO
{
    public event Action<double> OnPlayScheduled;

    public void PlayScheduled(double time)
    {
        OnPlayScheduled?.Invoke(time);
    }
}