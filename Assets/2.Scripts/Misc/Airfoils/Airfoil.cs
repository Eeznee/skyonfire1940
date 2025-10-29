using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Airfoil", menuName = "SOF/Aircraft Modules/Airfoil")]
public partial class Airfoil : ScriptableObject, IAirfoil
{
    [SerializeField] private bool symmetric = false;
    [SerializeField] private float zeroCl = 0f;
    [SerializeField] private float maxCl = 1.5f;
    [SerializeField] private float maxAlpha = 17f;
    [SerializeField] private float minAlpha = -14f;
    [SerializeField] private float minCd = 0.01f;
    [SerializeField] private float cdGrowth = 1f;

    [SerializeField] private float minCl;

    private AirfoilQuarterTool pos0to90;
    private AirfoilQuarterTool pos90to180;
    private AirfoilQuarterTool neg90to0;
    private AirfoilQuarterTool neg180to90;

    public float MaxCl => maxCl;
    public float HighPeakAlpha => maxAlpha;
    public float LowPeakAlpha => minAlpha;
    public float MinCD => minCd;

    public float GradientRadians() { return (maxCl - zeroCl) / (maxAlpha * Mathf.Deg2Rad); }

    public Vector2 Coefficients(float alpha)
    {
        alpha = Mathf.Repeat(alpha + 180f, 360f) - 180f;
        if (alpha < -90f) return neg180to90.Coefficients(alpha + 180f);
        if (alpha < 0f) return neg90to0.Coefficients(-alpha);
        if (alpha < 90f) return pos0to90.Coefficients(alpha);
        else return pos90to180.Coefficients(180f - alpha);
    }

    const float reverseClFactor = 0.5f;
    const float reverseMaxAlphaFactor = 0.65f;
    public void UpdateValues()
    {
        if(symmetric)
        {
            zeroCl = 0f;
            minAlpha = -maxAlpha;
        }

        float slope = (maxCl - zeroCl) / maxAlpha;
        float minCdAlpha = -zeroCl / slope;

        minCl = zeroCl + minAlpha * ((maxCl - zeroCl) / maxAlpha);
        pos0to90 = new AirfoilQuarterTool(maxCl, maxAlpha, zeroCl, minCd, cdGrowth);
        neg90to0 = new AirfoilQuarterTool(minCl, -minAlpha, zeroCl, minCd, cdGrowth);
        pos90to180 = new AirfoilQuarterTool(-maxCl * reverseClFactor, maxAlpha * reverseMaxAlphaFactor, zeroCl * reverseClFactor, minCd, cdGrowth);
        neg180to90 = new AirfoilQuarterTool(-minCl * reverseClFactor, -minAlpha * reverseMaxAlphaFactor, zeroCl * reverseClFactor, minCd, cdGrowth);

        float gradient = (maxCl - zeroCl) / (maxAlpha * Mathf.Deg2Rad);
    }
}
