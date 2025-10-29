using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public enum PitchCorrectionMode
{
    FullyAssisted,
    Assisted,
    Raw
}

public partial class AircraftInputs
{
    private SofAircraft aircraft;

    public float brake;
    public bool primaryFire;
    public bool secondaryFire;
    private bool useProgressiveForce;

    public AircraftAxes rawUncorrected;
    public AircraftAxes target;
    public AircraftAxes current;

    public AircraftAxes axesSpeed;


    private float maxPitch;
    private float maxPitchTimer;
    private const float maxPitchCycle = 0.1f;

    public float MaxPitch => maxPitch;

    private IPitchControlled[] elevators;
    private IRollControlled[] ailerons;
    private IYawControlled[] rudders;

    const float defaultPitchSpeed = 4f;
    const float defaultRollSpeed = 6f;
    const float defaultYawSpeed = 4f;



    public AircraftInputs(SofAircraft _aircraft)
    {
        aircraft = _aircraft;

        float stickFactor = 1f / aircraft.StickTorqueFactor;
        axesSpeed = new AircraftAxes(defaultPitchSpeed * stickFactor, defaultRollSpeed * stickFactor, defaultYawSpeed);

        elevators = aircraft.GetComponentsInChildren<IPitchControlled>();
        ailerons = aircraft.GetComponentsInChildren<IRollControlled>();
        rudders = aircraft.GetComponentsInChildren<IYawControlled>();

        abstractControls = Vector2.zero;
        multipliers = Vector2.one;
        previousLocalTarget = previousTarget = Vector3.forward;
    }
    public void FixedUpdate()
    {
        if (Player.aircraft == aircraft && Player.role == SeatRole.Bombardier)
            target.yaw = aircraft.bombardierSeat.forcedYawInput;

        current = SimulateControls(useProgressiveForce, Time.fixedDeltaTime);
        UpdateMaxPitch();
    }


    public void SetTargetInput(AircraftAxes input, bool progressiveForce, PitchCorrectionMode mode)
    {
        input.Clamp();

        useProgressiveForce = progressiveForce;
        target = rawUncorrected = input;

        if (mode == PitchCorrectionMode.Raw || aircraft.data.grounded.Get) return;
        target.pitch = CorrectPitch(input.pitch, mode == PitchCorrectionMode.FullyAssisted, false);
    }
    public AircraftAxes SimulateControls(bool progressiveForce, float dt)
    {
        AircraftAxes axes = current;

        axes.pitch = TargetControlWithForces(elevators.CombinedForces(), current.pitch, target.pitch, 500f * aircraft.PitchBoost, progressiveForce, dt);
        axes.roll = TargetControlWithForces(ailerons.CombinedForces(), current.roll, target.roll, 400f * aircraft.RollBoost, progressiveForce, dt);
        axes.yaw = TargetControlWithForces(rudders.CombinedForces(), current.yaw, target.yaw, 200f, progressiveForce, dt);

        return axes;
    }
    public AircraftAxes SimulateControls(FlightConditions flightConditions, AircraftAxes targetInputs,bool progressiveForce, float dt)
    {
        AircraftAxes axes = flightConditions.axes;

        axes.pitch = TargetControlWithForces(elevators.CombinedForces(flightConditions), flightConditions.axes.pitch, targetInputs.pitch, 500f * aircraft.PitchBoost, progressiveForce, dt);
        axes.roll = TargetControlWithForces(ailerons.CombinedForces(flightConditions), flightConditions.axes.roll, targetInputs.roll, 400f * aircraft.RollBoost, progressiveForce, dt);
        axes.yaw = TargetControlWithForces(rudders.CombinedForces(flightConditions), flightConditions.axes.yaw, targetInputs.yaw, 200f, progressiveForce, dt);

        return axes;
    }

