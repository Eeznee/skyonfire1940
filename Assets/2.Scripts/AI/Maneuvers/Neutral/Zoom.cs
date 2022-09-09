using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zoom : ActiveManeuver
{
    const float zoomAngle = 30f;
    private Vector3 targetDirection;
    const float safeDistance = 1000f;
    public override void Initialize(AI.GeometricData data)
    {
        base.Initialize(data); 
        targetDirection = Vector3.ProjectOnPlane(transform.forward,Vector3.up).normalized;
        targetDirection.y = Mathf.Tan(zoomAngle * Mathf.Deg2Rad);
        targetDirection *= 500f;
    }
    public override string Label()
    {
        return "Zoom";
    }
    public override float PickFactor(AI.GeometricData data)
    {
        float speedDeltaFactor = Mathf.InverseLerp(5f,30f,data.aircraft.data.gsp - data.target.data.gsp);
        return speedDeltaFactor;
    }
    public override void Execute(AI.GeometricData data)
    {
        if (done) return;

        base.Execute(data);
        Vector3 input;

        input = AircraftControl.TrackingInputs(transform.position + targetDirection, aircraft, 0f, 1f, true);
        aircraft.SetControls(input, true, false);
        if (aircraft.data.ias < aircraft.cruiseSpeed * 0.7f ||data.distance > safeDistance) done = true;
    }
}
