using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointTracking : MonoBehaviour
{
    public const float futureTime = 1f;
    const float targetMaxAngle = Mathf.PI / 3f;
    const float frontDistance = 1000f;

    const float aggressiveAngle = 8f;

    public static void Tracking(Vector3 target, SofAircraft aircraft, float targetRoll, float levelingFactor, bool assist)
    {
        AircraftAxes axes = TrackingInputs(target, aircraft, targetRoll, levelingFactor, assist);
        aircraft.controls.SetTargetInput(axes, PitchCorrectionMode.FullyAssisted);
    }
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
}