    public float TargetControlWithForces(float combinedForce, float currentInput, float targetInput, float maxPilotForce, bool progressiveForce, float dt)
    {
        float pilotForce = maxPilotForce;
        if (progressiveForce) pilotForce *= Mathf.Lerp(0.1f, 1f, Mathf.Max(Mathf.Abs(targetInput), Mathf.Abs(currentInput)));

        float forceDelta = pilotForce - Mathf.Abs(combinedForce);
        bool forceAndDesiredSameDirection = Mathf.Sign(targetInput - currentInput) == Mathf.Sign(combinedForce);

        if (forceDelta > 0f || forceAndDesiredSameDirection)
        {
            float minSpeedMultiplier = forceAndDesiredSameDirection ? 0.5f : 0f;
            float moveSpeed = defaultPitchSpeed * Mathf.Clamp(forceDelta / maxPilotForce, minSpeedMultiplier, 1f);
            currentInput = Mathf.MoveTowards(currentInput, targetInput, moveSpeed * dt);
        }
        else
        {
            float forcedPitchSpeed = defaultPitchSpeed * Mathf.Sign(combinedForce) * Mathf.Clamp01(Mathf.Abs(forceDelta) / maxPilotForce);
            currentInput += forcedPitchSpeed * dt;
        }
        return Mathf.Clamp(currentInput, -1f, 1f);
    }

    const float pilotForceStick = 8000f;
    const float pilotForcePedals = 4000f;
    public AircraftAxes MaximumInputsOld(float ias, AircraftAxes currentInputs)
    {
        float pitchResistance = 0f;
        float rollResistance = 0f;
        float yawResistance = 0f;

        foreach (IPitchControlled surface in elevators) if (surface != null) pitchResistance += surface.ControlsResistance(currentInputs);
        foreach (IRollControlled surface in ailerons) if (surface != null) rollResistance += surface.ControlsResistance(currentInputs);
        foreach (IYawControlled surface in rudders) if (surface != null) yawResistance += surface.ControlsResistance(currentInputs);

        AircraftAxes controlForces = new AircraftAxes(pitchResistance, rollResistance, yawResistance) * ias * ias;

        float stickForce = pilotForceStick * aircraft.StickTorqueFactor * aircraft.ControlsForceBoost;
        float pedalForce = pilotForcePedals * aircraft.ControlsForceBoost;

        AircraftAxes maximumInputs = new AircraftAxes(stickForce / controlForces.pitch, stickForce / controlForces.roll, pedalForce / controlForces.yaw);
        maximumInputs.Clamp();

        return maximumInputs;
    }

    private void UpdateMaxPitch()
    {
        AircraftAxes previous = current;

        if (Mathf.Abs(current.pitch) > 0.5f)
        {
            maxPitchTimer += Time.fixedDeltaTime;

            if (Mathf.Abs(previous.pitch) < 0.5f || maxPitchTimer > maxPitchCycle)
            {
                maxPitchTimer = 0f;
                maxPitch = MaxPitchAbs(current.pitch);

            }
        }
        else maxPitch = 0.75f;
    }
    const float altitudeLimitSea = 30f;
    const float altitudeLimitLand = 50f;

