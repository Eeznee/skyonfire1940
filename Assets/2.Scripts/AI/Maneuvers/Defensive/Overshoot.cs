using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Overshoot : ActiveManeuver
{
    const float fullThrottleSpeedDelta = 30f;
    private float side;
    TurnData turn;

    public override float PickFactor(AI.GeometricData data)
    {
        float speedDeltaFactor = Mathf.InverseLerp(-20f, 20f, data.aircraft.data.gsp - data.target.data.gsp);
        float speedFactor = Mathf.InverseLerp(data.aircraft.cruiseSpeed * 0.6f, data.aircraft.cruiseSpeed * 0.8f, data.aircraft.data.gsp);
        float crossingFactor = Mathf.Clamp01(1f - data.crossAngle / 90f);
        float distanceFactor = Mathf.InverseLerp(400f, 50f, data.distance);
        return speedDeltaFactor * speedFactor * crossingFactor * distanceFactor;
    }
    public override string Label()
    {
        return "Overshoot";
    }
    public override void Execute(AI.GeometricData data)
    {
        if (done) return;
        base.Execute(data);

        float throttle = (data.target.data.gsp - aircraft.data.gsp) / fullThrottleSpeedDelta;
        aircraft.SetThrottle(Mathf.Clamp01(throttle));
        if (turn == null || turn.ended)
        {
            if (data.crossAngle < 10f)
            {
                side = Mathf.Sign(Random.Range(-1f, 1f));
                turn = new TurnData(data.aircraft, 90f * side, Random.Range(3f, 5f), 1f);
            }
            else
            {
                AircraftControl.Tracking(transform.position + data.target.transform.forward * 500f, aircraft, data.target.data.bankAngle, 1f, true);
            }
        }
        else
        {
            turn.bankAngle -= side * Time.fixedDeltaTime * 90f;
            turn.TurnFixedTime();
        }

        if (data.state != AI.DogfightState.Defensive) done = true;
    }
}
