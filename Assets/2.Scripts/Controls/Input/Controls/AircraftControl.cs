using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.InputSystem;
public static class AircraftControl
{
    public const float futureTime = 1f;
    const float throttleIncrement = 0.0002f;

    public static void PlayerUpdate(SofAircraft aircraft)
    {
        Actions.PilotActions pilot = PlayerActions.pilot;
        if (pilot.FirePrimaries.ReadValue<float>() > 0.1f) aircraft.armament.FirePrimaries();
        if (pilot.FireSecondaries.ReadValue<float>() > 0.7f) aircraft.armament.FireSecondaries();
        aircraft.hydraulics.SetFlaps(Mathf.RoundToInt(pilot.Flaps.ReadValue<float>()));
        aircraft.inputs.brake = pilot.Brake.ReadValue<float>();
#if MOBILE_INPUT
        aircraft.boost = pilot.Boost.ReadValue<float>() > 0.5f;
#else

        if(PlayerActions.menu.Scroll.ReadValue<float>() != 0f)
        {
            float thr = Player.aircraft.engines.throttle;
            float input = PlayerActions.menu.Scroll.ReadValue<float>() * throttleIncrement;
            aircraft.engines.SetThrottle(thr + input);
            aircraft.engines.boost = (thr == 1f && input > 0f) || aircraft.engines.boost;
            if (aircraft.engines.boost && input < 0f) { aircraft.engines.boost = false; aircraft.engines.SetThrottle(1f); }
        }
#endif
    }
    public static void PlayerFixed(SofAircraft aircraft)
    {
        Actions.PilotActions actions = PlayerActions.pilot;
        AircraftAxes axes = AircraftAxes.zero;
        if (ControlsManager.CurrentMode() == ControlsMode.Tracking) //Tracking input, mouse
        {
            //track
            Vector3 targetPos = aircraft.transform.position + SofCamera.directionInput * 500f;
            axes = PointTracking.TrackingInputs(targetPos, aircraft, 0f, PlayerActions.dynamic ? 0f : 1f, false);

            //override conditions
            bool pitching = actions.Pitch.phase == InputActionPhase.Started;
            bool rolling = actions.Roll.phase == InputActionPhase.Started;
            if (rolling || pitching) axes.roll = actions.Roll.ReadValue<float>();
            if (pitching)
            {
                axes.pitch = -actions.Pitch.ReadValue<float>();
                if (PlayerPrefs.GetInt("InvertPitch", 0) == 1) axes.pitch = -axes.pitch;
            }
            axes.yaw = -actions.Rudder.ReadValue<float>();

            aircraft.inputs.SendAxes(axes, true, false);
        }
        else //Direct input, joystick, phone
        {
            axes.pitch = -actions.Pitch.ReadValue<float>();
            if (PlayerPrefs.GetInt("InvertPitch", 0) == 1) axes.pitch = -axes.pitch;
            axes.roll = actions.Roll.ReadValue<float>();
            axes.yaw = -actions.Rudder.ReadValue<float>();
            aircraft.inputs.SendAxes(axes, !ControlsManager.fullElevator, false);
        }
    }
}
