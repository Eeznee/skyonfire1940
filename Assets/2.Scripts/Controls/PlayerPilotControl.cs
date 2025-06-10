using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class PlayerPilotControl
{
    public static float maintainBank = 0f;
    public static bool forceCameraPointDirection = false;
    private static float forceCameraPointDirectionTimer = 0f;

    public const float throttleIncrement = 0.0002f;

    public static void ControlsUpdate(CrewMember crew)
    {
        SofAircraft aircraft = crew.aircraft;

        Actions.PilotActions pilot = PlayerActions.pilot;
        if (pilot.FirePrimaries.ReadValue<float>() > 0.1f) aircraft.armament.FirePrimaries();
        if (pilot.FireSecondaries.ReadValue<float>() > 0.7f) aircraft.armament.FireSecondaries();
        aircraft.hydraulics.SetFlaps(Mathf.RoundToInt(pilot.Flaps.ReadValue<float>()));
        aircraft.controls.brake = pilot.Brake.ReadValue<float>();

        float scrollValue = PlayerActions.general.Scroll.ReadValue<float>();
        if (scrollValue != 0f)
        {
            float currentThrottle = aircraft.engines.Throttle;
            float throttleIncrement = scrollValue * PlayerPilotControl.throttleIncrement;

            bool maxedThrottleAndPositiveIncrement = currentThrottle >= 1f && throttleIncrement > 0f;
            bool boostedAndNegativeIncrement = aircraft.engines.Throttle.Boost && throttleIncrement < 0f;

            if (maxedThrottleAndPositiveIncrement)
                aircraft.engines.SetThrottleAllEngines(1.1f, true);

            else if (boostedAndNegativeIncrement)

                aircraft.engines.SetThrottleAllEngines(1f, false);

            else
                aircraft.engines.SetThrottleAllEngines(currentThrottle + throttleIncrement, false);
        }
    }
    public static void ControlAxesFixedUpdate(CrewMember crew)
    {
        SofAircraft aircraft = crew.aircraft;
        Actions.PilotActions actions = PlayerActions.pilot;
        AircraftAxes axes = AircraftAxes.zero;

        float pitch = -actions.Pitch.ReadValue<float>();
        if (PlayerPrefs.GetInt("InvertPitch", 0) == 1) pitch = -pitch;
        float roll = actions.Roll.ReadValue<float>();
        float yaw = -actions.Rudder.ReadValue<float>();

        if (ControlsManager.CurrentMode() == ControlsMode.Tracking) //Tracking input, mouse
        {
            bool pitching = actions.Pitch.phase == InputActionPhase.Started;
            bool rolling = actions.Roll.phase == InputActionPhase.Started;
            bool yawing = actions.Rudder.phase == InputActionPhase.Performed;

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


            //PointTracking.Tracking(aircraft.tr.position + SofCamera.directionInput * 500f, aircraft, 0f, 0f, true);
            axes = NewPointTracking.FindOptimalControls(SofCamera.directionInput, aircraft, forcedAxes, SofCamera.lookAround ? 1f : maintainBank);
            aircraft.controls.SetTargetInput(axes, PitchCorrectionMode.Raw);
        }
        else //Direct input, joystick, phone
        {
            axes = new AircraftAxes(pitch, roll, yaw);
            aircraft.controls.SetTargetInput(axes, ControlsManager.pitchCorrectionMode);
        }
    }
}
