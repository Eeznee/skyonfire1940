using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disengage : ActiveManeuver
{
    const float disengageAngle = 10f;
    const float maxDistance = 1200f;
    const float maxSpeedFactor = 1.2f;
    private Vector3 targetDirection;

    public override void Initialize(AI.GeometricData data)
    {
        base.Initialize(data);
        targetDirection = Vector3.ProjectOnPlane(-target.transform.forward, Vector3.up).normalized;
        targetDirection.y = -Mathf.Tan(disengageAngle * Mathf.Deg2Rad);
        targetDirection *= 500f;
    }
    public override string Label()
    {
        return "Disengage";
    }
    public override float PickFactor(AI.GeometricData data)
    {
        float energyDeltaFactor = Mathf.InverseLerp(0f,-2000f,data.energyDelta);
        float speedFactor = Mathf.InverseLerp(data.aircraft.cruiseSpeed , data.aircraft.cruiseSpeed * 0.6f, data.aircraft.data.ias.Get);
        return energyDeltaFactor * speedFactor;
    }
    public override void Execute(AI.GeometricData data)
    {
        if (done) return;

        base.Execute(data);
        AircraftAxes axes;

        axes = PointTracking.TrackingInputs(transform.position + targetDirection, aircraft, 0f, 1f, true);
        aircraft.controls.SetTargetInput(axes, PitchCorrectionMode.FullyAssisted);
        if (data.distance > maxDistance || aircraft.data.ias.Get > aircraft.cruiseSpeed * maxSpeedFactor) done = true;
    }
}
