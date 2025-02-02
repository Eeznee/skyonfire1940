using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadOn : Maneuver
{
    TurnData turn;
    public override void Execute(AI.GeometricData data)
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
