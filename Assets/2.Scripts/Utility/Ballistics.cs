using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Ballistics
{
    /*
    public static Vector3 BallisticTrajectory(Vector3 initPos, Vector3 initSpeed, float t,float a)
    {
        Vector3 pos = initPos;
        pos += Vector3.down * Mathf.Log(Mathv.Cosh(Mathf.Sqrt(a * -Physics.gravity.y) * (initPos.y + t))) / a;

        return pos;
    }
    */
    public static Vector3 BallisticTrajectory(Vector3 initPos, Vector3 initSpeed, float t, float a)
    {
        Vector3 pos = initPos;
        pos += Physics.gravity / 2f * Mathf.Pow(t, 2);
        pos += initSpeed.normalized * (Mathf.Log(a * t + 1f / initSpeed.magnitude) / a - Mathf.Log(1f / initSpeed.magnitude) / a);
        return pos;
    }

    public static float ExplosionRangeSimple(float kg)
    {
        return Mathf.Sqrt(kg);
    }
    public static float HalfExplosionRangeSimple(float kg)
    {
        return Mathf.Sqrt(kg) * 2f;
    }
    public static float InterceptionTime(float shotSpeed, Vector3 relativePos,Vector3 relativeVel)
    {
        float velocitySquared = relativeVel.sqrMagnitude;
        if (velocitySquared < 0.001f)
            return 0f;

        float a = velocitySquared - shotSpeed * shotSpeed;

        //handle similar velocities
        if (Mathf.Abs(a) < 0.001f)
        {
            float t = -relativePos.sqrMagnitude /(2f * Vector3.Dot ( relativeVel, relativePos ) );
            return Mathf.Max(t, 0f); //don't shoot back in time
        }

        float b = 2f * Vector3.Dot(relativeVel, relativePos);
        float c = relativePos.sqrMagnitude;
        float determinant = b * b - 4f * a * c;

        if (determinant > 0f)
        { //determinant > 0; two intercept paths (most common)
            float t1 = (-b + Mathf.Sqrt(determinant)) / (2f * a),
                    t2 = (-b - Mathf.Sqrt(determinant)) / (2f * a);
            if (t1 > 0f)
            {
                if (t2 > 0f)
                    return Mathf.Min(t1, t2); //both are positive
                else
                    return t1; //only t1 is positive
            }
            else
                return Mathf.Max(t2, 0f); //don't shoot back in time
        }
        else if (determinant < 0f) //determinant < 0; no intercept path
            return 0f;
        else //determinant = 0; one intercept path, pretty much never happens
            return Mathf.Max(-b / (2f * a), 0f); //don't shoot back in time
    }
}
