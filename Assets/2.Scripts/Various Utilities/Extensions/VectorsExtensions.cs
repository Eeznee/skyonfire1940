using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorsExtensions
{
    public static Vector3 Up(this Quaternion q)
    {
        return q * Vector3.up;
    }
    public static Vector3 Forward(this Quaternion q)
    {
        return q * Vector3.forward;
    }
    public static Vector3 Right(this Quaternion q)
    {
        return q * Vector3.right;
    }

    public static Vector2 Rotate(this Vector2 v2, float angle)
    {
        Vector3 v3 = new Vector3(v2.x, v2.y, 0f);
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        v3 = rotation * v3;
        return new Vector2(v3.x, v3.y);
    }
    public static float ManhattanMagnitude(this Vector2 v)
    {
        return Mathf.Abs(v.x) + Mathf.Abs(v.y);
    }

    public static void ClampUnitSquare(ref this Vector2 v)
    {
        v.x = Mathf.Clamp(v.x, -1f, 1f);
        v.y = Mathf.Clamp(v.y, -1f, 1f);
    }
    public static bool IsContainedWithinDisplay(this Vector3 v)
    {
        if (v.x < 0f) return false;
        if (v.y < 0f) return false;

        if (v.x > Screen.width) return false;
        if (v.y > Screen.height) return false;

        return true;
    }
}
