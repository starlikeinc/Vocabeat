using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public abstract class AudioPlayerBase : MonoBehaviour
{
    [Header("Event Channel")]
    [SerializeField] protected AudioEventChannelSO _eventChannel;

    [Header("Audio Source")]
    [SerializeField] protected AudioSource _audioSource;

    private void Reset()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        if (_eventChannel != null)
        {
            _eventChannel.OnEventRaised += OnAudioEvent;
            _eventChannel.OnEventStop += OnAudioStop;

            _eventChannel.OnEventPause += OnAudioPause;
            _eventChannel.OnEventResume += OnAudioResume;

            OnBaseChannelEnable();
        }            
    }

    private void OnDisable()
    {
        if (_eventChannel != null)
        {
            _eventChannel.OnEventRaised -= OnAudioEvent;
            _eventChannel.OnEventStop -= OnAudioStop;

            _eventChannel.OnEventPause -= OnAudioPause;
            _eventChannel.OnEventResume -= OnAudioResume;

            OnBaseChannelDisable();
        }            
    }

    protected virtual void OnBaseChannelEnable() { }
    protected virtual void OnBaseChannelDisable() { }

    protected abstract void OnAudioEvent(AudioCueSO cue);    
    protected abstract void OnAudioStop();
    protected abstract void OnAudioPause();
    protected abstract void OnAudioResume();
}
