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

        Vector3 input;

        if (phase == 0) //Break Turn Inside Bandit
        {
            input = AircraftControl.TrackingInputs(transform.position + transform.forward * 500f, aircraft, angle, 1f, true);
            if (Mathf.Abs(aircraft.data.bankAngle - angle) < 10f) input.x = 1f;
            aircraft.SetControls(input, true, false);
            if (Vector3.Dot(initialDirection, transform.forward) < 0.3f) phase++;
        }
        else if (phase == 1) //Climb Hard
        {
            input = AircraftControl.TrackingInputs(target.transform.position + Vector3.up * 500f, aircraft, 0f, 0f, true);
            aircraft.SetControls(input, true, false);
            if (transform.forward.y > 0.8f) phase++;
        }
        else //Rolling scissors
        {
            AircraftControl.Tracking(data.target.transform.position, data.aircraft, 0f, 0f, true);
            if (data.state == AI.DogfightState.Offensive) done = true;
        }
        if (timeStart + maxDuration < Time.time) done = true;
    }
}
