using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitchCorrection
{
    public static float AoALimitSigned(SofAircraft aircraft, float pitchSign)
    {
        float maximumAoA = aircraft.stats.mainAirfoil.HighPeakAlpha - aircraft.StallMarginAngle + aircraft.stats.wingsIncidence;
        float minimumAoA = aircraft.stats.mainAirfoil.LowPeakAlpha + aircraft.StallMarginAngle + aircraft.stats.wingsIncidence;
        return pitchSign >= 0f ? maximumAoA : minimumAoA;
    }
    public static float CorrectPitch(SofAircraft aircraft, float targetPitch, bool correctZeroAoA)
    {
        float targetAoA = Mathf.Abs(targetPitch) * AoALimitSigned(aircraft, targetPitch);

        if (correctZeroAoA)
            CorrectZeroAoAForZeroTurnRate(ref targetAoA, aircraft, targetPitch);

        FlightConditions fc = SpecificAoAFlightConditions(aircraft, targetAoA);

        ResultingForce resultingForce = aircraft.GetComponent<ForcesCompiler>().Compute(fc);
        float pitchTorque = Vector3.Dot(resultingForce.torque, aircraft.tr.right);

        ResultingForce resultingGradient = ElevatorsGradient(fc, targetAoA);
        float gradientPitchTorque = Vector3.Dot(resultingGradient.torque, aircraft.tr.right);
        if (gradientPitchTorque == 0f) return aircraft.controls.current.pitch;

        //Using elevator potential force and simulated forces, find the elevator adjustment needed to maintain target aoa
        float elevatorAdjustment = pitchTorque / gradientPitchTorque;

        return aircraft.controls.current.pitch + elevatorAdjustment;
    }

    public static float MaxPitchAbs(SofAircraft aircraft, float pitchSign)
    {
        if (pitchSign == 0f) pitchSign = 1f;
        float maxPitch = CorrectPitch(aircraft, Mathf.Sign(pitchSign), false);
        return Mathf.Clamp01(Mathf.Abs(maxPitch));
    }

    private static void CorrectZeroAoAForZeroTurnRate(ref float targetAoA, SofAircraft aircraft,  float targetPitch)
    {
        float offsetAoA = targetAoA - Mathf.Sign(targetAoA);

        FlightConditions defaultFc = SpecificAoAFlightConditions(aircraft, targetAoA);
        FlightConditions offsetFc = SpecificAoAFlightConditions(aircraft, offsetAoA);

        ResultingForce target = aircraft.GetComponent<ForcesCompiler>().Compute(defaultFc);
        ResultingForce offset = aircraft.GetComponent<ForcesCompiler>().Compute(offsetFc);

        float targetG = Vector3.Dot(target.force, aircraft.tr.up);
        float offsetG = Vector3.Dot(offset.force, aircraft.tr.up);
        float weight = -Physics.gravity.y * aircraft.rb.mass * Vector3.Dot(aircraft.tr.up, Vector3.up);

        float lerpT = InvLerp(offsetG, targetG, weight);
        float zeroAoA = Mathf.LerpUnclamped(offsetAoA, targetAoA, lerpT);


        targetAoA = Mathf.LerpUnclamped(zeroAoA, AoALimitSigned(aircraft,targetPitch), Mathf.Abs(targetPitch));
    }

    private static FlightConditions SpecificAoAFlightConditions(SofAircraft aircraft, float targetAoA)
    {
        Vector3 simVelocity = Quaternion.Euler(aircraft.tr.right * targetAoA) * aircraft.tr.forward * aircraft.data.tas.Get;

        float maxTurnRate = aircraft.stats.MaxTurnRate * Mathf.Deg2Rad;
        float aoaLimit = Mathf.Abs(AoALimitSigned(aircraft, targetAoA));
        Vector3 angVelocity = aircraft.tr.right * maxTurnRate * targetAoA / aoaLimit;

        FlightConditions fc = new (aircraft, simVelocity, angVelocity, aircraft.controls.current);

        return fc;
    }
    private static ResultingForce ElevatorsGradient(FlightConditions fc, float targetAoA)
    {
        IPitchControlled[] pitchControlSurfaces = fc.complex.GetComponentsInChildren<IPitchControlled>();
        ForceAtPoint[] liftGradients = new ForceAtPoint[pitchControlSurfaces.Length];
        for (int i = 0; i < liftGradients.Length; i++)
        {
            ControlSurface cSurface = pitchControlSurfaces[i].ThisSurface;
            Vector3 forceGradient = cSurface.Gradient(fc, targetAoA >= 0f);
            Vector3 point = cSurface.Parent.quad.centerAero.Pos(fc);
            point -= fc.complex.rb.worldCenterOfMass;
            liftGradients[i] = new ForceAtPoint(forceGradient, point);
        }
        ResultingForce resultingGradient = new ResultingForce(liftGradients);

        return resultingGradient;
    }

    private static float InvLerp(float a, float b, float t)
    {
        if (a == b) return 0f;

        return (t - a) / (b - a);
    }
}
