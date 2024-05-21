using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointTracking : MonoBehaviour
{
    public const float futureTime = 1f;
    const float throttleIncrement = 0.0002f;
    const float minAltitude = 50f;
    const float targetMaxAngle = Mathf.PI / 3f;
    const float frontDistance = 500f;

    const float bankTurnAngle = 8f;

    public static void Tracking(Vector3 target, SofAircraft ctr, float targetRoll, float levelingFactor, bool assist)
    {
        AircraftAxes axes = TrackingInputs(target, ctr, targetRoll, levelingFactor, assist);
        ctr.inputs.SendAxes(axes, true, false);
    }
    public static AircraftAxes TrackingInputs(Vector3 target, SofAircraft ctr, float targetRoll, float levelingFactor, bool assist)
    {
        Vector3 dir = target - ctr.transform.position;
        dir = Vector3.RotateTowards(ctr.transform.forward, dir, targetMaxAngle, 500f);
        target = dir + ctr.transform.position;
        AircraftAxes axes = AircraftAxes.zero;
        if (ctr.data.ias.Get < 5f) return axes;

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
        float levelRoll = Mathv.Angle180(ctr.data.bankAngle.Get - targetRoll) * Mathf.Clamp01(1f - offAngle / bankTurnAngle) * levelingFactor;
        float agressiveRoll = Mathf.Clamp(rollAngle * Mathf.Lerp(3f, 1f, offAngle / 90f), -90f, 90f);
        float error = (levelRoll + agressiveRoll) / 180f;
        if (assist)
        {
            float rollLimit = Mathf.Lerp(45f, 180f, (ctr.data.relativeAltitude.Get - minAltitude) / ctr.minInvertAltitude);
            float multiplier = Mathf.InverseLerp(rollLimit * 1.5f, rollLimit / 2f, Mathf.Abs(ctr.data.bankAngle.Get)); //Value between 0 and 1, 0.5 being the middle
            multiplier = (multiplier - 0.5f) * 2f; //Make it a value between 1 and - 1
            if (Mathf.Sign(error) != Mathf.Sign(ctr.data.bankAngle.Get)) error *= Mathf.Clamp01(multiplier);
            if (multiplier < 0f) error = -multiplier * Mathf.Sign(ctr.data.bankAngle.Get);
        }

        axes.roll = ctr.pidRoll.Update(error, Time.fixedDeltaTime);

        //Pitch
        error = pitchAngle;
        axes.pitch = ctr.pidPitch.Update(error, Time.fixedDeltaTime);
        axes.pitch *= 1f - Mathf.Abs(axes.yaw) * 0.5f;
        axes.pitch *= Mathv.SmoothStart(Mathf.Min(1f, ctr.cruiseSpeed / ctr.data.ias.Get), 2);
        return axes;
    }

    public static Vector3 AssistTarget(Vector3 target, SofAircraft ctr)
    {
        target -= ctr.transform.position;
        if (target.y < 0f)
        {
            if (ctr.data.ias.Get > ctr.maxSpeed * 0.8f) target = Vector3.up;
            Vector3 frontPoint = ctr.transform.position + Vector3.ProjectOnPlane(ctr.transform.forward * frontDistance, Vector3.up);
            float frontRelAlt = ctr.data.altitude.Get - GameManager.map.HeightAtPoint(frontPoint);
            float relAlt = Mathf.Min(ctr.data.relativeAltitude.Get, frontRelAlt) - minAltitude;
            float spd = ctr.data.gsp.Get;
            float minCrashDelay = ctr.minCrashDelay * Mathf.Lerp(1f, 3f, Mathf.Abs(ctr.data.bankAngle.Get) / 180f);
            target.Normalize();
            float verticalSpeed = Mathf.Min(ctr.transform.forward.y, target.y) * spd;
            float crashDelay = relAlt / -verticalSpeed;
            if (verticalSpeed < 0f && crashDelay < minCrashDelay)
            {
                //target y is adjusted to avoid crash
                Vector3 flatTarget = new Vector3(target.x, 0f, target.z);
                Vector3 emerTarget = new Vector3(ctr.transform.forward.x, 0f, ctr.transform.forward.z);
                target = Vector3.Lerp(emerTarget, flatTarget, Mathf.InverseLerp(0.5f, 1f, crashDelay / minCrashDelay * 1.5f - 0.5f));
                target.y = -relAlt / (minCrashDelay * spd);
                target.Normalize();
            }
            target *= 500f;
        }
        return target + ctr.transform.position;
    }
}
