using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Evasive : Maneuver
{
    TurnData turn;

    const float baseBank = 70f;
    const float altitudeDeltaBankMultiplier = -0.06f;

    public override void Execute(AI.GeometricData data)
    {
        if (turn == null || turn.ended)
        {
            float targetAltitude = data.aircraft.mainSeat.aiTargetAltitude;
            float altitudeDelta = targetAltitude - data.aircraft.data.altitude.Get;

            float side = Mathf.Sign(Random.Range(-1f, 1f));

            float speedFactor = Mathf.InverseLerp(data.aircraft.cruiseSpeed * 0.6f, data.aircraft.cruiseSpeed, data.aircraft.data.ias.Get);

            float bank = baseBank + altitudeDelta * altitudeDeltaBankMultiplier;
            bank += Random.Range(-8f, 8f);
            bank = Mathf.Clamp(bank, 45f, 120f);

            float intensity = Mathf.Lerp(0f, 1f, data.aircraft.data.ias.Get / data.aircraft.cruiseSpeed) * Random.Range(0.4f, 0.7f);

            turn = new TurnData(data.aircraft, bank * side, Random.Range(3f, 7f), intensity);
        }
        turn.TurnFixedTime();
    }
}
