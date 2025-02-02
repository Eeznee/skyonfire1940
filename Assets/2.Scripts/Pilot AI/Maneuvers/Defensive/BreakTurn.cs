using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakTurn : ActiveManeuver
{
    private Vector3 initialDirection;
    private Turnfight turnFight;
    const float maxDuration = 30f;

    public BreakTurn()
    {
        turnFight = new Turnfight();
    }
    public override void Initialize(AI.GeometricData data)
    {
        base.Initialize(data);
        initialDirection = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        turnFight = new Turnfight();
    }

    public override string Label()
    {
        return "Break Turn";
    }
    public override float PickFactor(AI.GeometricData data)
    {
        float closureFactor = Mathf.InverseLerp(20f,-30f, data.closure);
        float disFactor = Mathf.InverseLerp(-200f,500f,data.distance);
        return closureFactor * disFactor;
    }
    public override void Execute(AI.GeometricData data)
    {
        if (done) return;

        base.Execute(data);

        turnFight.Execute(data);
        if (data.state == AI.DogfightState.Offensive || data.state == AI.DogfightState.Engage) done = true;
        if (timeStart + maxDuration < Time.time) done = true;
    }
}
