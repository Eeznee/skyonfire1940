using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointTracking
{
    /*
public const float futureTime = 1f;
const float targetMaxAngle = Mathf.PI / 3f;
const float frontDistance = 1000f;

const float aggressiveAngle = 8f;



public static AircraftAxes TrackingInputs(Vector3 target, SofAircraft aircraft, float targetRoll, float levelingFactor, bool assist)
{
    Vector3 dir = target - aircraft.transform.position;
    dir = Vector3.RotateTowards(aircraft.transform.forward, dir, targetMaxAngle, 500f);
    target = dir + aircraft.transform.position;
    AircraftAxes axes = AircraftAxes.zero;
    if (aircraft.data.ias.Get < 5f) return axes;

    float bankLimit = Mathf.Infinity;
    if (assist)
    {
        PreventGroundCrash(ref target, out bankLimit, aircraft);
        targetRoll = Mathf.Clamp(targetRoll, -bankLimit ,bankLimit);
    }

    //Target Angles
    Vector3 localTarget = aircraft.transform.InverseTransformPoint(target).normalized;
    float offAngle = Vector3.Angle(aircraft.transform.forward, target - aircraft.transform.position);
    float pitchAngle = Mathf.Asin(localTarget.y) * Mathf.Rad2Deg;
    float rollAngle = Mathf.Asin(localTarget.x) * Mathf.Rad2Deg;

    //Roll
    float levelRoll = Mathv.Angle180(aircraft.data.bankAngle.Get - targetRoll) * Mathf.Clamp01(1f - offAngle / aggressiveAngle) * levelingFactor;
    float agressiveRoll = Mathf.Clamp(rollAngle * Mathf.Lerp(3f, 1f, offAngle / 90f), -90f, 90f);
    float error = (levelRoll + agressiveRoll) / 180f;

    axes.roll = aircraft.pidRoll.Update(error, Time.fixedDeltaTime);

    if (assist && Mathf.Abs(aircraft.data.bankAngle.Get) > bankLimit)
    {
        float excessBank = Mathf.Abs(aircraft.data.bankAngle.Get) - bankLimit;
        axes.roll += Mathf.Sign(aircraft.data.bankAngle.Get) * excessBank * 0.1f;
    }

    //Pitch
    error = pitchAngle;
    axes.pitch = aircraft.pidPitch.Update(error, Time.fixedDeltaTime);
    //axes.pitch *= 1f - Mathf.Abs(axes.yaw) * 0.5f;
    //axes.pitch *= Mathv.SmoothStart(Mathf.Min(1f, ctr.cruiseSpeed / ctr.data.ias.Get), 2);
    return axes;
}

public static void PreventGroundCrash(ref Vector3 targetPos, out float bankLimit, SofAircraft aircraft)
{
    bankLimit = Mathf.Infinity;
    if (aircraft.data.ias.Get > aircraft.SpeedLimitMps * 0.85f)
    {
        targetPos = aircraft.transform.position + Vector3.up * 500f;
        return;
    }

    Vector3 targetDir = targetPos - aircraft.transform.position;
    targetDir.Normalize();

    Vector3 frontPoint = aircraft.transform.position + aircraft.transform.forward * frontDistance;

    bool flyingOverSea = Mathf.Abs(aircraft.data.relativeAltitude.Get - aircraft.data.altitude.Get) < 1f;
    float minRelativeAltitude = aircraft.data.altitude.Get - GameManager.mapTool.HeightAtPoint(frontPoint);
    minRelativeAltitude = Mathf.Min(aircraft.data.relativeAltitude.Get, minRelativeAltitude);

    float minAltitudeAllowed = flyingOverSea ? 20f : 50f; //min altitude tolerance is lower when flying above sea
    float timeToAvoidCrash = 2f * 90f / aircraft.stats.MaxTurnRate + 180f / aircraft.stats.RollRateCurrentSpeed();
    float altitudeWhenDelayIsReached = minRelativeAltitude + aircraft.data.vsp.Get * timeToAvoidCrash;

    if (altitudeWhenDelayIsReached > minAltitudeAllowed + 150f) return;

    float bankLimitFactor = Mathf.InverseLerp(minAltitudeAllowed + 150f, minAltitudeAllowed, altitudeWhenDelayIsReached);
    bankLimit = Mathf.Lerp(180f,10f, bankLimitFactor);

    if (altitudeWhenDelayIsReached > minAltitudeAllowed) return;

    float factor = Mathf.InverseLerp(minAltitudeAllowed, minAltitudeAllowed - 100f, altitudeWhenDelayIsReached);
    targetDir = Vector3.ProjectOnPlane(targetDir, Vector3.up).normalized;
    targetDir = Vector3.Lerp(targetDir, Vector3.up, factor).normalized;

    targetPos = targetDir * 500f + aircraft.transform.position;
}

const float rollYawAlpha = 14f;
private const float maxMultiplierValue = 8f;
private const float minMultiplierValue = 0.01f;

private const int iterationsSteps = 3;
private const int simSteps = 4;
private const float dtMultiplier = 3f;


public static AircraftAxes FindOptimalControls2(Vector3 targetDirection, SofAircraft aircraft, AircraftAxes forcedAxes, float maintainBankFactor)
{
    targetDirection.Normalize();
    float offAngle = Vector3.Angle(aircraft.rb.linearVelocity, targetDirection);
    float offAngleFactor = Mathf.Clamp01(offAngle / rollYawAlpha);

    float maxPitch = PitchCorrection.MaxPitchAbs(aircraft, Mathf.Sign(aircraft.controls.current.pitch));
    if (!float.IsNaN(forcedAxes.pitch)) forcedAxes.pitch = Mathf.Clamp(forcedAxes.pitch, -maxPitch, maxPitch);

    AircraftAxes controlsFound = AircraftAxes.zero;
    Vector3 previousLocalTarget = aircraft.tr.InverseTransformDirection(targetDirection);

    for (int i = 0; i < iterationsSteps; i++)
    {
        controlsFound = GetControls2(aircraft.ptAbstractControls, maxPitch, offAngleFactor, maintainBankFactor, aircraft, !float.IsNaN(forcedAxes.roll));
        ApplyForcedAxis(ref controlsFound, forcedAxes);
        controlsFound.Clamp(maxPitch);

        FlightConditions simConditions = FullSimulation(aircraft, controlsFound, simSteps, Time.fixedDeltaTime * dtMultiplier);

        Vector3 localTarget = Quaternion.Inverse(simConditions.rotation) * targetDirection;

        bool verticalOvershoots = Mathf.Sign(previousLocalTarget.y * localTarget.y) == -1f;
        bool horizontalOvershoots = Mathf.Sign(previousLocalTarget.x * localTarget.x) == -1f;
        aircraft.ptMultipliers.y = Mathf.Clamp(aircraft.ptMultipliers.y * (verticalOvershoots ? 0.5f : 2f), minMultiplierValue, maxMultiplierValue);
        aircraft.ptMultipliers.x = Mathf.Clamp(aircraft.ptMultipliers.x * (horizontalOvershoots ? 0.5f : 2f), minMultiplierValue, maxMultiplierValue);

        aircraft.ptAbstractControls.y += aircraft.ptMultipliers.y * localTarget.y;
        aircraft.ptAbstractControls.x += aircraft.ptMultipliers.x * localTarget.x;
        aircraft.ptAbstractControls.ClampUnitCube();

        previousLocalTarget = localTarget;
    }

    controlsFound = InterpolateForGrounded(controlsFound, aircraft, previousLocalTarget);
    ApplyForcedAxis(ref controlsFound, forcedAxes);

    return controlsFound;
}
private static AircraftAxes GetControls2(Vector2 abstractControls, float maxPitch, float offAngleFactor, float maintainBankFactor, SofAircraft aircraft, bool forceRoll)
{
    AircraftAxes controls;
    controls.yaw = -abstractControls.x * (1f - offAngleFactor);

    float maintainBank = Mathf.Clamp(aircraft.data.rollRate.Get * -0.02f, -1f, 1f);
    float levelRoll = Mathf.Clamp(aircraft.tr.right.y * 5f + aircraft.data.rollRate.Get * -0.02f, -levelRollLimit, levelRollLimit);
    float passiveRoll = Mathf.Lerp(levelRoll, maintainBank, maintainBankFactor);

    float aggressiveRoll = abstractControls.x;

    controls.roll = Mathf.Lerp(passiveRoll, aggressiveRoll, offAngleFactor);

    float slowDownRoll = aircraft.controls.axesSpeed.pitch * Time.fixedDeltaTime * Mathf.Lerp(1f, 0.4f, maintainBankFactor * (1f - offAngleFactor));
    controls.roll = Mathf.MoveTowards(aircraft.controls.current.roll, controls.roll, slowDownRoll);

    controls.pitch = abstractControls.y * maxPitch;
    if (!forceRoll) controls.pitch *= Mathf.Clamp01(1.5f - Mathf.Abs(controls.roll) * offAngleFactor);

    return controls;
}

public static AircraftAxes FindOptimalControls(Vector3 targetDirection, SofAircraft aircraft, AircraftAxes forcedAxes, float maintainBankFactor)
{
    targetDirection.Normalize();
    float offAngleFactor = Mathf.Clamp01(Vector3.Angle(aircraft.tr.forward, targetDirection) / rollYawAlpha);
    float offAngleYaw = Vector3.Angle(aircraft.rb.linearVelocity, targetDirection);

    float maxPitch = PitchCorrection.MaxPitchAbs(aircraft, Mathf.Sign(aircraft.controls.current.pitch));
    if (!float.IsNaN(forcedAxes.pitch)) forcedAxes.pitch = Mathf.Clamp(forcedAxes.pitch, -maxPitch, maxPitch);
    float maxRoll = Mathf.InverseLerp(240f, 120f, Mathf.Abs(aircraft.data.rollRate.Get));

    AircraftAxes controlsFound = AircraftAxes.zero;
    controlsFound = aircraft.controls.current;

    Vector3 currentLocalTarget = aircraft.tr.InverseTransformDirection(targetDirection);
    float to = Mathv.Angle180(TargetBank(currentLocalTarget, aircraft.tr.rotation, offAngleFactor));
    if (offAngleFactor >= 0.8f)
    {
        aircraft.targetBank = to;
    }
    else
    {
        //ok

        float slip = Vector3.SignedAngle(Vector3.ProjectOnPlane(aircraft.rb.linearVelocity, Vector3.up), Vector3.ProjectOnPlane(aircraft.tr.rotation * Vector3.forward, Vector3.up), Vector3.up);
        slip = 0f;
        aircraft.targetBank = Mathf.MoveTowards(aircraft.targetBank, slip * 3f, Time.fixedDeltaTime * 15f * (Mathf.Abs(aircraft.data.angleOfSlip.Get) * 0.2f + 1f));
        aircraft.targetBank = Mathf.MoveTowards(aircraft.targetBank, to, Time.fixedDeltaTime * offAngleFactor * 180f);
    }

    float bankDelta = aircraft.ptPreviousRoll * 90f;


    for (int i = 0; i < iterationsSteps; i++)
    {
        // GetControls(aircraft.ptAbstractControls, maxPitch, offAngleFactor, 0f, aircraft);// bankDelta, aircraft);
        controlsFound.pitch = Mathf.Clamp(aircraft.controls.current.pitch + aircraft.ptAbstractControls.y, -maxPitch, maxPitch);
        //controlsFound.yaw = Mathf.Clamp(aircraft.controls.current.yaw + aircraft.ptAbstractControls.x, -1f, 1f);
        //controlsFound.roll = Mathf.Clamp(aircraft.controls.current.roll + aircraft.ptAbstractControls.z, -1f, 1f);
        //if (offAngleYaw > rollYawAlpha) controlsFound.yaw = Mathf.MoveTowards(aircraft.controls.current.yaw, 0f, Time.fixedDeltaTime * 0.2f);

        aircraft.ptAbstractControls.y = controlsFound.pitch - aircraft.controls.current.pitch;
        aircraft.ptAbstractControls.x = controlsFound.yaw - aircraft.controls.current.yaw;
        aircraft.ptAbstractControls.z = controlsFound.roll - aircraft.controls.current.roll;

        ApplyForcedAxis(ref controlsFound, forcedAxes);
        //controlsFound.Clamp(maxPitch, maxRoll);
        FlightConditions simConditions = FullSimulation(aircraft, controlsFound, simSteps, Time.fixedDeltaTime * dtMultiplier);

        Vector3 localTarget = Quaternion.Inverse(simConditions.rotation) * targetDirection;

        bool verticalOvershoots = Mathf.Sign(aircraft.ptPreviousLocalTarget.y * localTarget.y) == -1f;
        bool horizontalOvershoots = Mathf.Sign(aircraft.ptPreviousLocalTarget.x * localTarget.x) == -1f;
        float roll = Mathv.Angle180(aircraft.targetBank + simConditions.rotation.eulerAngles.z) / 90f;
        bool rollOvershoots = Mathf.Sign(roll * aircraft.ptPreviousRoll) == -1f;

        aircraft.ptMultipliers.y = Mathf.Clamp(aircraft.ptMultipliers.y * (verticalOvershoots ? 0.33f : 2f), minMultiplierValue, maxMultiplierValue);
        aircraft.ptMultipliers.x = Mathf.Clamp(aircraft.ptMultipliers.x * (horizontalOvershoots ? 0.33f : 2f), minMultiplierValue, maxMultiplierValue);
        aircraft.ptMultipliers.z = Mathf.Clamp(aircraft.ptMultipliers.z * (rollOvershoots ? 0.33f : 2f), minMultiplierValue, maxMultiplierValue / 64f);

        aircraft.ptAbstractControls.y += localTarget.y * aircraft.ptMultipliers.y;
        aircraft.ptAbstractControls.x += -localTarget.x * aircraft.ptMultipliers.x;
        aircraft.ptAbstractControls.z += localTarget.x * aircraft.ptMultipliers.z;


        float maxDelta = 4f * Time.fixedDeltaTime * simSteps * dtMultiplier;
        aircraft.ptAbstractControls.y = Mathf.Clamp(aircraft.ptAbstractControls.y, -maxDelta, maxDelta);
        aircraft.ptAbstractControls.x = Mathf.Clamp(aircraft.ptAbstractControls.x, -maxDelta * 2f, maxDelta * 2f);
        aircraft.ptAbstractControls.z = Mathf.Clamp(aircraft.ptAbstractControls.z, -maxDelta, maxDelta);
        //aircraft.ptAbstractControls.ClampUnitCube();

        aircraft.ptPreviousLocalTarget = localTarget;
        aircraft.ptPreviousRoll = roll;
    }

    //controlsFound = InterpolateForGrounded(controlsFound, aircraft, previousLocalTarget);

    ApplyForcedAxis(ref controlsFound, forcedAxes);

    return controlsFound;
}

private static float TargetBank(Vector3 localTarget, Quaternion rotation, float offAngleFactor)
{
    float passiveRoll = Mathv.Angle180(rotation.eulerAngles.z);
    float activeRoll = Mathf.Atan2(localTarget.x, localTarget.y) * Mathf.Rad2Deg;
    if (Mathf.Abs(activeRoll) > 120f) activeRoll = (activeRoll + 180f) % 360f;
    if (Mathf.Abs(activeRoll) > 120f && offAngleFactor < 1f) activeRoll = 180f + activeRoll;

    float combined = Mathf.LerpAngle(passiveRoll, activeRoll, offAngleFactor);

    return Mathv.Angle180(combined - rotation.eulerAngles.z);
}

const float levelRollLimit = 0.2f;
private static AircraftAxes GetControls(Vector3 abstractControls, float maxPitch, float offAngleFactor, float bankDelta, SofAircraft aircraft)
{
    AircraftAxes controls;

    controls.yaw = -abstractControls.x * (1f - offAngleFactor);

    //float maintainBank = Mathf.Clamp(aircraft.data.rollRate.Get * -0.02f, -1f, 1f);
    controls.roll = abstractControls.z;// Mathf.Lerp(abstractControls.z, maintainBank,  maintainBankFactor);

    float bankFactor = Mathf.Abs(bankDelta) / 30f;
    float targetPitch = abstractControls.y * maxPitch * Mathf.Clamp01(1f - bankFactor);
    float currentPitch = aircraft.controls.target.pitch;
    controls.pitch = Mathf.MoveTowards(currentPitch, targetPitch, Time.fixedDeltaTime * Mathf.Lerp(4f, 0.1f, Mathf.Clamp01(bankFactor)));

    return controls;
}
*/
}
