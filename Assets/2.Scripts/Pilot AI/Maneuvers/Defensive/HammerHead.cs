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

        AircraftAxes axes;

        if (phase == 0) //Climb Up
        {
            axes = PointTracking.TrackingInputs(transform.position + Vector3.up * 500f, aircraft, 0f, 0f, true);
            aircraft.controls.SetTargetInput(axes, PitchCorrectionMode.FullyAssisted);
            if (aircraft.data.ias.Get < 150 / 3.6f) { phase++; way = Mathf.Sign(aircraft.data.bankAngle.Get); }
        }
        else if (phase == 1) //Hammerhead
        {
            axes = AircraftAxes.zero;
            axes.yaw = -way * Mathf.Sign(transform.forward.y);
            aircraft.engines.SetThrottleAllEngines(0f, false);
            aircraft.controls.SetTargetInput(axes, PitchCorrectionMode.FullyAssisted);
            if (transform.forward.y < 0f) done = true ;
        }
    }
}
