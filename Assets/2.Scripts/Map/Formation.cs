using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Formation", menuName = "Map/Formation")]
public class Formation : ScriptableObject
{
    public string formationName = "V";

    public Vector3[] aircraftPositions;
    public Vector3[] breakDirections;
    public float minDistance = 15f;
    public bool rollRelative;

    public Vector3 GetPosition(Transform leader,int index)
    {
        Vector3 localPos = aircraftPositions[index];
        Vector3 pos = leader.position;
        pos += localPos.z * leader.forward;
        pos += localPos.x * (rollRelative ? leader.right : -Vector3.Cross(leader.forward, Vector3.up).normalized);
        pos += localPos.y * (rollRelative ? leader.up : Vector3.up);
        return pos;
    }
    public Vector3 GetBreakDirection(Transform leader, int index)
    {
        Vector3 breakDir = breakDirections[index];
        Vector3 dir = breakDir.z * leader.forward;
        dir += breakDir.x * -Vector3.Cross(leader.forward, Vector3.up).normalized;
        dir += breakDir.y * Vector3.up;
        return dir;
    }
}
