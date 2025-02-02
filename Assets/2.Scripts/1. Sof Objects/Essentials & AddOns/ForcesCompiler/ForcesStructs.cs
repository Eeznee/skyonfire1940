using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;




public struct ForceAtPoint
{
    public Vector3 force;
    public Vector3 point;

    public ForceAtPoint(Vector3 _force, Vector3 _point)
    {
        force = _force;
        point = _point;
    }
}
public struct ResultingForce
{
    public Vector3 force;
    public Vector3 torque;

    public ResultingForce(ForceAtPoint[] forceAtPoints)
    {
        force = Vector3.zero;
        torque = Vector3.zero;

        for (int i = 0; i < forceAtPoints.Length; i++)
        {
            ForceAtPoint fap = forceAtPoints[i];
            force += fap.force;

            torque += Vector3.Cross(fap.point, fap.force);
        }
    }
    public ResultingForce(Vector3 _force, Vector3 _torque)
    {
        force = _force;
        torque = _torque;
    }
}
