using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefiantBellyAssault : ActiveManeuver
{
    const float minDistance = 100f;
    const float maxDistance = 300f;
    const float maxOffsetCoeff = 0.5f;
    private PID thrPID = new PID(new Vector3(0.01f, 0f, 0.2f));

    private float distance;
    private float sideOffset;

    public override void Initialize(AI.GeometricData data)
    {
        base.Initialize(data);
        distance = Random.Range(minDistance, maxDistance);
        distance = Mathf.Min(data.target.data.relativeAltitude - 200f, distance);
        sideOffset = distance * Random.Range(-maxOffsetCoeff, maxOffsetCoeff);
    }
    public override string Label()
    {
        return "Strike From Below";
    }
    public override float PickFactor(AI.GeometricData data)
    {
        float pairAndTarget = !data.aircraft.CanPairUp() && data.target.card.bomber ? 1f : 0f;
        float targetAltitude = Mathf.InverseLerp(400f, 700f, data.target.data.relativeAltitude);
        return pairAndTarget * targetAltitude;
    }
    public override void Execute(AI.GeometricData data)
    {
        base.Execute();

        //Direction
        Vector3 targetPos = target.transform.position + Vector3.down * distance + target.transform.right * sideOffset;
        Vector3 axis = AircraftControl.TrackingInputs(targetPos + target.transform.forward * 600f, aircraft, 0f, 1f, true);
        aircraft.SetControls(axis, true, false);

        //Throttle
        float dis = transform.InverseTransformDirection(targetPos - transform.position).z;
        float thr = 1f + thrPID.UpdateUnclamped(dis, Time.fixedDeltaTime);
        aircraft.SetThrottle(Mathf.Clamp01(thr));
        aircraft.boost = thr > 1.05f;
    }
}
