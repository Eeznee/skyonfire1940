using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turnfight : Maneuver
{
    TurnData turn;
    const float altitudeDeltaBankMultiplier = -0.04f;
    public override void Execute(AI.GeometricData data)
    {
        Transform tr = data.aircraft.transform;
        float targetAltitude = data.aircraft.mainSeat.aiTargetAltitude;
        float altitudeDelta = targetAltitude - data.aircraft.data.altitude.Get;

        Vector3 up = Vector3.ProjectOnPlane(Vector3.up, tr.forward);
        Vector3 targetBankedDir = Vector3.ProjectOnPlane(data.dir + data.target.rb.linearVelocity, tr.forward);
        float bankAngleToTarget = Vector3.SignedAngle(up, targetBankedDir,tr.forward);

        float randomPerlin = 30f * (Mathf.PerlinNoise(Time.time * 0.05f, data.aircraft.mainSeat.aiRandomizedPerlin) * 2f - 1f);
        float deltaAltitudeBank = Mathf.Sign(bankAngleToTarget) * Mathf.Clamp(altitudeDelta * altitudeDeltaBankMultiplier,-12f,12f);
        float bankAngleChosen = bankAngleToTarget + deltaAltitudeBank + randomPerlin;

        turn = new TurnData(data.aircraft, bankAngleChosen, 1f, 0.5f);
        turn.TurnFixedTime();

        //target.y = data.aircraft.mainSeat.aiTargetAltitude;
        //PointTracking.Tracking(target, data.aircraft, 0f, 0f, true);


        /*
        Vector3 dumbTarget = (data.target.transform.position + data.target.rb.velocity - tr.transform.position).normalized;
        Vector3 flatTurnTarget = Vector3.Cross(Vector3.up, new Vector3(tr.forward.x, 0f, tr.forward.z)).normalized;

        //Choose the right direction for the flat turn
        float bankFactor = Mathf.Sign(data.aircraft.data.bankAngle.Get) * Mathf.PingPong(data.aircraft.data.bankAngle.Get, 90f) / 90f * 0.2f;
        flatTurnTarget *= Mathf.Sign(Vector3.Dot(flatTurnTarget, dumbTarget) - bankFactor);

        //Mix flat and dumb to get the final target
        Vector3 target = Mathv.LerpDirection(flatTurnTarget, dumbTarget, Vector3.Dot(dumbTarget.normalized, tr.forward));
        target = tr.transform.position + target * 500f;
        */
    }

}
