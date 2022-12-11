using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Ballistics
{
    public static Quaternion Spread(Quaternion rotation, float maxAngle)
    {
        rotation *= Quaternion.Euler(0f, 0f, Random.Range(-90, 90));
        rotation *= Quaternion.Euler(Random.Range(-1f, 1f) * maxAngle, 0f, 0f);
        return rotation;
    }

    public static float ApproximatePenetration(float mass, float vel, float diameter)
    {
        return mass * vel * vel / (Mathf.Pow(diameter, 1.5f) * 35f);
    }
    public static Vector3[] BallisticPath(Vector3 startPos, Vector3 dir, float speed, float dragCoeff, int points, float lifetime)
    {
        float logConst = Mathf.Log(1f / speed) / dragCoeff;
        Vector3[] worldPos = new Vector3[points];
        for (int i = 0; i < points; i++)
        {
            float t = (float)i / points * lifetime;
            worldPos[i] = startPos;
            worldPos[i] += Physics.gravity / 2f * t * t;
            worldPos[i] += dir * (Mathf.Log(dragCoeff * t + 1f / speed) / dragCoeff - logConst);
        }
        return worldPos;
    }
    public static RaycastHit[] RaycastAndSort(Vector3 pos, Vector3 dir, float range, int layerMask)
    {
        RaycastHit[] hits = Physics.RaycastAll(pos, dir, range, layerMask);
        for (int i = 0; i < hits.Length - 1; i++) //Sort hits in order
            for (int j = 0; j < hits.Length - i - 1; j++)
                if (hits[j].distance > hits[j + 1].distance) { RaycastHit jplus1 = hits[j + 1]; hits[j + 1] = hits[j]; hits[j] = jplus1; }
        return hits;
    }
    public static float ExplosionRangeSimple(float kg)
    {
        return Mathf.Sqrt(kg);
    }
    public static float HalfExplosionRangeSimple(float kg)
    {
        return Mathf.Sqrt(kg) * 2f;
    }
    public static float InterceptionTime(float shotSpeed, Vector3 relativePos, Vector3 relativeVel)
    {
        float velocitySquared = relativeVel.sqrMagnitude;
        if (velocitySquared < 0.001f)
            return 0f;

        float a = velocitySquared - shotSpeed * shotSpeed;

        //handle similar velocities
        if (Mathf.Abs(a) < 0.001f)
        {
            float t = -relativePos.sqrMagnitude / (2f * Vector3.Dot(relativeVel, relativePos));
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
