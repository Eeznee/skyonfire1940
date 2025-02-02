using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class NewPointTracking
{
    static bool InvertFacingTarget(SofAircraft aircraft, Vector3 targetDirection)
    {
        Transform tr = aircraft.transform;
        float angleOffTarget = Vector3.Angle(tr.forward, targetDirection);

        float rollRate = aircraft.stats.RollRateCurrentSpeed();

        Vector3 flattenedTarget = Vector3.ProjectOnPlane(targetDirection, tr.forward);
        float turnRatePos = aircraft.stats.MaxTurnRate;
        float turnRateNeg = aircraft.stats.MaxNegTurnRate * 0.8f;

        float timeToReachFacing = TotalTimeToReachTarget(angleOffTarget, rollRate, turnRatePos, flattenedTarget, tr.up);
        float timeToReachBacking = TotalTimeToReachTarget(angleOffTarget, rollRate, turnRateNeg, flattenedTarget, -tr.up);
        
        return (timeToReachBacking < timeToReachFacing) && timeToReachFacing > 0.2f;
    }

    static float TotalTimeToReachTarget(float angleOffTarget, float rollRate, float turnRate, Vector3 flattenedTarget, Vector3 transformUp)
    {
        float rollAmountToFace = Vector3.Angle(transformUp, flattenedTarget);

        float timeToReachTarget = 0f;
        timeToReachTarget += rollAmountToFace / rollRate * Mathf.Clamp01(angleOffTarget / 20f);
        timeToReachTarget += angleOffTarget / turnRate;

        return timeToReachTarget;
    }
}
