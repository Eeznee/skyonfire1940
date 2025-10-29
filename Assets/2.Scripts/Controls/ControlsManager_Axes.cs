using UnityEngine;
using UnityEngine.InputSystem;

public partial class ControlsManager : MonoBehaviour
{
    static float maintainBank = 0f;
    static float forceCameraPointDirectionTimer = 0f;

    const float throttleIncrementFactor = 0.003f;
    public static bool forceCameraPointDirection = false;

    public static void PilotUpdateAxes(CrewMember crew)
    {
        SofAircraft aircraft = crew.aircraft;

        if (pilot.FirePrimaries.ReadValue<float>() > 0.2f || pilot.FireGuns.ReadValue<float>() > 0.2f) aircraft.armament.FireGuns(true,false,Time.deltaTime * 2f);
        if (pilot.FireSecondaries.ReadValue<float>() > 0.2f || pilot.FireGuns.ReadValue<float>() > 0.2f) aircraft.armament.FireGuns(false, true, Time.deltaTime * 2f);
        aircraft.controls.brake = pilot.Brake.ReadValue<float>();

        float relativeFlaps = pilot.FlapsRelative.ReadValue<float>();
        aircraft.hydraulics.SetFlaps(Mathf.RoundToInt(relativeFlaps));

        float scrollValue = anySeat.ScrollWheel.ReadValue<Vector2>().y * 6.6667f;
        float relativeThrottle = pilot.ThrottleRelative.ReadValue<float>() + scrollValue;
        if (relativeThrottle != 0f)
        {
            float currentThrottle = aircraft.engines.Throttle;
            float throttleIncrement = relativeThrottle * throttleIncrementFactor;

            if (currentThrottle >= 1f)
            {
                if (relativeThrottle < -0.05f) aircraft.engines.SetThrottleAllEngines(1f, false);
                if (relativeThrottle < -0.5f) aircraft.engines.SetThrottleAllEngines(currentThrottle + throttleIncrement, false);
                if (relativeThrottle > 0.8f) aircraft.engines.SetThrottleAllEngines(1.1f, true);
            }
            else
                aircraft.engines.SetThrottleAllEngines(currentThrottle + throttleIncrement, false);
        }
        

    }
    public static void PilotFixedUpdateAxes(CrewMember crew)
    {
        SofAircraft aircraft = crew.aircraft;

        float pitch = -pilot.Pitch.ReadValue<float>();
        if (SofSettingsSO.CurrentSettings.invertPitch) pitch = -pitch;
        float roll = pilot.Roll.ReadValue<float>();
        float yaw = -pilot.Rudder.ReadValue<float>();

        if (CurrentMode() == ControlsMode.Tracking) //Tracking input, mouse
        {
            bool pitching = pilot.Pitch.phase == InputActionPhase.Started;
            bool rolling = pilot.Roll.phase == InputActionPhase.Started;
            bool yawing = pilot.Rudder.phase == InputActionPhase.Started;

            AircraftAxes forcedAxes = new(float.NaN, float.NaN, float.NaN);

            if (pitching)
            {
                forceCameraPointDirection = true;
                forceCameraPointDirectionTimer = 1f;
                forcedAxes.pitch = pitch;
            }
            else
            {
                forceCameraPointDirectionTimer -= Time.deltaTime;
                if (forceCameraPointDirectionTimer < 0f) forceCameraPointDirection = false;
            }
            if (rolling || pitching)
            {
                forcedAxes.roll = Mathf.MoveTowards(aircraft.controls.current.roll, roll, Time.fixedDeltaTime * 3f);
                maintainBank = 1f;
            }
            else
            {
                maintainBank = Mathf.MoveTowards(maintainBank, 0f, 0.5f * Time.fixedDeltaTime);
            }
            if (yawing || pitching) forcedAxes.yaw = yaw;

            aircraft.controls.PreciseTracking(SofCamera.directionInput, forcedAxes, SofCamera.lookAround ? 1f : maintainBank);
            //aircraft.controls.SimpleTracking(SofCamera.directionInput, 0f, 1f, true);

        }
        else //Direct input, joystick, phone
        {
            AircraftAxes axes = new AircraftAxes(pitch, roll, yaw);
            aircraft.controls.SetTargetInput(axes, true, (PitchCorrectionMode)SofSettingsSO.CurrentSettings.pitchCorrectionMode);
        }
    }
    public static ControlsMode CurrentMode()
    {
        if (UIManager.gameUI != GameUI.Game) return ControlsMode.Direct;



        if (Extensions.IsMobile)
        {
            if (Player.role == SeatRole.Gunner && SofSettingsSO.CurrentSettings.mobileControlsMode != 2 && !(Player.gunnerSeat.gunMount && Player.gunnerSeat.gunMount.ForceJoystickControls))
            {
                return ControlsMode.Tracking;
            }

            return ControlsMode.Direct;
        }

        ControlsMode preferredControls = (ControlsMode)SofSettingsSO.CurrentSettings.pcControlsMode;

        if (Player.role == SeatRole.Pilot)
        {
            bool cantUseTracking = SofCamera.subCam != null && SofCamera.subCam.logic.BaseDirMode != CamDir.SeatAligned;
            if (preferredControls == ControlsMode.Tracking && cantUseTracking) return ControlsMode.Direct;

            return preferredControls;
        }

        if (Player.role == SeatRole.Gunner)
        {
            if (Player.gunnerSeat.gunMount && Player.gunnerSeat.gunMount.ForceJoystickControls)
            {
                if (preferredControls == ControlsMode.Direct) return ControlsMode.Direct;
                return ControlsMode.MouseStick;
            }
            return ControlsMode.Tracking;
        }

        return ControlsMode.Direct;
    }
}
public enum ControlsMode
{
    Tracking,
    MouseStick,
    Direct
}
