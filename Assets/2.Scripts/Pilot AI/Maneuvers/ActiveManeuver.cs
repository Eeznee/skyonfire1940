using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActiveManeuver : Maneuver
{
    protected int phase = 0;
    public bool done;
    protected float timeStart;

    protected float TimeSinceManeuverStart => Time.time - timeStart;

    public virtual void Initialize(AI.GeometricData data)
    {
        phase = 0;
        done = false;
        timeStart = Time.time;
        aircraft = data.aircraft;
        transform = aircraft.transform;
        target = data.target;
    }
    public abstract string Label();

    public virtual float PickFactor(AI.GeometricData data)
    {
        return 0f;
    }
    public override void Execute(AI.GeometricData data)
    {
        if (target != data.target) { done = true; return; }
    }
}
