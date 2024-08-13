using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AircraftInputs
{
    private SofAircraft aircraft;

    public bool controlSent;
    public float brake;
    public bool primaryFire;
    public bool secondaryFire;

    public AircraftAxes rawUncorrected;
    public AircraftAxes target;
    public AircraftAxes current;

    public AircraftInputs(SofAircraft _aircraft)
    {
        aircraft = _aircraft;

        controlSent = false;
        brake = 0f;
        primaryFire = false;
        secondaryFire = false;


        rawUncorrected = target = current = AircraftAxes.zero;
    }
    public void FixedUpdate()
    {
        current.MoveTowards(controlSent ? target : AircraftAxes.zero, aircraft.axesSpeed, Time.fixedDeltaTime);
        controlSent = false;
    }
    public void SendAxes(AircraftAxes input, bool correctedPitch, bool instant)
    {
        if (controlSent) return;

        target = rawUncorrected = input;
        if (correctedPitch && aircraft.data.tas.Get > 30f) target.CorrectPitch(aircraft, input.pitch, Time.fixedDeltaTime);

        if (instant) current = target;
        controlSent = true;
    }
}
