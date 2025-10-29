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

        if (phase == 0) //Break Turn Inside Bandit
        {
            aircraft.controls.SimpleTracking(transform.forward, angle, 1f, true);
            if (Vector3.Dot(initialDirection, transform.forward) < 0.3f) phase++;
        }
        else if (phase == 1) //Climb Hard
        {
            aircraft.controls.SimpleTracking(Vector3.up, 0f, 0f, true);
            if (transform.forward.y > 0.8f) phase++;
        }
        else //Rolling scissors
        {
            aircraft.controls.SimpleTracking(data.target.transform.position - transform.position, 0f, 0f, true);
            if (data.state == AI.DogfightState.Offensive) done = true;
        }
        if (timeStart + maxDuration < Time.time) done = true;
    }
}
