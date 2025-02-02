using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ControlsMode
{
    Direct,
    Tracking,
    MouseStick
}
public class ControlsManager : MonoBehaviour
{
    public static ControlsMode preferredPilot { get; private set; }
    public static ControlsMode preferredGunner { get; private set; }
    public static PitchCorrectionMode pitchCorrectionMode { get; private set; }

    public static ControlsMode CurrentMode()
    {
#if MOBILE_INPUT
        return ControlsMode.Direct;
#endif
        if (UIManager.gameUI != GameUI.Game) return ControlsMode.Direct;


        if (Player.role == SeatRole.Pilot)
        {
            bool cantUseTracking = SofCamera.subCam != null && SofCamera.subCam.logic.BaseDirMode != CamDir.SeatAligned;
            if (preferredPilot == ControlsMode.Tracking && cantUseTracking) return ControlsMode.Direct;

            return preferredPilot;
        }
        if (Player.role == SeatRole.Gunner)
        {
            if (!Player.gunnerSeat.gunMount) return preferredGunner;

            if(Player.gunnerSeat.gunMount.ForceJoystickControls)
                if (preferredGunner == ControlsMode.Tracking) return ControlsMode.MouseStick;
            return preferredGunner;
        }
        return ControlsMode.Direct;
    }
    private void Awake()
    {
        pitchCorrectionMode = (PitchCorrectionMode)PlayerPrefs.GetInt("PitchCorrectionMode", 3);
        preferredPilot = (ControlsMode)PlayerPrefs.GetInt("ControlsMode", 2);
        preferredGunner = (ControlsMode)PlayerPrefs.GetInt("ControlsMode", 2);
    }
}
