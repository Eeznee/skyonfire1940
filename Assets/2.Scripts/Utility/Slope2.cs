using UnityEngine;

public class Slope2
{
    public Vector2 start;
    public Vector2 mid;
    public Vector2 end;
    public Slope2(float sx,float sy,float mx,float my, float ex,float ey)
    {
        start = new Vector2(sx, sy);
        mid = new Vector2(mx, my);
        end = new Vector2(ex, ey);
    }
    public Slope2(Vector2 s, Vector2 m, Vector2 e)
    {
        start = s;
        mid = m;
        end = e;
    }
    public Slope2(Vector2 m)
    {
        start = Vector2.zero;
        mid = m;
        end = Vector2.zero;
    }
    public Slope2()
    {
        start = Vector2.zero;
        mid = Vector2.zero;
        end = Vector2.zero;
    }
    public float A1()
    {
        return (mid.x - start.x) * (start.y + mid.y) * 0.5f;
    }
    public float A2()
    {
        return (end.x - mid.x) * (end.y + mid.y) * 0.5f;
    }
    public float Area()
    {
        return A1() + A2();
    }
    public float Evaluate(float x)
    {
        if (x < mid.x) return Mathf.Lerp(start.y, mid.y, Mathf.InverseLerp(start.x, mid.x, x));
        else return Mathf.Lerp(mid.y, end.y, Mathf.InverseLerp(mid.x, end.x, x));
    }
    public float Integral(float x)
    {
        if (x <= mid.x)
            return (x - start.x) * (Evaluate(x) + start.y) * 0.5f;
        else
            return A1() + (x - mid.x) * (Evaluate(x) + mid.y) * 0.5f;

    }
    public float Integral(float from, float to)
    {
        return Integral(to) - Integral(from);
    }
    public void Scale(float s)
    {
        start.y *= s;
        mid.y *= s;
        end.y *= s;
    }
    public void Normalize()
    {
        Scale(1f / Area());
    }
}
