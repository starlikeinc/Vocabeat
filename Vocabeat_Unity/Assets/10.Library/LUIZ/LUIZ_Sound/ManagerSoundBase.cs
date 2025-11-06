using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LUIZ
{
    public abstract class ManagerSoundBase : SingletonBase<ManagerSoundBase>, IManagerInstance
    {
        public enum ESoundPlayType
        {
            Reset,          // 재생을 중지하고 다시 재생한다.
            PlayOneShot,    // 중복 재생을 한다.
            Exclusive,      // 재생중이면 재생하지 않는다
        }

        private class SoundChannelData
        {
            public int SoundChannelID;

            public AudioSource AudioOutput;
            public float DefaultVolume;

            public Coroutine VolChangeHandle;
        }

        [Space(20)]
        //TODO : 체널 별로 설정할 수 있도록?
        [SerializeField] private AnimationCurve FadeInCurve = AnimationCurve.Linear(0, 0, 1f, 1f);
        [SerializeField] private AnimationCurve FadeOutCurve = AnimationCurve.Linear(0, 0, 1f, 1f);
        [SerializeField] private float FadeInTime = 2f;
        [SerializeField] private float FadeOutTime = 2f;

        private Dictionary<int, SoundChannelData> m_dicSoundChannel = new Dictionary<int, SoundChannelData>();

        //---------------------------------------------------------------------------------------
        public abstract bool IsInitialized();

        //---------------------------------------------------------------------------------------
        /// <summary>
        /// 해당 함수를 통해 사운드 체널을 등록한 후 사용해야함
        /// </summary>
        protected void ProtSoundChannelAdd(int channelType, AudioSource audioSource)
        {
            if (m_dicSoundChannel.ContainsKey(channelType) == false)
            {
                SoundChannelData channelData = new SoundChannelData();
                channelData.SoundChannelID = channelType;
                channelData.AudioOutput = audioSource;
                channelData.DefaultVolume = audioSource.volume;
                channelData.VolChangeHandle = null;

                m_dicSoundChannel.Add(channelType, channelData);
            }
        }

        //---------------------------------------------------------------------------------------
        //TODO :체널별 defaultVolume 조절 기능

        /// <summary>
        /// volume = 0 일 경우 해당 체널의 기본 볼륨을 이용한다.
        /// </summary>
        protected void ProtSoundPlay(int channelType, AudioClip playClip, ESoundPlayType playType, float volume, bool isLoop)  //ÀÏ°ý ¸¶½ºÅÍ¸µ ÇØÁÖ¸é ÁÁ°ÚÁö¸¸ ³ìÀ½µÈ º¼·ýÀÌ ´Ù¸¥ °ÍµéÀÌ ºó¹øÇÔ..
        {
            if (PrivTryFindSoundChannel(channelType, out SoundChannelData channelData))
            {
                if (volume == 0)
                {
                    volume = channelData.DefaultVolume;
                }

                PrivSoundPlayByType(channelData, playClip, playType, volume, isLoop);
            }
        }

        protected void ProtSoundChange(int channelType, AudioClip playClip, bool isInstant, bool isLoop)
        {
            if (PrivTryFindSoundChannel(channelType, out SoundChannelData channelData))
            {
                PrivSoundChange(channelData, playClip, isInstant, channelData.DefaultVolume, isLoop);
            }
        }

        protected void ProtSoundFadeInOut(int channelType, bool isFadeIn, float volumeEnd, Action<AudioSource> delFinish, bool isInstant)
        {
            if (PrivTryFindSoundChannel(channelType, out SoundChannelData channelData))
            {
                AnimationCurve volumeCurve = isFadeIn ? FadeInCurve : FadeOutCurve;
                float changeDuration = isFadeIn ? FadeInTime : FadeOutTime;

                PrivMgrSoundChangeVolume(channelData, volumeCurve, changeDuration, channelData.AudioOutput.volume, volumeEnd, delFinish, isInstant);
            }
        }

        protected void ProtSoundResume(int channelType)
        {
            if (PrivTryFindSoundChannel(channelType, out SoundChannelData channelData))
            {
                PrivSoundResume(channelData.AudioOutput);
            }
        }

        protected void ProtSoundPause(int channelType)
        {
            if (PrivTryFindSoundChannel(channelType, out SoundChannelData channelData))
            {
                PrivSoundPause(channelData.AudioOutput);
            }
        }

        //---------------------------------------------------------------------------------------
        private void PrivSoundChange(SoundChannelData channelData, AudioClip playClip, bool isInstant, float volume, bool isLoop)
        {
            AudioSource pAudioSource = channelData.AudioOutput;

            if (isInstant == true)
            {
                PrivSoundPlayClip(pAudioSource, playClip, volume, isLoop);
                return;
            }

            if (pAudioSource.isPlaying)
            {
                PrivMgrSoundChangeVolume(channelData, FadeOutCurve, FadeOutTime, pAudioSource.volume, 0, (AudioSource pAudioSource) =>
                {
                    PrivSoundPlayClip(pAudioSource, playClip, pAudioSource.volume, isLoop);
                    PrivMgrSoundChangeVolume(channelData, FadeInCurve, FadeInTime, pAudioSource.volume, volume, null, false);
                }, false);
            }
            else
            {
                PrivSoundPlayClip(pAudioSource, playClip, pAudioSource.volume, isLoop);
                PrivMgrSoundChangeVolume(channelData, FadeInCurve, FadeInTime, pAudioSource.volume, volume, null, false);
            }
        }

        private void PrivSoundPause(AudioSource audioSource)
        {
            audioSource.Stop();
        }

        private void PrivSoundResume(AudioSource audioSource)
        {
            audioSource.Play();
        }

        private void PrivSoundPlayByType(SoundChannelData channelData, AudioClip playClip, ESoundPlayType playType, float volume, bool isLoop)
        {
            if (playType == ESoundPlayType.Exclusive)
            {
                if (channelData.AudioOutput.clip == playClip)
                {
                    if (channelData.AudioOutput.isPlaying == false)
                    {
                        PrivSoundPlayClip(channelData.AudioOutput, playClip, volume, isLoop);
                    }
                }
                else
                {
                    PrivSoundPlayClip(channelData.AudioOutput, playClip, volume, isLoop);
                }
            }
            else if (playType == ESoundPlayType.PlayOneShot)
            {
                channelData.AudioOutput.PlayOneShot(playClip);
            }
            else if (playType == ESoundPlayType.Reset)
            {
                PrivSoundPlayClip(channelData.AudioOutput, playClip, volume, isLoop);
            }
        }

        private void PrivSoundPlayClip(AudioSource audioSource, AudioClip playClip, float volume, bool isLoop)
        {
            audioSource.Stop();
            audioSource.clip = playClip;
            audioSource.volume = volume;
            audioSource.loop = isLoop;
            audioSource.Play();
        }

        private void PrivMgrSoundChangeVolume(SoundChannelData channelData, AnimationCurve animCurve, float duration, float volStart, float volEnd, Action<AudioSource> delFinish, bool isInstant)
        {
            if (channelData.VolChangeHandle != null)
                StopCoroutine(channelData.VolChangeHandle);

            if (isInstant == true)
            {
                channelData.AudioOutput.volume = volEnd;
                channelData.VolChangeHandle = null;
                delFinish?.Invoke(channelData.AudioOutput);
            }
            else
            {
                channelData.VolChangeHandle = StartCoroutine(PrivCOChangeVolume(channelData, animCurve, duration, volStart, volEnd, delFinish));
            }
        }

        private bool PrivTryFindSoundChannel(int channelType, out SoundChannelData channelData)
        {
            bool isSuccess = false;

            if (m_dicSoundChannel.TryGetValue(channelType, out channelData))
            {
                isSuccess = true;
            }
            else
            {
                Debug.LogError($"[ManagerSoundBase] channelType {channelType} is not a valid channel. Please add channel first");
            }

            return isSuccess;
        }

        //---------------------------------------------------------------------------------------
        private IEnumerator PrivCOChangeVolume(SoundChannelData channelData, AnimationCurve animCurve, float duration, float volStart, float volEnd, Action<AudioSource> delFinish)
        {
            AudioSource audioSource = channelData.AudioOutput;
            float currentDuration = 0;
            float percent = 0;
            float curveValue = 0;

            while (currentDuration < duration)
            {
                percent = currentDuration / duration;
                curveValue = animCurve.Evaluate(percent);

                audioSource.volume = Mathf.Lerp(volStart, volEnd, curveValue);

                currentDuration += Time.deltaTime;

                yield return null;
            }

            audioSource.volume = volEnd;
            channelData.VolChangeHandle = null;
            delFinish?.Invoke(audioSource);
        }
    }
}
