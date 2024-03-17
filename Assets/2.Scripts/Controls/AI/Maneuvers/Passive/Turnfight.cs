using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turnfight : Maneuver
{
    public override void Execute(AI.GeometricData data)
    {
        Transform tr = data.aircraft.transform;

        Vector3 dumbTarget = (data.target.transform.position + data.target.data.rb.velocity - tr.transform.position).normalized;
        Vector3 flatTurnTarget = Vector3.Cross(Vector3.up, new Vector3(tr.forward.x, 0f, tr.forward.z)).normalized;

        //Choose the right direction for the flat turn
        float bankFactor = Mathf.Sign(data.aircraft.data.bankAngle.Get) * Mathf.PingPong(data.aircraft.data.bankAngle.Get, 90f) / 90f * 0.2f;
        flatTurnTarget *= Mathf.Sign(Vector3.Dot(flatTurnTarget, dumbTarget) - bankFactor);

        //Mix flat and dumb to get the final target
        Vector3 target = Mathv.LerpDirection(flatTurnTarget, dumbTarget, Vector3.Dot(dumbTarget.normalized, tr.forward));
        target = tr.transform.position + target * 500f;

        AircraftControl.Tracking(target, data.aircraft, 0f, 0f, true);
    }

}
