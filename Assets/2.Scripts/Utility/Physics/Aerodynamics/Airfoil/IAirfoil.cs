using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAirfoil
{
    public Vector2 Coefficients(float alpha);
    public Vector2 Coefficients(float alpha,float flaps);

    public float Gradient();
    public float Gradient(float flaps);

    public float MinCD();

    public float PeakAlpha();

}
