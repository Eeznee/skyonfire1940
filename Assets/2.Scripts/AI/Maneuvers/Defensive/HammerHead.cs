using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerHead : ActiveManeuver
{
    private float way;
    public override float PickFactor(AI.GeometricData data)
    {
        float energyDeltaFactor = Mathf.InverseLerp(250f * -Physics.gravity.y,600f * -Physics.gravity.y, data.energyDelta);
        return energyDeltaFactor;
    }
    public override string Label()
    {
        return "Hammer Head";
    }
    public override void Execute(AI.GeometricData data)
    {
        if (done) return;
        base.Execute(data);

        Vector3 input;

        if (phase == 0) //Climb Up
        {
            input = AircraftControl.TrackingInputs(transform.position + Vector3.up * 500f, aircraft, 0f, 0f, true);
            aircraft.SetControls(input, true, false);
            if (aircraft.data.ias < 150 / 3.6f) { phase++; way = Mathf.Sign(aircraft.data.bankAngle); }
        }
        else if (phase == 1) //Hammerhead
        {
            input = Vector3.zero;
            input.y = -way * Mathf.Sign(transform.forward.y);
            aircraft.SetThrottle(0f);
            aircraft.SetControls(input, true, false);
            if (transform.forward.y < 0f) done = true ;
        }
    }
}
