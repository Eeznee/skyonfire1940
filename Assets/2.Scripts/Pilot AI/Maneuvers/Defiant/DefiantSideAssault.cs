using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefiantSideAssault : ActiveManeuver
{
    const float minDistance = 200f;
    const float maxDistance = 400f;
    const float maxOffsetCoeff = 0.15f;
    private PID thrPID = new PID(new Vector3(0.01f, 0f, 0.2f));

    private float distance;
    private float altitudeOffset;

    public override void Initialize(AI.GeometricData data)
    {
        base.Initialize(data);
        distance = Random.Range(minDistance, maxDistance);
        altitudeOffset = distance * Random.Range(-maxOffsetCoeff, maxOffsetCoeff);
    }
    public override string Label()
    {
        return "Strike From Side";
    }
    public override float PickFactor(AI.GeometricData data)
    {
        float pairAndTarget = !data.aircraft.CanPairUp()  && data.target.card.bomber ? 1f : 0f;
        float energyAdvantage = Mathf.InverseLerp(100f * -Physics.gravity.y, 400f * -Physics.gravity.y, data.energyDelta);
        return pairAndTarget * energyAdvantage;
    }
    public override void Execute(AI.GeometricData data)
    {
        base.Execute();

        //Direction
        Vector3 targetPos = target.transform.position + target.transform.right * distance + Vector3.up * altitudeOffset;
        aircraft.controls.SimpleTrackingPos(targetPos + target.transform.forward * 600f, 0f, 1f, true);

        //Throttle
        float dis = transform.InverseTransformDirection(targetPos - transform.position).z;
        float thr = 1f + thrPID.UpdateUnclamped(dis, Time.fixedDeltaTime);
        aircraft.engines.SetThrottleAllEngines(thr, false);
    }
}
