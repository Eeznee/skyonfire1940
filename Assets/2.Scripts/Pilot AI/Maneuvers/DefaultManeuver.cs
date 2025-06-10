using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultManeuver : Maneuver
{
    private Turnfight turnfight;
    private Pursuit pursuit;
    private HeadOn headOn;
    private Evasive evasive;
    private FollowPair pair;
    public DefaultManeuver()
    {
        turnfight = new Turnfight();
        pursuit = new Pursuit();
        headOn = new HeadOn();
        evasive = new Evasive();
        pair = new FollowPair();
    }
    public override void Execute(AI.GeometricData data)
    {
        base.Execute(data);

        if (aircraft.CanPairUp()) { pair.Execute(aircraft); return; }

        switch (data.state)
        {
            case AI.DogfightState.Offensive: pursuit.Execute(data); break;
            case AI.DogfightState.Defensive: evasive.Execute(data); break;
            case AI.DogfightState.Turnfight:  turnfight.Execute(data); break;
            case AI.DogfightState.HeadOn: headOn.Execute(data); break;
            case AI.DogfightState.Engage:  pursuit.Execute(data); break;
        }
    }
}
