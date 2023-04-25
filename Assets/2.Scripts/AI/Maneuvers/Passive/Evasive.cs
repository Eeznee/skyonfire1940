using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Evasive : Maneuver
{
    TurnData turn;
    const float minAltitude = 100f;
    const float safeAltitude = 800f;
    public override void Execute(AI.GeometricData data)
    {
        if (turn == null || turn.ended)
        {
            float side = Mathf.Sign(Random.Range(-1f, 1f));

            float speedFactor = Mathf.InverseLerp(data.aircraft.cruiseSpeed * 0.6f, data.aircraft.cruiseSpeed, data.aircraft.data.ias.Get);
            float minBank = Mathf.Lerp(60f,80f, speedFactor);
            float maxBank = Mathf.Lerp(80f, 120f,(data.aircraft.data.relativeAltitude.Get + minAltitude) / safeAltitude);
            float bank = Random.Range(minBank, maxBank);

            float intensity = Mathf.Lerp(0f, 1f, data.aircraft.data.ias.Get / data.aircraft.cruiseSpeed) * Random.Range(0.4f, 0.7f);

            turn = new TurnData(data.aircraft, bank * side, Random.Range(3f, 7f), intensity);
        }
        turn.TurnFixedTime();
    }
}
