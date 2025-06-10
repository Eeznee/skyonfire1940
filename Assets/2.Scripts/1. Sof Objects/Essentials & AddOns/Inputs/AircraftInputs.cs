using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public enum PitchCorrectionMode
{
    Raw,
    Clamped,
    Assisted,
    FullyAssisted
}

public class AircraftInputs
{
    private SofAircraft aircraft;

    public float brake;
    //public float differentialBrakes;
    public bool primaryFire;
    public bool secondaryFire;

    public AircraftAxes rawUncorrected;
    public AircraftAxes target;
    public AircraftAxes current;

    public AircraftAxes axesSpeed;

    private IPitchControlled[] elevators;
    private IRollControlled[] ailerons;
    private IYawControlled[] rudders;

    const float defaultPitchSpeed = 5f;
    const float defaultRollSpeed = 6f;
    const float defaultYawSpeed = 6f;

    const float pilotForceStick = 8000f;
    const float pilotForcePedals = 4000f;

    public AircraftInputs(SofAircraft _aircraft)
    {
        aircraft = _aircraft;

        float stickFactor = 1f / aircraft.StickTorqueFactor;
        axesSpeed = new AircraftAxes(defaultPitchSpeed * stickFactor, defaultRollSpeed * stickFactor, defaultYawSpeed);

        elevators = aircraft.GetComponentsInChildren<IPitchControlled>();
        ailerons = aircraft.GetComponentsInChildren<IRollControlled>();
        rudders = aircraft.GetComponentsInChildren<IYawControlled>();
    }
    public void FixedUpdate()
    {
        float ias = aircraft.data.ias.Get;

        current = SimulateControls(ias, current, target, Time.fixedDeltaTime);
    }

    public void SetTargetInput(AircraftAxes input, PitchCorrectionMode mode)
    {
       target = rawUncorrected = input;

        if (mode == PitchCorrectionMode.Raw || aircraft.data.grounded.Get) return;
        
        if(mode == PitchCorrectionMode.Clamped)
        {
            float maxPitch = PitchCorrection.MaxPitchAbs(aircraft, input.pitch);
            target.pitch = Mathf.Clamp(input.pitch, -maxPitch, maxPitch);
            return;
        }
        target.pitch = PitchCorrection.CorrectPitch(aircraft, input.pitch, mode == PitchCorrectionMode.FullyAssisted);
    }
    public AircraftAxes SimulateControls(float ias, AircraftAxes currentInputs, AircraftAxes targetInputs, float dt)
    {
        AircraftAxes leverage = MaximumInputs(ias, currentInputs);
        targetInputs.Clamp(leverage);
        if (Player.aircraft == aircraft && Player.role == SeatRole.Bombardier) targetInputs.yaw = aircraft.bombardierSeat.forcedYawInput;

        return AircraftAxes.MoveTowards(currentInputs, targetInputs, leverage * axesSpeed, dt);
    }
    public AircraftAxes MaximumInputs(float ias, AircraftAxes currentInputs)
    {
        if (aircraft.HydraulicControls) return new AircraftAxes(1f, 1f, 1f);

        float pitchResistance = 0f;
        float rollResistance = 0f;
        float yawResistance = 0f;

        foreach (IPitchControlled surface in elevators) if (surface != null) pitchResistance += surface.ControlsResistance(currentInputs);
        foreach (IRollControlled surface in ailerons) if (surface != null) rollResistance += surface.ControlsResistance(currentInputs);
        foreach (IYawControlled surface in rudders) if (surface != null) yawResistance += surface.ControlsResistance(currentInputs);

        AircraftAxes controlForces = new AircraftAxes(pitchResistance, rollResistance, yawResistance) * ias * ias;

        float stickForce = pilotForceStick * aircraft.StickTorqueFactor;
        float pedalForce = pilotForcePedals * aircraft.StickTorqueFactor; 

        AircraftAxes maximumInputs = new AircraftAxes(stickForce / controlForces.pitch, stickForce / controlForces.roll, pedalForce / controlForces.yaw);
        maximumInputs.Clamp();

        return maximumInputs;
    }
}
