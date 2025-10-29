using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakFormation : ActiveManeuver
{
    private SofAircraft leader;
    private Vector3 breakDir;
    private float breakCountDown;
    public override void Initialize(AI.GeometricData data)
    {
        base.Initialize(data);
        leader = GameManager.squadrons[data.aircraft.SquadronId][0];
        breakDir = aircraft.card.formation.GetBreakDirection(leader.transform, aircraft.placeInSquad);
        breakCountDown = 2.5f;

        if (GameManager.squadrons[data.aircraft.SquadronId].Length == 1) done = true;
    }
    public override string Label()
    {
        return "Break Formation";
    }
    public override void Execute(AI.GeometricData data)
    {
        if (done) return;
        base.Execute(data);

        breakCountDown -= Time.fixedDeltaTime;
        aircraft.controls.SimpleTracking(breakDir, 0f, 0f, true);
        if (breakCountDown < 0f) done = true;
    }
}
