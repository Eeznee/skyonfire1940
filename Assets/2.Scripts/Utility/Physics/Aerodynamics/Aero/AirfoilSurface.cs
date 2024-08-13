using System.Collections.Generic;
using UnityEngine;
public class AirfoilSurface
{
    protected Transform tr;
    protected SofAirframe airframe;
    protected BoundedAirframe bounded;
    protected Airfoil airfoil;

    public ControlSurface control;
    public Flap flap;
    public Slat slat;

    public Quad quad;

    public AirfoilSurface(SofAirframe _airframe, Quad _quad, Airfoil _airfoil)
    {
        airframe = _airframe;
        bounded = airframe.GetComponent<BoundedAirframe>();
        tr = _quad.tr;
        quad = _quad;
        airfoil = _airfoil;
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
        return airframe.structureDamage;
    }
    protected float ApplyForcesSpecificQuad(Quad q, bool draw)
    {
        ObjectData data = airframe.data;
        Vector3 center = q.CenterAero(true);
        Vector3 aeroDir = q.AeroDir(true);
        Vector3 vel = airframe.rb.GetPointVelocity(center) + airframe.tr.root.forward * airframe.PropSpeedEffect();
        Vector3 projectedVel = Vector3.ProjectOnPlane(vel, aeroDir);
        Vector3 chord = Vector3.ProjectOnPlane(q.ChordDir(true), aeroDir);

        float alpha = Vector3.SignedAngle(chord, projectedVel, aeroDir);

        if (center.y <= 0f) return alpha;

        Vector2 coeffs = Coefficients(alpha);

        Vector3 force = Aerodynamics.Lift(vel, data.tas.Get, aeroDir, data.density.Get, q.Area, coeffs.y, Integrity());
        force += Aerodynamics.Drag(vel, data.tas.Get, data.density.Get, q.Area, coeffs.x, Integrity());
        if (draw)
        {
            Debug.DrawRay(center, Vector3.Cross(vel, q.AeroDir(true)).normalized * alpha);
            Debug.DrawRay(center, chord.normalized * 10f, Color.red);
            Debug.DrawRay(center, projectedVel.normalized * 10f, Color.yellow);
        }
        data.rb.AddForceAtPosition(force, center);

        return alpha;
    }
    public virtual float ApplyForces()
    {
        return ApplyForcesSpecificQuad(quad, false);
    }
    public virtual void DrawForces()
    {
        ApplyForcesSpecificQuad(quad, true);
    }
}