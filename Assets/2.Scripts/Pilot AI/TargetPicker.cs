using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TargetPicker
{
    public static SofAircraft PickTargetPilot(SofAircraft aircraft, List<SofAircraft> targets, SofAircraft currentTarget,float bombersPrio)
    {
        if (targets == null || targets.Count == 0) return null;
        float bestPrio = TargetPrioPilot(aircraft, targets[0], currentTarget,bombersPrio);
        int picked = 0;
        for(int i = 1; i < targets.Count; i++)
        {
            float prio = TargetPrioPilot(aircraft, targets[i], currentTarget,bombersPrio);
            if (!targets[i].destroyed && prio < bestPrio)
            {
                bestPrio = prio;
                picked = i;
            }
        }
        if (targets[picked].destroyed) return null;
        return targets[picked];
    }
    public static SofAircraft PickTargetGunner(GunMount turret, List<SofAircraft> targets, SofAircraft currentTarget)
    {
        if (targets == null || targets.Count == 0) return null;
        float bestPrio = TargetPrioGunner(turret, targets[0], currentTarget);
        int picked = 0;
        for (int i = 1; i < targets.Count; i++)
        {
            float prio = TargetPrioGunner(turret, targets[i], currentTarget);
            if (!targets[i].destroyed && prio < bestPrio)
            {
                bestPrio = prio;
                picked = i;
            }
        }
        if (targets[picked].destroyed) return null;
        return targets[picked];
    }
    private static float TargetPrioPilot(SofAircraft aircraft, SofAircraft target, SofAircraft currentTarget,float bombersPrio)
    {
        Rigidbody rb = aircraft.rb;
        Rigidbody targetRb = target.rb;
        Vector3 dir = target.tr.position - aircraft.tr.position;
        float dis = dir.magnitude;
        dir = dir / dis;
        float closure = Vector3.Dot(targetRb.velocity - rb.velocity, dir);
        float prio = dis + Mathf.Abs(closure) * 10f;
        prio *= Mathf.Clamp01(2f * (target.card.bomber ? bombersPrio : 1f - bombersPrio)); 
        if (currentTarget == target) prio -= 400f;


        SofAircraft[] squadron =GameManager.squadrons[aircraft.SquadronId];
        foreach(SofAircraft squadAircraft in squadron)
        {
            if (squadAircraft == aircraft) continue;

            if (squadAircraft.mainSeat.target == target && (squadAircraft.tr.position - aircraft.tr.position).magnitude < 300f) prio += 300f;
        }

        return prio;
        //Priority is only dependant on closure and distance
        //Zero closure makes an aircraft an important target
        //The priority of a target dis 0 closure -200m/s is  the same as a target 1 km away
        //The priority of a target dis 500 closure 10 m/s is  the same as a target 600 m away
    }
    private static float TargetPrioGunner(GunMount turret, SofAircraft target, SofAircraft currentTarget)
    {
        Vector3 dir = target.tr.position - turret.tr.position;
        float dis = dir.magnitude;
        dir /= dis;
        float threaten = Vector3.Dot(target.transform.forward, dir) + 1f;
        float alignement = -Vector3.Dot(turret.FiringDirection, dir) + 1f;
        float prio = dis + threaten * 300f + alignement * 400f;
        prio += Random.Range(0f, 300f);
        if (currentTarget == target) prio -= 200f;
        return prio * turret.TargetAvailability(target.tr.position);
        //Target aiming perpendicular is equivalent to 300m, target aiming opposite is equivalent to 600m
        //Target aligned perpendicularly to guns is equivalent to 400m, target aligned opposite to guns is equivalent to 800m
    }
}
