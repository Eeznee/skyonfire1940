using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class Mathv
{
    public static float Angle180(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
    }
    public static Quaternion Damp(Quaternion a, Quaternion b, float lambda)
    {
        return Quaternion.Slerp(a, b, 1 - Mathf.Exp(-lambda * Time.unscaledDeltaTime));
    }
    public static float SquareArea(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {
        float abcArea = TriangleArea(A, B, C);
        float acdArea = TriangleArea(A, C, D);

        return abcArea + acdArea;
    }
    public static float TriangleArea(float AB, float BC, float AC)
    {
        //perimeter coeff equal to half the perimeter
        float pc = (AB + BC + AC) / 2;

        //Using the Heron's formula to get area
        return Mathf.Sqrt(pc * (pc - AB) * (pc - BC) * (pc - AC));
    }

    public static Vector3 ClampRectangle(Vector3 pos, float w, float h)
    {
        pos.x -= w / 2f;
        pos.y -= h / 2f;
        if (Mathf.Abs(pos.x) > w / 2f) pos *= w / 2f / Mathf.Abs(pos.x);
        else if (Mathf.Abs(pos.y) > h / 2f) pos *= h / 2f / Mathf.Abs(pos.y);
        pos.x += w / 2f;
        pos.y += h / 2f;
        return pos;
    }
    public static float QuickSin(float rad)
    {
        return rad - rad * rad * rad / 6f;
    }
    public static float QuickCos(float rad)
    {
        return 1f - rad * rad / 2f + rad * rad * rad * rad / 24f;
    }
    public static float QuickASin(float sin)
    {
        return sin + sin * sin * sin / 6f;
    }
    public static float TriangleArea(Vector3 A, Vector3 B, Vector3 C)
    {
        //Calculation of sides
        float AB = (A - B).magnitude;
        float BC = (B - C).magnitude;
        float AC = (A - C).magnitude;

        return TriangleArea(AB, BC, AC);
    }
    public static float SmoothStop(float x, int p)
    {
        float powered = 1f;
        for (int i = 0; i < p; i++) powered *= 1f - x;
        return 1f - powered;
    }
    public static float SmoothStart(float x, int p)
    {
        float powered = 1f;
        for (int i = 0; i < p; i++) powered *= x;
        return powered;
    }
    public static float SmoothStep(float x, int p)
    {
        return SmoothStart(x, p) * (1f - x) + SmoothStop(x, p) * x;
    }

    public static float SignNoZero(float f)
    {
        return f == 0f ? 1f : Mathf.Sign(f);
    }

    public static float Cosh(float x)
    {
        return (Mathf.Exp(x) + Mathf.Exp(-x)) / 2f;
    }
    public static float Sinh(float x)
    {
        return (Mathf.Exp(x) - Mathf.Exp(-x)) / 2f;
    }

    static Vector2[] hexSides = new Vector2[] { new Vector2(0f, 1f), new Vector2(0.866f, -0.5f), new Vector2(-0.866f, -0.5f) };
    public static Vector2 HexPoint(float x, float y, int sub)
    {
        return hexSides[sub % 3] * x + hexSides[(sub + 1) % 3] * y;
    }
    public static Vector3 LerpDirection(Vector3 from,Vector3 to, float t)
    {
        float angle = Vector3.Angle(from, to) * Mathf.Deg2Rad;
        return Vector3.RotateTowards(from, to, angle * Mathf.Clamp01(t) , 1f).normalized;
    }
    public static Vector2 RandomHexPoint(float size)
    {
        return HexPoint(Random.value, Random.value, Random.Range(0, 3));
    }

    public static Vector3 HexTilePosition(int x, int y, float hexScale, int boardSize)
    {
        int dy = y - boardSize;
        int dx = x - boardSize;
        Vector3 pos = new Vector3(dx * hexScale, 0f, 0f);
        pos += new Vector3(dy * hexScale * 0.5f,0f, dy * 0.866f * hexScale);
        return pos;
    }

    public static float OptimalLeverRotation(Transform lever,Vector3 target, Vector3 axis,Vector3 defaultLeverUp)
    {
        axis = lever.parent.TransformDirection(axis).normalized;
        Vector3 gripDir = Vector3.ProjectOnPlane(target - lever.position, axis);
        return Vector3.SignedAngle(defaultLeverUp, gripDir, axis);
    }

    public static float InverseLerpVec3(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }
    public static Color CombineColors(Color a, Color b)
    {
        Color c = new Color();
        c.a = a.a + b.a * (1f - a.a);
        if (c.a == 0f) return Color.clear;
        c.r = (a.r * a.a + b.r * b.a * (1f - a.a)) / c.a;
        c.g = (a.g * a.a + b.g * b.a * (1f - a.a)) / c.a;
        c.b = (a.b * a.a + b.b * b.a * (1f - a.a)) / c.a;
        return c;
    }
}
