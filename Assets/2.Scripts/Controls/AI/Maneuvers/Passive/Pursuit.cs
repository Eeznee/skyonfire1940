using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pursuit : Maneuver
{
    private TurnData turn;
    const float turnTime = 4.5f;
    public override void Execute(AI.GeometricData data)
    {
        base.Execute(data);
        if (turn != null)
        {
            turn.TurnFixedTime();
            if (turn.ended) turn = null;
            return;
        }
        
        if (!data.target.card.fighter && (data.distance < 70f || data.Collision(2.5f))){
            float bank = Random.Range(70f, 110f) * Mathf.Sign(Random.value - 0.5f);
            turn = new TurnData(aircraft, bank, turnTime, 1f);
        } else
        {
            float bulletTime = data.distance / 850f;
            Vector3 target = data.target.transform.position + data.target.data.rb.velocity * bulletTime;
            float levelingFactor = Mathf.Clamp01(1f - data.offAngle / 90f);
            AircraftAxes axes = PointTracking.TrackingInputs(target, data.aircraft, data.target.data.bankAngle.Get, levelingFactor, true);

            data.aircraft.inputs.SendAxes(axes, true, false);
        }
    }
}
