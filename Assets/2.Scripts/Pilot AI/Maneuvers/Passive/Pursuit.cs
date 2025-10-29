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

        if (!data.target.card.fighter && (data.distance < 70f || data.Collision(2.5f)))
        {
            DisengageFromBomberBeforeCollision(data);
            return;
        }



        float bulletTime = data.distance / 850f;
        bulletTime += Mathf.PerlinNoise(Time.time * 0.2f, data.aircraft.mainSeat.aiRandomizedPerlin * 2f) * 2f - 1f;
        Vector3 target = data.target.transform.position + data.target.rb.linearVelocity * bulletTime;


        float levelingFactor = Mathf.InverseLerp(30f,0f,data.offAngle);
        float targetBank = data.target.data.bankAngle.Get;
        targetBank += 60f * (Mathf.PerlinNoise(Time.time * 0.1f, data.aircraft.mainSeat.aiRandomizedPerlin) * 2f - 1f);

        aircraft.controls.SimpleTrackingPos(target, targetBank, levelingFactor, true);
    }

    public void DisengageFromBomberBeforeCollision(AI.GeometricData data)
    {
        float bank = Random.Range(70f, 110f) * Mathf.Sign(Random.value - 0.5f);
        turn = new TurnData(aircraft, bank, turnTime, 1f);
    }
}
