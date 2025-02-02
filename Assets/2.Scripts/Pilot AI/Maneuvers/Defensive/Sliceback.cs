using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliceback : ActiveManeuver
{
    private float angle;
    private Vector3 initialDirection;
    private Vector3 downDirection;
    private Vector3 initialDirectionNormalized;

    const float altitudeSafety = 200f;
    const float minBank = 135f;
    public override void Initialize(AI.GeometricData data)
    {
        base.Initialize(data);

        float maxAngle = 180f;
        float altitudeLoss180 = aircraft.stats.TurningRadius * 2f;
        float lowestAltitude = aircraft.data.relativeAltitude.Get - altitudeLoss180;
        if (lowestAltitude < altitudeSafety) maxAngle = Mathf.Rad2Deg * Mathf.Acos(-(aircraft.data.relativeAltitude.Get - altitudeSafety) / altitudeLoss180);
        angle = Random.Range(minBank, maxAngle) * Mathf.Sign(aircraft.data.bankAngle.Get);

        initialDirection = Vector3.ProjectOnPlane(transform.forward, Vector3.up) * 500f;
        initialDirectionNormalized = initialDirection.normalized;
        downDirection = Quaternion.AngleAxis(-(180f-angle), initialDirection) * Vector3.down * 500f;
    }
    public override bool MaxPitch()
    {
        return true;
    }
    public override string Label()
    {
        return "Sliceback";
    }

    public override float PickFactor(AI.GeometricData data)
    {
        float closureFactor = Mathf.InverseLerp(15f, -30f, data.closure);
        float disFactor = Mathf.InverseLerp(-200f, 500f, data.distance);
        float minAltitudeLoss = Mathf.Cos(minBank * Mathf.Deg2Rad) * data.aircraft.stats.TurningRadius * 2f;
        float altitudeFactor = data.aircraft.data.relativeAltitude.Get + minAltitudeLoss > altitudeSafety ? 1f : 0f;
        return closureFactor * disFactor * altitudeFactor;
    }
    public override void Execute(AI.GeometricData data)
    {
        if (done) return;

        base.Execute(data);
        switch (phase)
        {
            case 0: //Phase 1 : invert bank
                PointTracking.Tracking(transform.position + transform.forward * 500f, aircraft, angle, 1f, false);
                if (Mathf.Abs(aircraft.data.bankAngle.Get - angle) < 5f) phase++;
                break;
            case 1: //Phase 2 : first half of the sliceback
                PointTracking.Tracking(transform.position + downDirection, aircraft, angle, 0f, false);
                if (aircraft.transform.forward.y < -0.5f) phase++;
                break;
            case 2: //Phase 3 : Direction reversed
                PointTracking.Tracking(transform.position - initialDirection, aircraft, 0f, 1f, false);
                if (Vector3.Dot(transform.forward, -initialDirectionNormalized) > 0.9f) done = true;
                break;
        }
    }
}