    private float cumulativePitch = 0f;
    public void MaintainBankAndPitch(float pitch, float bank)
    {
        float currentBank = aircraft.data.bankAngle.Get;
        float currentPitch = aircraft.data.pitchAngle.Get;

        float pitchDelta = pitch - currentPitch;
        float bankDelta = bank - currentBank;

        cumulativePitch += pitchDelta * 0.003f * Time.fixedDeltaTime;
        cumulativePitch = Mathf.Clamp(cumulativePitch, -0.15f, 0.15f);

        AircraftAxes final = new AircraftAxes(cumulativePitch + pitchDelta * 0.02f, -bankDelta * 0.02f, 0f);
        final.Clamp(0.3f, 0.3f);

        SetTargetInput(final, false, PitchCorrectionMode.Raw);
    }
    public void MaintainBankAndPitchAvoidCrash(float pitch, float bank)
    {
        if (aircraft.data.altitude.Get > 1000f)
        {
            MaintainBankAndPitch(pitch, bank);
            return;
        }

        bool flyingOverSea = Mathf.Abs(aircraft.data.relativeAltitude.Get - aircraft.data.altitude.Get) < 1f;
        float minRelativeAltitude = aircraft.data.relativeAltitude.Get;

        float minAltitudeAllowed = flyingOverSea ? altitudeLimitSea : altitudeLimitLand; //min altitude tolerance is lower when flying above sea
        float timeToAvoidCrash = 2f * 90f / aircraft.stats.MaxTurnRate + 90f / aircraft.stats.RollRateCurrentSpeed();
        float altitudeWhenDelayIsReached = minRelativeAltitude + aircraft.data.vsp.Get * timeToAvoidCrash;

        if (altitudeWhenDelayIsReached < minAltitudeAllowed)
        {
            float pitchUpAltitude = minAltitudeAllowed - Mathf.Max(minRelativeAltitude, 50f);
            float factor = Mathf.InverseLerp(minAltitudeAllowed, pitchUpAltitude, altitudeWhenDelayIsReached);
            pitch = Mathf.Lerp(pitch, 10f, factor);
        }
        MaintainBankAndPitch(pitch, bank);
    }
    public float CorrectPitch(float targetPitch, bool correctZeroAoA, bool brokenForTracking)
    {
        float targetAoA = Mathf.Abs(targetPitch) * aircraft.stats.MaxStableAoA(targetPitch);

        if (correctZeroAoA && !aircraft.data.grounded.Get)
        {
            float correctedAoA = CorrectZeroAoAForZeroTurnRate(targetAoA, targetPitch);
            if (aircraft.TimeSinceLastLanding > 30f)
                targetAoA = correctedAoA;
            else
                targetAoA = Mathf.Lerp(targetAoA, correctedAoA, aircraft.TimeSinceLastLanding / 30f);
        }

        FlightConditions fc = new FlightConditions(aircraft, targetAoA);


        ResultingForce resultingForce = aircraft.GetComponent<ForcesCompiler>().Compute(fc);
        float pitchTorque = Vector3.Dot(resultingForce.torque, aircraft.tr.right);

        ResultingForce resultingGradient = fc.ElevatorsGradient(targetAoA, brokenForTracking);
        float gradientPitchTorque = Vector3.Dot(resultingGradient.torque, aircraft.tr.right);
        if (gradientPitchTorque == 0f) return aircraft.controls.current.pitch;

        //Using elevator potential force and simulated forces, find the elevator adjustment needed to maintain target aoa
        float elevatorAdjustment = pitchTorque / gradientPitchTorque;
        float correctedPitch = Mathf.Clamp(aircraft.controls.current.pitch + elevatorAdjustment, -1f, 1f);

        return correctedPitch;
    }

    public float MaxPitchAbs(float pitchSign)
    {
        float maxPitch = CorrectPitch(Mathv.SignNoZero(pitchSign), false, false);
        return Mathf.Clamp01(Mathf.Abs(maxPitch));
    }

    //Okay, this is a terrible solution, but an issue of conversion from rad to degrees was wrong and, when I fixed it, it broke the mouse tracking. So there is an option to use the broken conversion
    public float MaxPitchBrokenForTracking(float pitchSign)
    {
        float maxPitch = CorrectPitch(Mathv.SignNoZero(pitchSign), false, true);
        return Mathf.Clamp01(Mathf.Abs(maxPitch));
    }

    private float CorrectZeroAoAForZeroTurnRate(float targetAoA, float targetPitch)
    {
        float offsetAoA = targetAoA - Mathf.Sign(targetAoA);

        FlightConditions defaultFc = new FlightConditions(aircraft, targetAoA);
        FlightConditions offsetFc = new FlightConditions(aircraft, offsetAoA);

        ResultingForce target = aircraft.GetComponent<ForcesCompiler>().Compute(defaultFc);
        ResultingForce offset = aircraft.GetComponent<ForcesCompiler>().Compute(offsetFc);

        float targetG = Vector3.Dot(target.force, aircraft.tr.up);
        float offsetG = Vector3.Dot(offset.force, aircraft.tr.up);
        float weight = -Physics.gravity.y * aircraft.rb.mass * Vector3.Dot(aircraft.tr.up, Vector3.up);

        float lerpT = Mathv.InverseLerpUnclamped(offsetG, targetG, weight);
        float zeroAoA = Mathf.LerpUnclamped(offsetAoA, targetAoA, lerpT);

        return Mathf.LerpUnclamped(zeroAoA, aircraft.stats.MaxStableAoA(targetPitch), Mathf.Abs(targetPitch));
    }
}
