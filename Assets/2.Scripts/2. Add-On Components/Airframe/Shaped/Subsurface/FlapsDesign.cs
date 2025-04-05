using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "new Flaps Design", menuName = "SOF/Aircraft Modules/Flaps Design")]
public class FlapsDesign : ScriptableObject
{
    [SerializeField] private string description;
    [Range(0f,2f)]
    [SerializeField] private float liftCoefficientShift;
    [Range(0f, 0.2f)]
    [SerializeField] private float dragCoefficientShift;
    [Range(-10f, 10f)]
    [SerializeField] private float maxAlphaShift;

    public float ClShift(float flapFactor)
    {
        return Mathv.SmoothStop(flapFactor,2) * liftCoefficientShift;
    }
    public float CdShift(float flapFactor)
    {
        return flapFactor * dragCoefficientShift;
    }
    public float AlphaShift(float flapFactor)
    {
        return -flapFactor * maxAlphaShift;
    }

    public Vector2 ApplyFlapEffectToCoefficients(Vector2 coefficients, float flapFactor)
    {
        coefficients.x += CdShift(flapFactor);
        coefficients.y += ClShift(flapFactor);
        return coefficients;
    }
}
