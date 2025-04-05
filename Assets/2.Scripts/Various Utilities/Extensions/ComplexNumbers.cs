using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComplexNumbers
{
    public static Vector2 Pow(Vector2 z, float p)
    {
        float angle = Vector2.Angle(Vector2.right, z) * Mathf.Deg2Rad;
        float magnitude = Mathf.Pow(z.sqrMagnitude, p * 0.5f);

        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * magnitude;
    }

    public static Vector2 Divide(float a, Vector2 z)
    {
        return new Vector2(a * z.x ,- a * z.y) / z.sqrMagnitude;
    }
}
