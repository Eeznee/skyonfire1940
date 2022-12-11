using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.InputSystem;
public static class AircraftControl
{
    public const float futureTime = 1f;
    const float throttleIncrement = 0.0002f;
    const float minAltitude = 50f;
    const float targetMaxAngle = Mathf.PI / 3f;
    const float frontDistance = 500f;
    public static Vector3 AssistTarget(Vector3 target, SofAircraft ctr)
    {
        target -= ctr.transform.position;
        if (target.y < 0f)
        {
            if (ctr.data.ias > ctr.maxSpeed * 0.8f) target = Vector3.up;
            Vector3 frontPoint = ctr.transform.position + Vector3.ProjectOnPlane(ctr.transform.forward * frontDistance, Vector3.up);
            float frontRelAlt =  ctr.data.altitude - GameManager.map.HeightAtPoint(frontPoint);
            float relAlt = Mathf.Min(ctr.data.relativeAltitude,frontRelAlt) - minAltitude;
            float spd = ctr.data.gsp;
            float minCrashDelay = ctr.minCrashDelay * Mathf.Lerp(1f,3f, Mathf.Abs(ctr.data.bankAngle)/180f);
            target.Normalize();
            float verticalSpeed = Mathf.Min(ctr.transform.forward.y ,target.y) * spd;
            float crashDelay = relAlt / -verticalSpeed;
            if (verticalSpeed < 0f && crashDelay < minCrashDelay)
            {
                //target y is adjusted to avoid crash
                Vector3 flatTarget = new Vector3(target.x, 0f, target.z);
                Vector3 emerTarget = new Vector3(ctr.transform.forward.x, 0f, ctr.transform.forward.z);
                target = Vector3.Lerp(emerTarget, flatTarget, Mathf.InverseLerp(0.5f,1f,crashDelay/minCrashDelay * 1.5f - 0.5f));
                target.y = -relAlt / (minCrashDelay * spd);
                target.Normalize();
            }
            target *= 500f;
        }
        return target + ctr.transform.position;
    }
    public static Vector3 TrackingInputs(Vector3 target, SofAircraft ctr, float targetRoll, float levelingFactor,bool assist)
    {
        Vector3 dir = target - ctr.transform.position;
        dir = Vector3.RotateTowards(ctr.transform.forward, dir, targetMaxAngle, 500f);
        target = dir + ctr.transform.position;
        Vector3 axis = Vector3.zero;
        if (ctr.data.ias < 5f) return axis;
        if (assist)
        {
            target = AssistTarget(target, ctr);
        }

        //Target Angles
        Vector3 localTarget = ctr.transform.InverseTransformPoint(target).normalized;
        float offAngle = Vector3.Angle(ctr.transform.forward, target - ctr.transform.position);
        float pitchAngle = Mathf.Asin(localTarget.y) * Mathf.Rad2Deg;
        float rollAngle = Mathf.Asin(localTarget.x) * Mathf.Rad2Deg;

        //Roll
        float levelRoll = Mathv.Angle180(ctr.data.bankAngle - targetRoll) * Mathf.Clamp01(1f - offAngle / ctr.bankTurnAngle) * levelingFactor;
        float agressiveRoll = Mathf.Clamp(rollAngle * Mathf.Lerp(3f,1f,offAngle/90f), -90f, 90f);
        float error = (levelRoll + agressiveRoll)/180f;
        if (assist)
        {
            float rollLimit = Mathf.Lerp(45f,180f,(ctr.data.relativeAltitude - minAltitude) / ctr.minInvertAltitude);
            float multiplier = Mathf.InverseLerp(rollLimit * 1.5f, rollLimit / 2f, Mathf.Abs(ctr.data.bankAngle)); //Value between 0 and 1, 0.5 being the middle
            multiplier = (multiplier - 0.5f) * 2f; //Make it a value between 1 and - 1
            if (Mathf.Sign(error) != Mathf.Sign(ctr.data.bankAngle)) error *= Mathf.Clamp01(multiplier);
            if (multiplier < 0f) error = -multiplier * Mathf.Sign(ctr.data.bankAngle);
        }

        axis.z =  ctr.pidRoll.Update(error, Time.fixedDeltaTime);

        //Pitch
        error = pitchAngle;
        axis.x = ctr.pidPitch.Update(error, Time.fixedDeltaTime);
        axis.x *= 1f - Mathf.Abs(axis.z)*0.5f;
        axis.x *= Mathv.SmoothStart(Mathf.Min(1f, ctr.cruiseSpeed / ctr.data.ias), 2);
        return axis;
    }
    public static void Tracking(Vector3 target, SofAircraft ctr, float targetRoll, float levelingFactor,bool assist)
    {
        Vector3 axis = TrackingInputs(target, ctr, targetRoll, levelingFactor,assist);
        ctr.SetControls(axis, true, false);
    }
    public static void PlayerUpdate(SofAircraft aircraft)
    {
        Actions.PilotActions pilot = PlayerActions.instance.actions.Pilot;
        if (pilot.FirePrimaries.ReadValue<float>() > 0.1f) aircraft.FirePrimaries();
        if (pilot.FireSecondaries.ReadValue<float>() > 0.7f) aircraft.FireSecondaries();
        aircraft.SetFlaps(Mathf.RoundToInt(pilot.Flaps.ReadValue<float>()));
        aircraft.brake = pilot.Brake.ReadValue<float>();
#if MOBILE_INPUT
        aircraft.boost = pilot.Boost.ReadValue<float>() > 0.5f;
#else

        if(PlayerActions.instance.actions.General.Scroll.ReadValue<float>() != 0f)
        {
            float thr = PlayerManager.player.aircraft.engines[0].throttleInput;
            float input = PlayerActions.instance.actions.General.Scroll.ReadValue<float>() * throttleIncrement;
            aircraft.SetThrottle(thr + input);
            aircraft.boost = (thr == 1f && input > 0f) || aircraft.boost;
            if (aircraft.boost && input < 0f) { aircraft.boost = false; aircraft.SetThrottle(1f); }
        }
#endif
    }
    public static void PlayerFixed(SofAircraft aircraft)
    {
        Actions.PilotActions actions = PlayerActions.instance.actions.Pilot;

        Vector3 axis = Vector3.zero;
        bool canUseTracking = PlayerCamera.customCam.dir == CamDirection.Game || PlayerCamera.customCam.dir == CamDirection.Free;
        if (GameManager.trackingControl && canUseTracking) //Tracking input, mouse
        {
            //track
            Vector3 targetPos = aircraft.transform.position + PlayerCamera.directionInput * 500f;
            axis = TrackingInputs(targetPos, aircraft, 0f, PlayerCamera.dynamic ? 0f : 1f,false);

            //override conditions
            bool pitching = actions.Pitch.phase == InputActionPhase.Started;
            bool rolling = actions.Roll.phase == InputActionPhase.Started;
            if (rolling || pitching) axis.z = actions.Roll.ReadValue<float>();
            if (pitching) axis.x = -actions.Pitch.ReadValue<float>();
            axis.y = -actions.Rudder.ReadValue<float>();

            aircraft.SetControls(axis, true, false);
        }
        else //Direct input, joystick, phone
        {
            axis.x = -actions.Pitch.ReadValue<float>();
            if (PlayerPrefs.GetInt("InvertPitch", 0) == 1) axis.x = -axis.x;
            axis.z = actions.Roll.ReadValue<float>();
            axis.y = -actions.Rudder.ReadValue<float>();

            aircraft.SetControls(axis, PlayerPrefs.GetInt("FullElevatorControl", 0) == 0, false);
        }
    }
}
