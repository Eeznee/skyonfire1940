using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingScissors : ActiveManeuver
{
    private float angle;
    private Vector3 initialDirection;

    const float minBank = 60f;
    const float maxBank = 110f;
    const float maxDuration = 40f;
    public override void Initialize(AI.GeometricData data)
    {
        base.Initialize(data);
        Vector3 targetRelativePos = transform.InverseTransformPoint(data.target.transform.position);
        angle = Random.Range(minBank, maxBank) * Mathf.Sign(-targetRelativePos.x);
        initialDirection = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
    }
    public override string Label()
    {
        return "Shake off";
    }
    public override float PickFactor(AI.GeometricData data)
    {
        float energyFactor = Mathf.InverseLerp(70f * -Physics.gravity.y, -200f * -Physics.gravity.y, data.energyDelta);
        float distanceFactor = Mathf.InverseLerp(700f, 50f, data.distance);
        return energyFactor * distanceFactor;
    }
    public bool Executable()
    {
        return true;
    }
    public override void Execute(AI.GeometricData data)
    {
        if (done) return;
        base.Execute(data);

        AircraftAxes axes;

        if (phase == 0) //Break Turn Inside Bandit
        {
            axes = PointTracking.TrackingInputs(transform.position + transform.forward * 500f, aircraft, angle, 1f, true);
            if (Mathf.Abs(aircraft.data.bankAngle.Get - angle) < 10f) axes.pitch = 1f;
            aircraft.controls.SetTargetInput(axes, PitchCorrectionMode.FullyAssisted);
            if (Vector3.Dot(initialDirection, transform.forward) < 0.3f) phase++;
        }
        else if (phase == 1) //Climb Hard
        {
            axes = PointTracking.TrackingInputs(target.transform.position + Vector3.up * 500f, aircraft, 0f, 0f, true);
            aircraft.controls.SetTargetInput(axes, PitchCorrectionMode.FullyAssisted);
            if (transform.forward.y > 0.8f) phase++;
        }
        else //Rolling scissors
        {
            PointTracking.Tracking(data.target.transform.position, data.aircraft, 0f, 0f, true);
            if (data.state == AI.DogfightState.Offensive) done = true;
        }
        if (timeStart + maxDuration < Time.time) done = true;
    }
}
