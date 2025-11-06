using System;
using System.Collections.Generic;
using LUIZ;

public class LocalDataSettings : ILocalDataLoadable
{
    public void OnDataLoadFinish()
    {
        
    }
    
    //볼륨-----------------------------------------------------
    private const float c_VolumeDefault = 0.5f;
    private const float c_VolumeMin = 0f;
    private const float c_VolumeMax = 1f;

    private float m_VolumeBGM = c_VolumeDefault;
    private float m_VolumeSFX = c_VolumeDefault;

    public float VolumeBGM
    {
        get => m_VolumeBGM;
        set => m_VolumeBGM = Math.Clamp(value, c_VolumeMin, c_VolumeMax);
    }

    public float VolumeSFX
    {
        get => m_VolumeSFX;
        set => m_VolumeSFX = Math.Clamp(value, c_VolumeMin, c_VolumeMax);
    }

    //프레임------------------------------------------------------
    public enum EFrameRateMaxType
    {
        FPS30 = 0,
        FPS60 = 1,
    }

    private const int c_FrameRateMaxDefault = (int)EFrameRateMaxType.FPS60;

    private int m_FrameRateMaxType = c_FrameRateMaxDefault;
    public int FrameRateMaxType
    {
        get => m_FrameRateMaxType;
        set
        {
            if (Enum.IsDefined(typeof(EFrameRateMaxType), value) == true)
            {
                m_FrameRateMaxType = value;
            }
            else
            {
                m_FrameRateMaxType = c_FrameRateMaxDefault;
            }
        }
    }
}
