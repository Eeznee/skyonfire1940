using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldFormation : Maneuver
{
    const float leadTime = 5f;
    private float breakFormationTimer;
    private float targetAltitude;
    private Vector3 breakDir;
    PID thrPID = new PID(new Vector3(0.007f, 0f, 0.05f));

    public override void Execute(SofAircraft a)
    {
        aircraft = a;
        transform = a.transform;
        base.Execute();


        SofAircraft[] squad = GameManager.squadrons[aircraft.squadronId];
        SofAircraft leader = null;
        for (int i = 0; i < squad.Length && leader == null; i++)
            if (!squad[i].destroyed) leader = squad[i];
        /*
        if (breakFormation && breakFormationTimer < Time.time)
        {
            Vector3 axis = AircraftControl.TrackingInputs(transform.position + breakDir * 300f, aircraft, 0f, 0f,true);
            aircraft.SetControls(axis, true, false);
        } 

        else
                */
        if (leader == aircraft || leader == null)
        {
            Vector3 pos = transform.position + transform.forward * 500f;
            pos.y = Mathf.Max(transform.position.y,500f);
            AircraftControl.Tracking(pos, aircraft, 0f, 1f,true);
        }
        else
        {
            //Direction
            Vector3 targetPos = aircraft.card.formation.GetPosition(leader.transform, aircraft.placeInSquad);
            AircraftControl.Tracking(targetPos + leader.transform.forward * 1000f, aircraft, leader.data.bankAngle, 1f, true);

            //Throttle
            float dis = transform.InverseTransformDirection(targetPos - transform.position).z;
            float thr = leader.throttle + thrPID.UpdateUnclamped(dis,Time.fixedDeltaTime);
            aircraft.SetThrottle(Mathf.Clamp01(thr));
            aircraft.boost = thr > 1.15f;
        }
        /*
        if (Input.GetKeyDown(KeyCode.B))
        {
            breakFormation = true;
            breakFormationTimer = Time.time + Random.Range(0.5f, 2f);
            breakDir = aircraft.card.formation.GetBreakDirection(leader.transform, aircraft.placeInSquad);
        }
        */
    }
}
