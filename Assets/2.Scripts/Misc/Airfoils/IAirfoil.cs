using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAirfoil
{

    public Vector2 Coefficients(float alpha);

    public Vector2 Coefficients(float alpha,FlapsDesign flapsDesign,float flapsFactor)
    {
        if (flapsDesign == null) return Coefficients(alpha);

        Vector2 coeffs = Coefficients(alpha + flapsDesign.AlphaShift(flapsFactor));
        coeffs = flapsDesign.ApplyFlapEffectToCoefficients(coeffs, flapsFactor);

        return coeffs;
    }

    public float GradientRadians();
    public float GradientDegrees()
    {
        return GradientRadians() * Mathf.Deg2Rad;
    }

    public float MinCD { get; }

    public float HighPeakAlpha { get; }

    public float LowPeakAlpha { get; }

}
