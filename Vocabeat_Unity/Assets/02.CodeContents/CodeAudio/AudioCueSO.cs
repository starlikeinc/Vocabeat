using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "SFX_", menuName = "AudioChannel/Audio Cue")]
public class AudioCueSO : ScriptableObject
{
    [SerializeField] private List<AudioClip> _listClips = new();

    [Header("재생설정")]
    [Range(0f, 1f)]
    [SerializeField] private float _volume = 1f;
    [Range(-3f, 3f)]
    [SerializeField] private float _pitch = 1f;

    [SerializeField] private bool _randomizePitch = false;
    [SerializeField] private Vector2 _pitchRange = new Vector2(0.95f, 1.05f);

    [SerializeField] private AudioMixerGroup _outputMixerGroup; // 선택?

    public List<AudioClip> ListClips => _listClips;

    public AudioClip GetRandomClip()
    {
        if (_listClips == null || _listClips.Count == 0)
            return null;

        int index = Random.Range(0, _listClips.Count);
        return _listClips[index];
    }

    public float Volume => _volume;
    public float Pitch
    {
        get
        {
            if (!_randomizePitch)
                return _pitch;
            return Random.Range(_pitchRange.x, _pitchRange.y);
        }
    }

    public AudioMixerGroup OutputMixerGroup => _outputMixerGroup;
}
