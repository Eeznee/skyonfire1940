using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldFormation : Maneuver
{
    PID thrPID = new PID(new Vector3(0.007f, 0f, 0.1f));
    PID pitchPID = new PID(new Vector3(0.01f, 0.001f, 0.04f));
    PID bankPID = new PID(new Vector3(0.015f, 0f, 0.06f));


    const float threshold = 800f;


    private float timeSinceLast = 0f;
    private Vector3 direction;
    private float altitude;
    private float targetBank;
    private float targetPitch;


    public void ReInitializeHoldFormation()
    {
        direction = transform.forward;
        direction.y = 0f;
        direction =  direction.normalized * 2000f;

        altitude = Mathf.Max(transform.position.y, 250f);

        targetPitch = aircraft.data.pitchAngle.Get;
        targetBank = aircraft.data.bankAngle.Get;
        if (Mathf.Abs(targetBank) < 10f && Mathf.Abs(targetPitch) < 10f) targetBank = targetPitch = 0f;
    }


    public void LeadFormation()
    {
        if (Player.aircraft == aircraft && (targetPitch != 0f || targetBank != 0f))
        {
            aircraft.controls.MaintainBankAndPitchAvoidCrash(targetPitch, targetBank);
        }
        else
        {
            Vector3 pos = transform.position + direction;
            pos.y = altitude;
            aircraft.controls.SimpleTrackingPos(pos, 0f, 1f, true);
        }
    }
    public void FollowLeader(SofAircraft leader)
    {
        Vector3 targetPos = aircraft.card.formation.GetPosition(leader.transform, aircraft.placeInSquad);
        Vector3 formationLocalDelta = leader.transform.InverseTransformDirection(targetPos - transform.position);

        if ((targetPos - transform.position).sqrMagnitude > threshold * threshold)
        {
            Vector3 distantTargetPos = targetPos + leader.transform.forward * 500f;
            distantTargetPos.y = Mathf.Max(distantTargetPos.y, 15f);
            aircraft.controls.SimpleTrackingPos(distantTargetPos, 0f, 0f, true);
        }
        else
        {
            float pitch = leader.data.pitchAngle.Get + pitchPID.UpdateLockIntegral(formationLocalDelta.y, Time.fixedDeltaTime) * 45f;
            float bank = leader.data.bankAngle.Get + bankPID.Update(-formationLocalDelta.x, Time.fixedDeltaTime) * 45f;

            aircraft.controls.MaintainBankAndPitchAvoidCrash(pitch, bank);
        }

        float thr = leader.engines.Throttle + thrPID.UpdateUnclamped(formationLocalDelta.z, Time.fixedDeltaTime);
        aircraft.engines.SetThrottleAllEngines(thr, false);
    }


    public override void Execute(SofAircraft a)
    {
        aircraft = a;
        transform = a.transform;
        base.Execute();

        if (Time.time - timeSinceLast > 1f || timeSinceLast == 0f) ReInitializeHoldFormation();
        timeSinceLast = Time.time;

        SofAircraft[] squad = GameManager.squadrons[aircraft.SquadronId];
        SofAircraft leader = null;
        for (int i = 0; i < squad.Length && leader == null; i++)
            if (!squad[i].Destroyed) leader = squad[i];

        if (leader == aircraft || leader == null)
            LeadFormation();
        else
            FollowLeader(leader);
    }
}
