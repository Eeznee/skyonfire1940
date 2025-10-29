using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerHead : ActiveManeuver
{
    private float way;
    const float maxChance = 0.6f;
    public override float PickFactor(AI.GeometricData data)
    {
        float energyDeltaFactor = Mathf.InverseLerp(2500f,6000f, data.energyDelta) * maxChance;
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

        if (phase == 0) //Climb Up
        {
            aircraft.controls.SimpleTracking(Vector3.up, 0f, 0f, true);
            if (aircraft.data.ias.Get < 150 / 3.6f) { phase++; way = Mathf.Sign(aircraft.data.bankAngle.Get); }
        }
        else if (phase == 1) //Hammerhead
        {
            AircraftAxes axes = AircraftAxes.zero;
            axes.yaw = -way * Mathf.Sign(transform.forward.y);
            aircraft.engines.SetThrottleAllEngines(0f, false);
            aircraft.controls.SetTargetInput(axes, false, PitchCorrectionMode.FullyAssisted);
            if (transform.forward.y < 0f) done = true ;
        }
    }
}
