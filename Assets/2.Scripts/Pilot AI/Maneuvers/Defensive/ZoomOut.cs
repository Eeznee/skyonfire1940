using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomOut : ActiveManeuver
{
    const float maxDistance = 2500f;
    private Vector3 targetDirection;

    public override void Initialize(AI.GeometricData data)
    {
        base.Initialize(data);

        float pitchAngle = Random.Range(30f, 40f);

        Vector3 flattenedDirection = Vector3.ProjectOnPlane(-data.dir, Vector3.up).normalized;
        targetDirection = Quaternion.AngleAxis(pitchAngle, Vector3.Cross(flattenedDirection, Vector3.up)) * flattenedDirection;
        targetDirection *= 500f;
    }
    public override string Label()
    {
        return "Zoom Out";
    }
    public override float PickFactor(AI.GeometricData data)
    {
        float energyDeltaFactor = Mathf.InverseLerp(2000f,5000f,data.energyDelta);
        return energyDeltaFactor;
    }
    public override void Execute(AI.GeometricData data)
    {
        if (done) return;

        base.Execute(data);

        aircraft.controls.SimpleTracking(targetDirection, 0f, 1f, true);

        if (data.distance > maxDistance || aircraft.data.ias.Get < aircraft.stats.MinTakeOffSpeedNoFlaps * 1.3f) done = true;
    }
}
