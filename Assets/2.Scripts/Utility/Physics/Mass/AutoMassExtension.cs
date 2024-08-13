using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AutoMassExtension
{
    public static void ComputeAutoMass(this SofComplex complex, Mass targetEmptyMass)
    {
        IMassComponent[] massComponents = complex.GetComponentsInChildren<IMassComponent>();
        SofAirframe[] airframes = complex.GetComponentsInChildren<SofAirframe>();

        foreach (SofAirframe airframe in airframes)
            airframe.mass = airframe.ApproximateMass();

        Mass fixedMass = new Mass(massComponents, true) - new Mass(airframes, true);
        Mass targetAirframeMass = targetEmptyMass - fixedMass;

        if (targetAirframeMass.mass < 0f) Debug.LogError("Target mass is too small, fixed mass parts already go above that weight !", complex);
        
        ApproximateAirframesMass(targetAirframeMass, airframes);
        BalanceFrontAndBack(targetAirframeMass, airframes);
    }
    static void ApproximateAirframesMass(Mass targetMass, SofAirframe[] airframes)
    {
        Mass approximated = new Mass(0f, Vector3.zero);
        foreach (SofAirframe airframe in airframes)
        {
            Vector3 localPos = airframe.transform.root.InverseTransformPoint(airframe.transform.position);
            approximated.mass += airframe.ApproximateMass();
            approximated.center += localPos * airframe.ApproximateMass();
        }
        approximated.center /= approximated.mass;

        float factor = targetMass.mass / approximated.mass;
        foreach (SofAirframe airframe in airframes)
            airframe.mass = factor * airframe.ApproximateMass();
    }
    static void BalanceFrontAndBack(Mass targetMass, SofAirframe[] airframes)
    {
        List<SofAirframe> frontFrames = new List<SofAirframe>();
        List<SofAirframe> backFrames = new List<SofAirframe>();
        foreach (SofAirframe airframe in airframes)
        {
            Vector3 localPos = airframe.transform.root.InverseTransformPoint(airframe.transform.position);
            if (localPos.z > targetMass.center.z)
                frontFrames.Add(airframe);
            else if (localPos.z < targetMass.center.z)
                backFrames.Add(airframe);
        }
        if (frontFrames.Count == 0 || backFrames.Count == 0) Debug.LogError("Target center of gravity is impossible to reach");

        ComputeFrontBackFactors(targetMass, frontFrames, backFrames, out float frontFactor, out float backFactor);

        foreach (SofAirframe airframe in frontFrames)
            airframe.mass *= frontFactor;
        foreach (SofAirframe airframe in backFrames)
            airframe.mass *= backFactor;
    }
    static void ComputeFrontBackFactors(Mass targetMass, List<SofAirframe> frontFrames, List<SofAirframe> backFrames, out float frontFactor, out float backFactor)
    {
        Mass front = new Mass(frontFrames.ToArray(), true);
        Mass back = new Mass(backFrames.ToArray(), true);

        float targetZ = targetMass.center.z;
        float frontZ = front.center.z;
        float backZ = back.center.z;
        float centerShiftMass = (front.mass * (targetZ - frontZ) + back.mass * (targetZ - backZ)) / (frontZ - backZ);
        frontFactor = 1f + centerShiftMass / front.mass;
        backFactor = 1f - centerShiftMass / back.mass;
    }
}
