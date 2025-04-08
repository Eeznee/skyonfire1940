using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public static partial class NewPointTracking
{
    const float rollYawAlpha = 30f;
    private const float maxMultiplierValue = 64f;
    private const int simSteps = 3;
    private const int iterationsSteps = 4;

    private static float SimDt = Time.fixedDeltaTime * 4f;
    private static float SimTime => simSteps * SimDt;
    public static AircraftAxes FindOptimalControls(Vector3 targetDirection, SofAircraft aircraft, AircraftAxes forcedAxes)
    {
        targetDirection.Normalize();
        float offAngle = Vector3.Angle(aircraft.rb.velocity, targetDirection);
        float offAngleFactor = Mathf.Clamp01(offAngle / rollYawAlpha);

        float maxPitch = PitchCorrection.MaxPitchAbs(aircraft, Mathf.Sign(aircraft.inputs.current.pitch));
        if (!float.IsNaN(forcedAxes.pitch)) forcedAxes.pitch = Mathf.Clamp(forcedAxes.pitch, -maxPitch, maxPitch);

        AircraftAxes controlsFound = AircraftAxes.zero;
        Vector3 previousLocalTarget = aircraft.tr.InverseTransformDirection(targetDirection);

        for (int i = 0; i < iterationsSteps; i++)
        {
            controlsFound = GetControls(aircraft.ptAbstractControls, maxPitch, offAngleFactor, aircraft);
            ApplyForcedAxis(ref controlsFound, forcedAxes);
            controlsFound.Clamp(maxPitch);

            FlightConditions simConditions = FullSimulation(aircraft, controlsFound, simSteps, SimDt);

            Vector3 localTarget = Quaternion.Inverse(simConditions.rotation) * targetDirection;

            bool verticalOvershoots = Mathf.Sign(previousLocalTarget.y * localTarget.y) == -1f;
            bool horizontalOvershoots = Mathf.Sign(previousLocalTarget.x * localTarget.x) == -1f;
            aircraft.ptMultipliers.y = Mathf.Clamp(aircraft.ptMultipliers.y * (verticalOvershoots ? 0.5f : 2f), 0f, maxMultiplierValue);
            aircraft.ptMultipliers.x = Mathf.Clamp(aircraft.ptMultipliers.x * (horizontalOvershoots ? 0.5f : 2f), 0f, maxMultiplierValue);

            aircraft.ptAbstractControls.y += aircraft.ptMultipliers.y * localTarget.y;
            aircraft.ptAbstractControls.x += aircraft.ptMultipliers.x * localTarget.x;
            aircraft.ptAbstractControls.ClampUnitSquare();

            previousLocalTarget = localTarget;
        }

        return InterpolateForGrounded(controlsFound, aircraft, previousLocalTarget, forcedAxes);
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
    private static AircraftAxes InterpolateForGrounded(AircraftAxes controlsFound, SofAircraft aircraft, Vector3 localTarget, AircraftAxes forcedAxes)
    {
        AircraftAxes final = new AircraftAxes();

        float vertical = Mathf.Clamp(localTarget.y * 5f, -1f, 1f);
        float horizontal = Mathf.Clamp(localTarget.x * 5f, -1f, 1f);

        if (aircraft.data.tas.Get < 0.2f)
        {
            final = new AircraftAxes(vertical, horizontal, -horizontal);
            ApplyForcedAxis(ref final, forcedAxes);
            return final;
        }

        float lerpFactor = aircraft.data.tas.Get / 20f;

        final.pitch = Mathf.Lerp(vertical, controlsFound.pitch, lerpFactor);
        final.roll = Mathf.Lerp(horizontal, controlsFound.roll, lerpFactor);
        final.yaw = Mathf.Lerp(0f, controlsFound.yaw, lerpFactor);

        ApplyForcedAxis(ref final, forcedAxes);

        return final;
    }
    private static void ApplyForcedAxis(ref AircraftAxes applyTo, AircraftAxes forcedAxes)
    {
        if (!float.IsNaN(forcedAxes.pitch)) applyTo.pitch = forcedAxes.pitch;
        if (!float.IsNaN(forcedAxes.roll)) applyTo.roll = forcedAxes.roll;
        if (!float.IsNaN(forcedAxes.yaw)) applyTo.yaw = forcedAxes.yaw;
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

    public static bool ReachableControls(SofAircraft aircraft, AircraftAxes toReach, float timeDelta)
    {
        AircraftAxes end = aircraft.inputs.SimulateControls(aircraft.data.ias.Get, aircraft.inputs.current, toReach, timeDelta);

        if (end.pitch - toReach.pitch == 0f) return true;
        if (end.roll - toReach.roll == 0f) return true;
        if (end.yaw - toReach.yaw == 0f) return true;

        return false;
    }
}
