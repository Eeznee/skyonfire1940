using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AutoMassExtension
{
    public static void ComputeAutoMass(this SofModular complex, Mass targetEmptyMass)
    {
        SofAircraft aircraft = complex.GetComponent<SofAircraft>();
        if (aircraft) 
        {
            aircraft.hydraulics.SetAllHydraulicsToDefaultPosition();
        }

        IMassComponent[] massComponents = complex.GetComponentsInChildren<IMassComponent>();
        SofFrame[] frames = complex.GetComponentsInChildren<SofFrame>();

        foreach (SofFrame frame in frames)
            frame.mass = frame.ApproximateMass();

        Mass fixedMass = new Mass(massComponents, MassCategory.Empty) - new Mass(frames, MassCategory.Empty);
        Mass targetFrameMass = targetEmptyMass - fixedMass;

        if (targetFrameMass.mass < 0f) Debug.LogError("Target mass is too small, fixed mass parts already go above that weight !", complex);
        
        ApproximateFramesMass(targetFrameMass, frames);
        BalanceFrontAndBack(targetFrameMass, frames);
    }
    static void ApproximateFramesMass(Mass targetMass, SofFrame[] frames)
    {
        Mass approximated = new Mass(0f, Vector3.zero);
        foreach (SofFrame frame in frames)
        {
            Vector3 localPos = frame.transform.root.InverseTransformPoint(frame.transform.position);
            approximated.mass += frame.ApproximateMass();
            approximated.center += localPos * frame.ApproximateMass();
        }
        approximated.center /= approximated.mass;

        float factor = targetMass.mass / approximated.mass;
        foreach (SofFrame frame in frames)
            frame.mass = factor * frame.ApproximateMass();
    }
    static void BalanceFrontAndBack(Mass targetMass, SofFrame[] frames)
    {
        List<SofFrame> frontFrames = new List<SofFrame>();
        List<SofFrame> backFrames = new List<SofFrame>();
        foreach (SofFrame frame in frames)
        {
            Vector3 localPos = frame.transform.root.InverseTransformPoint(frame.transform.position);
            if (localPos.z > targetMass.center.z)
                frontFrames.Add(frame);
            else if (localPos.z < targetMass.center.z)
                backFrames.Add(frame);
        }
        if (frontFrames.Count == 0 || backFrames.Count == 0) Debug.LogError("Target center of gravity is impossible to reach");

        ComputeFrontBackFactors(targetMass, frontFrames, backFrames, out float frontFactor, out float backFactor);

        foreach (SofFrame frame in frontFrames)
            frame.mass *= frontFactor;
        foreach (SofFrame frame in backFrames)
            frame.mass *= backFactor;
    }
    static void ComputeFrontBackFactors(Mass targetMass, List<SofFrame> frontFrames, List<SofFrame> backFrames, out float frontFactor, out float backFactor)
    {
        Mass front = new Mass(frontFrames.ToArray(), MassCategory.Empty);
        Mass back = new Mass(backFrames.ToArray(), MassCategory.Empty);

        float targetZ = targetMass.center.z;
        float frontZ = front.center.z;
        float backZ = back.center.z;
        float centerShiftMass = (front.mass * (targetZ - frontZ) + back.mass * (targetZ - backZ)) / (frontZ - backZ);
        frontFactor = 1f + centerShiftMass / front.mass;
        backFactor = 1f - centerShiftMass / back.mass;
    }
}
