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
        SofAircraft leader = GameManager.squadrons[aircraft.squadronId][aircraft.placeInSquad - aircraft.placeInSquad % 2];
        if (leader.destroyed) leader = aircraft;

        //Direction
        Vector3 targetPos = leader.transform.position - Vector3.ProjectOnPlane(leader.transform.forward, Vector3.up) * pairDistance;
        Vector3 axis = AircraftControl.TrackingInputs(targetPos + leader.transform.forward * 400f, aircraft, leader.bankTurnAngle, 1f, true);
        aircraft.SetControls(axis, true, false);

        //Throttle
        float dis = transform.InverseTransformDirection(targetPos - transform.position).z;
        float thr = leader.throttle + thrPID.UpdateUnclamped(dis, Time.fixedDeltaTime);
        aircraft.SetThrottle(Mathf.Clamp01(thr));
        aircraft.boost = thr > 1.05f;
    }
}
