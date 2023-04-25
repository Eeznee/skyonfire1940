using System.Collections.Generic;
using UnityEngine;
public class Aero
{
    protected Transform tr;
    protected AirframeBase airframe;
    protected BoundedAirframe bounded;
    protected Airfoil airfoil;

    public ControlSurface control;
    public Flap flap;
    public Slat slat;

    public Quad quad;

    public Aero(AirframeBase _airframe, Quad _quad, Airfoil _sectionTest)
    {
        airframe = _airframe;
        bounded = airframe.GetComponent<BoundedAirframe>();
        tr = _quad.tr;
        quad = _quad;
        airfoil = _sectionTest;
        if (airfoil) airfoil.SendToCurve(-90f, 90f, 0.2f);
    }

    public virtual float Area()
    {
        return quad.Area;
    }
    public virtual Vector2 Coefficients(float aoa)
    {
        if (airfoil) return airfoil.Coefficients(aoa);
        if (bounded) return bounded.SimplifiedCoefficients(aoa);
        Debug.LogError("No Airfoil Profile Assigned", airframe);
        return Vector2.zero;
    }
    public virtual float Integrity()
    {
        return airframe.Integrity;
    }
    protected float ApplyForcesSpecificQuad(Quad q)
    {
        ObjectData data = airframe.data;
        Vector3 center = q.CenterAero(true);
        Vector3 velocity = airframe.rb.GetPointVelocity(center);
        float alpha = Vector3.SignedAngle(q.ChordDir(true), velocity, q.AeroDir(true));
        if (center.y <= 0f) return alpha;

        Vector2 coeffs = Coefficients(alpha);

        Vector3 force = Aerodynamics.ComputeLift(velocity, data.tas.Get, q.AeroDir(true), data.density.Get, q.Area, coeffs.y, Integrity());
        force += Aerodynamics.ComputeDrag(velocity, data.tas.Get, data.density.Get, q.Area, coeffs.x, Integrity());
        data.rb.AddForceAtPosition(force, center);

        return alpha;
    }
    public virtual float ApplyForces()
    {
        return ApplyForcesSpecificQuad(quad);
    }
}
/*
    if (section)
    {
        float flapAngle = 0f;
        float flapFraction = 0f;
        if (control)
        {
            flapAngle = control.controlAngle;
            flapFraction = control.miniFoil.tr.localScale.z / length;
        }
        if (flap)
        {
            flapAngle = flapsInput * -60f;
            flapFraction = flap.miniFoil.tr.localScale.z / length;
        }
        v.Compute(true, section, section.zeroAoA == 0f ? 2f : 5.6f, flapFraction, -flapAngle * Mathf.Deg2Rad);
        Vector3 coefs = section.Coefficients(alpha * Mathf.Deg2Rad, v);
        cl = coefs.x;
        cd = coefs.y;
        Debug.DrawRay(tr.TransformPoint(pos), cl * tr.up, Color.red);

        if (control) Debug.Log(frame.data.angleOfAttack.Get + ", " + alpha + ", " + controlSqrt * airfoil.gradient * control.sinControlAngle + ", " + section.testPlot.Evaluate(alpha));
    }
*/