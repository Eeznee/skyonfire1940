using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameCustomRipSpeed
{
    public float customRipSpeed;
    public HydraulicSystem hydraulics;

    public FrameCustomRipSpeed(HydraulicsDragAndRip _hydraulicsRip)
    {
        hydraulics = _hydraulicsRip.hydraulics;
        customRipSpeed = _hydraulicsRip.MaxSpeeedMps;
    }

    public FrameCustomRipSpeed(float _customRipSpeed, HydraulicSystem _hydraulics)
    {
        customRipSpeed = _customRipSpeed;
        hydraulics = _hydraulics;
    }

    public FrameCustomRipSpeed(float _customRipSpeed)
    {
        customRipSpeed = _customRipSpeed;
        hydraulics = null;
    }

    public float MaxSpeed(SofFrame frame)
    {
        if (hydraulics)
        {
            return Mathf.Lerp(frame.MaxSpd(), customRipSpeed, hydraulics.state);
        }
        else
        {
            return customRipSpeed;
        }
    }
}
