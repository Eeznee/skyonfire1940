using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public static partial class NewPointTracking
{
    const float rollYawAlpha = 30f;
    public static AircraftAxes FindOptimalControls(Vector3 targetDirection, SofAircraft aircraft, AircraftAxes forcedAxes)
    {
        targetDirection.Normalize();
        float offAngle = Vector3.Angle(aircraft.rb.velocity, targetDirection);
        float offAngleFactor = offAngle / rollYawAlpha;
        offAngleFactor *= offAngleFactor;
        offAngleFactor = Mathf.Clamp01(0.01f + offAngleFactor);

        float maxPitch = PitchCorrection.MaxPitchAbs(aircraft, Mathf.Sign(aircraft.inputs.current.pitch));
        if (!float.IsNaN(forcedAxes.pitch)) forcedAxes.pitch = Mathf.Clamp(forcedAxes.pitch, -maxPitch, maxPitch);

        AircraftAxes controlsFound = aircraft.inputs.target;
        ApplyForcedAxis(ref controlsFound, forcedAxes);

        Vector2 multipliers = new(8f, 8f);
        Vector2 abstractControls = new(controlsFound.roll / offAngleFactor, controlsFound.pitch);

        Vector3 previousLocalTarget = aircraft.tr.InverseTransformDirection(targetDirection);

        for (int i = 0; i < 8; i++)
        {
            abstractControls.ClampUnitSquare();
            controlsFound = GetControls(abstractControls, maxPitch, offAngleFactor, aircraft);
            ApplyForcedAxis(ref controlsFound, forcedAxes);

            controlsFound.Clamp(maxPitch);
            FlightConditions simConditions = FullSimulation(aircraft, controlsFound, 3, Time.fixedDeltaTime * 4f * Mathf.Sqrt(aircraft.StickTorqueFactor));

            Vector3 localTarget = Quaternion.Inverse(simConditions.rotation) * targetDirection;

            bool verticalOvershoots = Mathf.Sign(previousLocalTarget.y * localTarget.y) == -1f;
            bool horizontalOvershoots = Mathf.Sign(previousLocalTarget.x * localTarget.x) == -1f;
            multipliers.y *= verticalOvershoots ? 0.5f : 2f;
            multipliers.x *= horizontalOvershoots ? 0.5f : 2f;

            //OPTIMIZATION 1 : STOP IF VALUES ARE MAXED
            bool noOvershoots = !verticalOvershoots && !horizontalOvershoots;
            bool maxValueTried = abstractControls.ManhattanMagnitude() >= 1.99f;
            if (noOvershoots && maxValueTried) return InterpolateForGrounded(controlsFound, aircraft, localTarget, forcedAxes);

            abstractControls.y += multipliers.y * localTarget.y;
            abstractControls.x += multipliers.x * localTarget.x;

            previousLocalTarget = localTarget;
        }

        return InterpolateForGrounded(controlsFound, aircraft, previousLocalTarget, forcedAxes);
    }
    private static AircraftAxes InterpolateForGrounded(AircraftAxes controlsFound, SofAircraft aircraft, Vector3 localTarget, AircraftAxes forcedAxes)
    {
        float vertical = Mathf.Clamp(localTarget.y * 5f, -1f, 1f);
        float horizontal = Mathf.Clamp(localTarget.x * 5f, -1f, 1f);

        float lerpFactor = aircraft.data.tas.Get / 20f;

        AircraftAxes final = new AircraftAxes();

        final.pitch = Mathf.Lerp(vertical, controlsFound.pitch, lerpFactor);
        final.roll = Mathf.Lerp(horizontal, controlsFound.roll, lerpFactor);
        final.yaw = Mathf.Lerp(-horizontal, controlsFound.yaw, lerpFactor);

        ApplyForcedAxis(ref final, forcedAxes);

        return final;
    }
    private static void ApplyForcedAxis(ref AircraftAxes applyTo, AircraftAxes forcedAxes)
    {
        if (!float.IsNaN(forcedAxes.pitch)) applyTo.pitch = forcedAxes.pitch;
        if (!float.IsNaN(forcedAxes.roll)) applyTo.roll = forcedAxes.roll;
        if (!float.IsNaN(forcedAxes.yaw)) applyTo.yaw = forcedAxes.yaw;
    }
    private static AircraftAxes GetControls(Vector2 abstractControls, float maxPitch, float offAngleFactor, SofAircraft aircraft)
    {
        AircraftAxes controls;
        controls.pitch = abstractControls.y * maxPitch;
        controls.yaw = -abstractControls.x * (1f - offAngleFactor);

        float aggressiveRoll = abstractControls.x;
        float levelRoll = aircraft.tr.right.y * 0.2f;
        controls.roll = Mathf.Lerp(levelRoll, aggressiveRoll, offAngleFactor);

        return controls;
    }
    public static FlightConditions FullSimulation(SofAircraft aircraft, AircraftAxes controlsToTry, int steps, float dt)
    {
        FlightConditions flightConditions = new FlightConditions(aircraft, true);

        for (int simStep = 0; simStep < steps; simStep++)
        {
            flightConditions.SimulateControls(controlsToTry, dt);
            flightConditions.ApplyForces(dt);
        }

        return flightConditions;
    }
}
