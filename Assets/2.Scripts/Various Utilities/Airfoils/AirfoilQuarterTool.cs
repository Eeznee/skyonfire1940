using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class AirfoilQuarterTool
{
    private float maxCl;
    private float maxAlpha;
    private float zeroCl;
    private float minCd;

    private float sign;

    //Lift coefficient constants
    const float ltclf = 1.02f; //linearTargetClFactor
    const float stallDepth = 0.5f;
    const float stallOffsetAlpha = 6f;
    const float stallOffsetAlphaInvert = 1f / stallOffsetAlpha;
    const float flatMaxCl = 1f;
    const float one45Ratio = 1f / 45f;

    //Drag coefficient constants
    const float cdclAtPeak = 0.02f;
    const float stalledDrag = 0.12f;
    const float flatPlateMaxDrag = 1.8f;

    public AirfoilQuarterTool(float _maxCl, float _maxAlpha, float _zeroCl, float _minCd)
    {
        maxCl = _maxCl;
        maxAlpha = _maxAlpha;
        zeroCl = _zeroCl;
        minCd = _minCd;

        linearSlope = (maxCl * ltclf - zeroCl) / maxAlpha;
        linearToPeakAlpha = (maxAlpha * (maxCl * (ltclf - 2f) + zeroCl)) / (zeroCl - maxCl * ltclf);
        peakFactor = Mathv.SmoothStart(zeroCl - maxCl * ltclf, 2) / (4 * maxAlpha * maxAlpha * maxCl * (ltclf - 1));
        stallMinimaAlpha = maxAlpha + stallOffsetAlpha;
        tAscentFactor = 1f / (45f - stallMinimaAlpha);
        sign = Mathf.Sign(maxCl);

        maxAlphaInvert = 1f / maxAlpha;
        stallDragDerivative = degree4Factor * 4f + degree3Factor * 3f + degree2Factor * 2f;
        stallDragDerivative *= Mathf.Abs(maxCl) * cdclAtPeak / maxAlpha;
        tAscentFactor90 = 1f / (90f - stallMinimaAlpha);
    }


    private float linearSlope;
    private float linearToPeakAlpha;
    private float Linear(float alpha)
    {
        return alpha * linearSlope + zeroCl;
    }

    private float peakFactor;
    private float Peak(float alpha)
    {
        return maxCl - Mathv.SmoothStart(alpha - maxAlpha, 2) * peakFactor;
    }
    private float Stall(float alpha)
    {
        float t = (alpha - maxAlpha) * stallOffsetAlphaInvert;
        return Mathf.Lerp(maxCl, maxCl * stallDepth, Mathv.SmoothStep(t, 2));
    }
    private float stallMinimaAlpha;
    private float tAscentFactor;
    private float PlateAscent(float alpha)
    {
        float t = (alpha - stallMinimaAlpha) * tAscentFactor;
        return Mathf.Lerp(maxCl * stallDepth, sign * flatMaxCl, Mathv.SmoothStep(t, 2));
    }
    private float PlateDescent(float alpha)
    {
        return sign * (flatMaxCl - Mathv.SmoothStart(alpha * one45Ratio - 1f, 2));
    }
    public float Cl(float alpha)
    {
        if (alpha < linearToPeakAlpha) return Linear(alpha);
        else if (alpha < maxAlpha) return Peak(alpha);
        else if (alpha < stallMinimaAlpha) return Stall(alpha);
        else if (alpha < 45f) return PlateAscent(alpha);
        else return PlateDescent(alpha);
    }

    const float degree4Factor = 1.5f;
    const float degree3Factor = -1.9f;
    const float degree2Factor = 1.4f;
    private float maxAlphaInvert;
    private float PreStall(float alpha)
    {
        float x = alpha * maxAlphaInvert;
        float polynomialTo1 = degree4Factor * Mathv.SmoothStart(x, 4) + degree3Factor * Mathv.SmoothStart(x, 3) + degree2Factor * Mathv.SmoothStart(x, 2);
        return polynomialTo1 * Mathf.Abs(maxCl) * cdclAtPeak + minCd;
    }
    private float stallDragDerivative;

    private float PostStall(float alpha)
    {
        float start = (alpha - maxAlpha) * stallDragDerivative + Mathf.Abs(maxCl) * cdclAtPeak + minCd;
        float end = (alpha - stallMinimaAlpha) * (stalledDrag - Mathf.Abs(maxCl) * cdclAtPeak - minCd) * stallOffsetAlphaInvert + stalledDrag;
        return Mathf.Lerp(start, end, (alpha - maxAlpha) * stallOffsetAlphaInvert);
    }
    private float tAscentFactor90;

    private float PlateLikeDrag(float alpha)
    {
        return Mathf.Lerp(stalledDrag, flatPlateMaxDrag, Mathv.SmoothStop((alpha - stallMinimaAlpha) * tAscentFactor90, 2));
    }

    public float Cd(float alpha)
    {
        if (alpha < maxAlpha) return PreStall(alpha);
        else if (alpha < stallMinimaAlpha) return PostStall(alpha);
        else return PlateLikeDrag(alpha);
    }
    public Vector2 Coefficients(float alpha)
    {
        return new Vector2(Cd(alpha), Cl(alpha));
    }
}