using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class Mathv
{
    public const float invPI = 1f / Mathf.PI;
    public static float Angle180(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
    }
    public static Quaternion Damp(Quaternion a, Quaternion b, float lambda, float maxAngularSpeed)
    {
        Quaternion cappedSpeed = Quaternion.RotateTowards(a, b, maxAngularSpeed * Time.unscaledDeltaTime);
        Quaternion damped = Quaternion.Slerp(a, b, 1f - Mathf.Exp(-lambda * Time.unscaledDeltaTime));

        if (Quaternion.Angle(cappedSpeed, b) > Quaternion.Angle(damped, b)) return cappedSpeed;
        return damped;
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


    public static float QuickSin(float rad)
    {
        return rad - rad * rad * rad / 6f;
    }
    public static float QuickCos(float rad)
    {
        return 1f - rad * rad / 2f + rad * rad * rad * rad / 24f;
    }
    public static float QuickLoopingCos(float rad)
    {
        rad = Mathf.Repeat(rad, Mathf.PI * 2f);
        if (rad > Mathf.PI) rad = Mathf.PI * 2f - rad;

        float t = (rad * invPI - 0.4f) * 5f;
        return Mathf.Lerp(QuickCos(rad), -QuickCos(rad - Mathf.PI), t);
    }
    public static float QuickLoopingSin(float rad)
    {
        return QuickLoopingCos(rad - Mathf.PI * 0.5f);
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
    public static float Square(float x)
    {
        return x * x;
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
    public static Vector3 LerpDirection(Vector3 from, Vector3 to, float t)
    {
        float angle = Vector3.Angle(from, to) * Mathf.Deg2Rad;
        return Vector3.RotateTowards(from, to, angle * Mathf.Clamp01(t), 1f).normalized;
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
        pos += new Vector3(dy * hexScale * 0.5f, 0f, dy * 0.866f * hexScale);
        return pos;
    }

    public static float OptimalLeverRotation(Transform lever, Vector3 target, Vector3 axis, Vector3 defaultLeverUp)
    {
        axis = lever.parent.TransformDirection(axis).normalized;
        Vector3 gripDir = Vector3.ProjectOnPlane(target - lever.position, axis);
        return Vector3.SignedAngle(defaultLeverUp, gripDir, axis);
    }
    public static float Map(float outFrom, float outTo, float inFrom, float inOut, float t)
    {
        return Mathf.Lerp(outFrom, outTo, Mathf.InverseLerp(inFrom, inOut, t));
    }
    public static float InverseLerpUnclamped(float a, float b, float value)
    {
        return (value - a) / (b - a);
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

    public static float SolveQuadratic(float a, float b, float c, bool minusDelta)
    {
        float delta = Mathf.Sqrt(b * b - 4f * a * c);
        if (minusDelta)
            return (-b - delta) / (2f * a);
        else
            return (-b + delta) / (2f * a);
    }
    public const float cubicRoot = 1 / 3f;
    public static float CubicRoot(float x)
    {
        return Mathf.Pow(Mathf.Abs(x), cubicRoot) * Mathf.Sign(x);
    }

    public static float ClampAbs(float x, float absMax)
    {
        absMax = Mathf.Abs(absMax);
        return Mathf.Clamp(x, -absMax, absMax);
    }
}

public static class M
{
    public static float SquareSigned(float x)
    {
        return x * Mathf.Abs(x);
    }
    public static float Pow(float a, int p)
    {
        float result = 1f;
        for (int i = 0; i < p; i++)
        {
            result *= a;
        }
        return result;
    }
    public static float AbsPow(float a, int p)
    {
        float result = 1f;
        for (int i = 0; i < p; i++)
        {
            result *= a;
        }
        return Mathf.Abs(Pow(a, p));
    }
}
