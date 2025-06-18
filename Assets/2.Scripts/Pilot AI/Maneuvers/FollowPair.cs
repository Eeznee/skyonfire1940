using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPair : Maneuver
{
    const float pairDistance = 80f;
    private PID thrPID = new PID(new Vector3(0.01f, 0f, 0.2f));

    public override void Execute(SofAircraft a)
    {
        aircraft = a;
        transform = a.transform;
        base.Execute();

        //Create teams of 2
        SofAircraft leader = GameManager.squadrons[aircraft.SquadronId][aircraft.placeInSquad - aircraft.placeInSquad % 2];
        if (leader.Destroyed) leader = aircraft;

        //Direction
        Vector3 targetPos = leader.transform.position - Vector3.ProjectOnPlane(leader.transform.forward, Vector3.up) * pairDistance;
        AircraftAxes axes = PointTracking.TrackingInputs(targetPos + leader.transform.forward * 400f, aircraft, leader.data.bankAngle.Get, 1f, true);
        aircraft.controls.SetTargetInput(axes, PitchCorrectionMode.FullyAssisted);

        //Throttle
        float dis = transform.InverseTransformDirection(targetPos - transform.position).z;
        float thr = leader.engines.Throttle + thrPID.UpdateUnclamped(dis, Time.fixedDeltaTime);
        aircraft.engines.SetThrottleAllEngines(thr, false);
    }
}
