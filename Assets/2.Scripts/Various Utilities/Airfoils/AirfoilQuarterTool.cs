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
    private float cdGrowth;

    private float sign;

    //Lift coefficient constants
    const float ltclf = 1.02f; //linearTargetClFactor
    const float stallDepth = 0.5f;
    const float stallOffsetAlpha = 6f;
    const float stallOffsetAlphaInvert = 1f / stallOffsetAlpha;
    const float flatMaxCl = 1f;
    const float one45Ratio = 1f / 45f;


    public AirfoilQuarterTool(float _maxCl, float _maxAlpha, float _zeroCl, float _minCd, float _cdGrowth)
    {
        maxCl = _maxCl;
        maxAlpha = _maxAlpha;
        zeroCl = _zeroCl;
        minCd = _minCd;
        cdGrowth = _cdGrowth;

        linearSlope = (maxCl * ltclf - zeroCl) / maxAlpha;
        linearToPeakAlpha = (maxAlpha * (maxCl * (ltclf - 2f) + zeroCl)) / (zeroCl - maxCl * ltclf);
        peakFactor = Mathv.SmoothStart(zeroCl - maxCl * ltclf, 2) / (4 * maxAlpha * maxAlpha * maxCl * (ltclf - 1));
        finalStallAlpha = maxAlpha + stallOffsetAlpha;
        tAscentFactor = 1f / (45f - finalStallAlpha);
        sign = Mathf.Sign(maxCl);

        float cl = Cl(maxAlpha);
        maxAlphaCd = PreStallDrag(cl);
        stalledCd = maxAlphaCd + 0.15f;
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
        return maxCl - M.Pow(alpha - maxAlpha, 2) * peakFactor;
    }
    private float Stall(float alpha)
    {
        float t = (alpha - maxAlpha) * stallOffsetAlphaInvert;
        return Mathf.Lerp(maxCl, maxCl * stallDepth, Mathv.SmoothStep(t, 2));
    }
    private float finalStallAlpha;
    private float tAscentFactor;
    private float PlateAscent(float alpha)
    {
        float t = (alpha - finalStallAlpha) * tAscentFactor;
        return Mathf.Lerp(maxCl * stallDepth, sign * flatMaxCl, Mathv.SmoothStep(t, 2));
    }
    private float PlateDescent(float alpha)
    {
        return sign * (flatMaxCl - M.Pow(alpha * one45Ratio - 1f, 2));
    }
    public float Cl(float alpha)
    {
        if (alpha < linearToPeakAlpha) return Linear(alpha);
        if (alpha < maxAlpha) return Peak(alpha);
        if (alpha < finalStallAlpha) return Stall(alpha);
        if (alpha < 45f) return PlateAscent(alpha);
        else return PlateDescent(alpha);
    }


    const float degree4Factor = 1.5f;
    const float degree3Factor = -1.9f;
    const float degree2Factor = 1.4f;

    const float flatPlateMaxDrag = 1.8f;
    const float clToCdPreStall = 0.03f;

    private float maxAlphaCd;
    private float stalledCd;
    private float PreStallDrag(float cl)
    {
        float polynomialTo1 = degree4Factor * M.Pow(cl, 4) + degree3Factor * M.AbsPow(cl, 3) + degree2Factor * M.Pow(cl, 2);
        return polynomialTo1 * clToCdPreStall * cdGrowth + minCd;
    }

    private float PostStallDrag(float alpha)
    {
        float t = (alpha - maxAlpha) * stallOffsetAlphaInvert;
        return Mathf.Lerp(maxAlphaCd, stalledCd, t);
    }

    private float PlateLikeDrag(float alpha)
    {
        float t = Mathv.InverseLerpUnclamped(finalStallAlpha, 90f, alpha);
        return Mathf.Lerp(stalledCd, flatPlateMaxDrag, Mathv.SmoothStop(t,2));
    }

    public float Cd(float alpha, float cl)
    {
        if (alpha < maxAlpha) return PreStallDrag(cl);
        if (alpha < finalStallAlpha) return PostStallDrag(alpha);
        return PlateLikeDrag(alpha);
    }
    public Vector2 Coefficients(float alpha)
    {
        float cl = Cl(alpha);
        return new Vector2(Cd(alpha,cl), cl);
    }
}