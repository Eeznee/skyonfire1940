using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadOn : Maneuver
{
    TurnData turn;

    public static bool CanHeadOn(AI.GeometricData data)
    {
        return data.energyDelta > 2000f && data.target.data.ias.Get < data.target.stats.altitudeZeroMaxSpeed * 0.5f;
    }
    public override void Execute(AI.GeometricData data)
    {
        if (CanHeadOn(data))
        {
            float bulletTime = data.distance / 850f;
            Vector3 target = data.target.transform.position + data.target.rb.velocity * bulletTime;
            AircraftAxes axes = PointTracking.TrackingInputs(target, data.aircraft, 0f, 0f, false);
            data.aircraft.controls.SetTargetInput(axes, PitchCorrectionMode.FullyAssisted);

        }
        else
        {
            if (turn == null || turn.ended)
            {
                float side = Mathf.Sign(Random.Range(-1f, 1f) * Mathf.Max(0f, Random.Range(-1f, 4f)));
                float bank = Random.Range(30f, 160f);
                turn = new TurnData(data.aircraft, bank * side, bank / 90f * Random.Range(1f, 2f), 1f);
            }
            if (data.distance / -data.closure < turn.turnTime && turn.bankAngle != 0f)
            {
                turn.TurnFixedTime();
            }
        }
    }
}
