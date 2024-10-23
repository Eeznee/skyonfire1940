using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAirfoil : IAirfoil
{
    public float zeroAlpha;
    public float maxCl;
    public float minCd;

    private float gradient;

    const float maxCd = 1.5f;

    public SimpleAirfoil(Bounds airframeBounds, float _zeroAlpha)
    {
        float thickness = Mathf.Min(airframeBounds.size.y, airframeBounds.size.x);
        minCd = maxCd * thickness / airframeBounds.size.z;
        maxCl = 0.7f;

        zeroAlpha = _zeroAlpha;

        RefreshGradient();
    }
    public SimpleAirfoil(float _maxCl, float _minCd,float _zeroAlpha)
    {
        maxCl = _maxCl;
        minCd = _minCd;
        zeroAlpha = _zeroAlpha;

        RefreshGradient();
    }
    private void RefreshGradient() { gradient = (maxCl - Coefficients(0f).y) / (90f * Mathf.Deg2Rad); }
    public Vector2 Coefficients(float alpha)
    {
        float rad = (alpha - zeroAlpha) * Mathf.Deg2Rad;
        float cd = (1f - Mathv.QuickLoopingCos(rad)) * (maxCd - minCd) * 0.5f + minCd;
        float cl = Mathv.QuickLoopingSin(rad) * maxCl;
        return new Vector2(cd, cl);
    }
    public Vector2 Coefficients(float alpha, float flaps)
    {
        return Coefficients(alpha);
    }
    public float Gradient() { return gradient; }
    public float Gradient(float flaps) { return gradient; }
}
