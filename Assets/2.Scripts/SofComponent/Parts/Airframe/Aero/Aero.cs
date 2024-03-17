using System.Collections.Generic;
using UnityEngine;
public class Aero
{
    protected Transform tr;
    protected SofAirframe airframe;
    protected BoundedAirframe bounded;
    protected Airfoil airfoil;

    public ControlSurface control;
    public Flap flap;
    public Slat slat;

    public Quad quad;

    public Aero(SofAirframe _airframe, Quad _quad, Airfoil _sectionTest)
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
    protected float ApplyForcesSpecificQuad(Quad q, bool draw)
    {
        ObjectData data = airframe.data;
        Vector3 center = q.CenterAero(true);
        Vector3 velocity = airframe.rb.GetPointVelocity(center);
        //float alpha = Vector3.SignedAngle(q.ChordDir(true), projectedVelocity , q.AeroDir(true));
        Vector3 vTest = Vector3.ProjectOnPlane(velocity, q.AeroDir(true));
        Vector3 chordTest = Vector3.ProjectOnPlane(q.ChordDir(true), q.AeroDir(true));
        float alpha = Vector3.SignedAngle(chordTest, vTest, q.AeroDir(true));
        if (center.y <= 0f) return alpha;

        Vector2 coeffs = Coefficients(alpha);

        Vector3 force = Aerodynamics.ComputeLift(velocity, data.tas.Get, q.AeroDir(true), data.density.Get, q.Area, coeffs.y, Integrity());
        force += Aerodynamics.ComputeDrag(velocity, data.tas.Get, data.density.Get, q.Area, coeffs.x, Integrity());

        if (draw)
        {
            Debug.DrawRay(center, Vector3.Cross(velocity,q.AeroDir(true)).normalized * alpha);
            Debug.DrawRay(center, chordTest.normalized * 10f, Color.red);
            Debug.DrawRay(center, vTest.normalized * 10f, Color.yellow);
        }
        else
        {
            data.rb.AddForceAtPosition(force, center);
        }


        return alpha;
    }
    public virtual float ApplyForces()
    {
        return ApplyForcesSpecificQuad(quad,false);
    }
    public virtual void DrawForces()
    {
        ApplyForcesSpecificQuad(quad,true);
    }
}