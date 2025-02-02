using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AirfoilSim
{
    public float zeroCl;
    public float maxCl;
    public float maxAlpha;
    public float minAlpha;
    [HideInInspector]public float minCl;

    public float minCd;

    private float gradient;

    public AirfoilQuarterTool pos0to90;
    public AirfoilQuarterTool pos90to180;
    public AirfoilQuarterTool neg90to0;
    public AirfoilQuarterTool neg180to90;

    const float reverseClFactor = 0.5f;
    const float reverseMaxAlphaFactor = 0.65f;

    public AirfoilSim(float _zeroCl, float _maxCl, float _maxAlpha, float _minAlpha, float _minCd)
    {
        zeroCl = _zeroCl;
        maxCl = _maxCl;
        maxAlpha = _maxAlpha;
        minAlpha = _minAlpha;
        minCd = _minCd;

        minCl = zeroCl + minAlpha * ((maxCl - zeroCl) / maxAlpha);
        pos0to90 = new AirfoilQuarterTool(maxCl, maxAlpha, zeroCl, minCd);
        neg90to0 = new AirfoilQuarterTool(minCl, -minAlpha, zeroCl, minCd);
        pos90to180 = new AirfoilQuarterTool(-maxCl * reverseClFactor, maxAlpha * reverseMaxAlphaFactor, zeroCl * reverseClFactor, minCd);
        neg180to90 = new AirfoilQuarterTool(-minCl * reverseClFactor, -minAlpha * reverseMaxAlphaFactor, zeroCl * reverseClFactor, minCd);

        gradient = (maxCl - zeroCl) / (maxAlpha * Mathf.Deg2Rad);
    }

    public void UpdateAirfoilQuarterTools()
    {
        minCl = zeroCl + minAlpha * ((maxCl - zeroCl) / maxAlpha);
        pos0to90 = new AirfoilQuarterTool(maxCl, maxAlpha, zeroCl, minCd);
        neg90to0 = new AirfoilQuarterTool(minCl, -minAlpha, zeroCl, minCd);
        pos90to180 = new AirfoilQuarterTool(-maxCl * reverseClFactor, maxAlpha * reverseMaxAlphaFactor, zeroCl * reverseClFactor, minCd);
        neg180to90 = new AirfoilQuarterTool(-minCl * reverseClFactor, -minAlpha * reverseMaxAlphaFactor, zeroCl * reverseClFactor, minCd);

        gradient = (maxCl - zeroCl) / (maxAlpha * Mathf.Deg2Rad);
    }

    public float Gradient()
    {
        return gradient;
    }
    public Vector2 Coefficients(float alpha)
    {
        alpha = Mathf.Repeat(alpha + 180f, 360f) - 180f;
        if (alpha < -90f) return neg180to90.Coefficients(alpha + 180f);
        if (alpha < 0f) return neg90to0.Coefficients(-alpha);
        if (alpha < 90f) return pos0to90.Coefficients(alpha);
        else return pos90to180.Coefficients(180f - alpha);
    }
}
