using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BGM_EventChannel", menuName = "AudioChannel/BGM Event Channel")]
public class BGMEventChannelSO : AudioEventChannelSO
{
    public event Action<double, bool> OnPlayScheduled;    

    public void PlayScheduled(double time, bool isLoop = false)
    {
        OnPlayScheduled?.Invoke(time, isLoop);
    }
}