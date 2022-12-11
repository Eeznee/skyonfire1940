using System;
using UnityEngine;


public class AeroSurface : MonoBehaviour
{
    [SerializeField] AeroSurfaceConfig config = null;
    public bool IsControlSurface;
    public float InputMultiplyer = 1;

    private float flapAngle;

    public void SetFlapAngle(float angle)
    {
        flapAngle = Mathf.Clamp(angle, -Mathf.Deg2Rad * 50, Mathf.Deg2Rad * 50);
    }

    public BiVector3 CalculateForces(Vector3 worldAirVelocity, float airDensity, Vector3 relativePosition)
    {
        BiVector3 forceAndTorque = new BiVector3();
        if (!gameObject.activeInHierarchy || config == null) return forceAndTorque;

        // Calculating air velocity relative to the surface's coordinate system.
        // Z component of the velocity is discarded
        Vector3 airVelocity = transform.InverseTransformDirection(worldAirVelocity);
        airVelocity = new Vector3(airVelocity.x, airVelocity.y);
        Vector3 dragDirection = transform.TransformDirection(airVelocity.normalized);
        Vector3 liftDirection = Vector3.Cross(dragDirection, transform.forward);

        float area = config.chord * config.span;
        float dynamicPressure = 0.5f * airDensity * airVelocity.sqrMagnitude;
        float angleOfAttack = Mathf.Atan2(airVelocity.y, -airVelocity.x);

        Vector3 aerodynamicCoefficients = CalculateCoefficients(angleOfAttack, flapAngle);

        Vector3 lift = liftDirection * aerodynamicCoefficients.x * dynamicPressure * area;
        Vector3 drag = dragDirection * aerodynamicCoefficients.y * dynamicPressure * area;
        Vector3 torque = -transform.forward * aerodynamicCoefficients.z * dynamicPressure * area * config.chord;

        forceAndTorque.p += lift + drag;
        forceAndTorque.q += Vector3.Cross(relativePosition, forceAndTorque.p);
        forceAndTorque.q += torque;

        return forceAndTorque;
    }
    private Vector3 CalculateCoefficients(float aoa, float flapAngle)
    {
        Vector3 flapEffect = config.FlapEffect(flapAngle);
        return Vector3.zero;// CalculateCoefficients(aoa, flapEffect.x, flapEffect.y, flapEffect.z);
    }
}
