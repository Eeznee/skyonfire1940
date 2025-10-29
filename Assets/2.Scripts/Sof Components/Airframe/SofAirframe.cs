using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SofAirframe : SofFrame, IDamageTick, IMassComponent
{
    public float area { get; protected set; }
    public SurfaceQuad quad { get; protected set; }
    public float angleOfAttack { get; protected set; }
    public float airSpeed { get; protected set; }

    public abstract IAirfoil Airfoil { get; }
    public abstract float HpPerSquareMeter { get; }
    public virtual float AerodynamicIntegrity => structureDamage;
    public override float MaxHp => area * HpPerSquareMeter + minimumHp;


    protected IAirfoil virtualAirfoil;



    const float minimumHp = 10f;



    public virtual float PropSpeedEffect() { return 0f; }
    public override float ApproximateMass() { return Mathf.Pow(area, 1.5f); }
    public virtual float AreaCd() { return 0f; }
    public virtual void UpdateArea()
    {
        area = quad.area;
    }
    public abstract void UpdateQuad();
    public virtual void UpdateAerofoil()
    {
        UpdateQuad();
        UpdateArea();
        virtualAirfoil = Airfoil;
    }
    public override void SetReferences(SofModular _complex)
    {
        base.SetReferences(_complex);
        UpdateAerofoil();
    }
    public virtual void ApplyForceDebris()
    {
        ForceAtPoint fap = CurrentForces();
        if (float.IsFinite(fap.force.x)) rb.AddForceAtPosition(fap.force, fap.point);
    }
    public virtual Vector2 SimulatedCoefficients(float angleOfAttack, float pointSpeed, AircraftAxes axes)
    {
        return Airfoil.Coefficients(angleOfAttack);
    }

    public float SimplifiedASIN(float sin)
    {
        return Mathf.Rad2Deg * (sin + M.Pow(sin, 3) * 0.16667f);
    }
    protected ForceAtPoint OLD(SurfaceQuad specificQuad, FlightConditions flightConditions)
    {
        Vector3 centerAero = specificQuad.centerAero.Pos(flightConditions);
        Vector3 aeroDir = specificQuad.aeroDir.Dir(flightConditions);

        Vector3 vel = flightConditions.PointVelocity(centerAero);
        vel += flightConditions.Forward * PropSpeedEffect();

        float speed = vel.magnitude;
        if (!flightConditions.fictionalConditions) airSpeed = speed;

        Vector3 projectedVel = Vector3.ProjectOnPlane(vel, aeroDir);

        Vector3 projectedChord = Vector3.ProjectOnPlane(specificQuad.chordDir.Dir(flightConditions), aeroDir);

        float aoa = Vector3.SignedAngle(projectedChord, projectedVel, aeroDir);

        Vector2 coeffs = SimulatedCoefficients(aoa, speed, flightConditions.axes);

        Vector3 lift = Aerodynamics.Lift(vel, speed, aeroDir, flightConditions.airDensity, area, coeffs.y, AerodynamicIntegrity);
        Vector3 drag = Aerodynamics.Drag(vel, speed, flightConditions.airDensity, area, coeffs.x, AerodynamicIntegrity);

        return new ForceAtPoint(lift + drag, centerAero);

    }
    public float AngleOfAttack(SurfaceQuad specificQuad, FlightConditions flightConditions)
    {
        Vector3 centerAero = specificQuad.centerAero.Pos(flightConditions);
        Vector3 upDir = specificQuad.upDir.Dir(flightConditions);
        Vector3 vel = flightConditions.PointVelocity(centerAero);
        float velMagnitude = vel.magnitude;

        return AngleOfAttack(upDir, vel, velMagnitude);
    }
    public float AngleOfAttack(FlightConditions flightConditions)
    {
        return AngleOfAttack(quad, flightConditions);
    }
    public float AngleOfAttack(Vector3 upDir, Vector3 velocity, float velocityMagnitude)
    {
        if (velocityMagnitude < 0.1f) return 0f;
        float sin = Mathf.Clamp(Vector3.Dot(velocity, upDir) / velocityMagnitude, -1f, 1f);
        return -Mathf.Asin(sin) * Mathf.Rad2Deg;
    }
    protected ForceAtPoint SimulatedForceOnQuad(SurfaceQuad specificQuad, FlightConditions flightConditions)
    {
        Vector3 centerAero = specificQuad.centerAero.Pos(flightConditions);
        Vector3 aeroDir = specificQuad.aeroDir.Dir(flightConditions);
        Vector3 upDir = specificQuad.upDir.Dir(flightConditions);

        Vector3 vel = flightConditions.PointVelocity(centerAero);
        vel += flightConditions.Forward * PropSpeedEffect();
        vel = Vector3.ProjectOnPlane(vel, aeroDir);
        float velMagnitude = vel.magnitude;
        if (velMagnitude < 1f) velMagnitude = 1f;

        float aoa = AngleOfAttack(upDir, vel, velMagnitude);
        Vector2 coeffs = SimulatedCoefficients(aoa, velMagnitude, flightConditions.axes);

        if (false && !flightConditions.fictionalConditions)
        {
            float cm = Cm(aoa);
            rb.AddTorque(cm * Vector3.Cross(upDir, vel).normalized * vel.sqrMagnitude * flightConditions.airDensity * area * 0.5f);
        }

        if (!flightConditions.fictionalConditions)
        {
            angleOfAttack = aoa;
            airSpeed = velMagnitude;
        }

        return new ForceAtPoint(Aerodynamics.LiftAndDrag(vel, velMagnitude, aeroDir, flightConditions.airDensity, area, coeffs, AerodynamicIntegrity), centerAero);
    }
    public float Cm(float alpha)
    {
        float constant = 0f;

        float sinFunc = -0.65f * Mathf.Sin(2f * alpha * Mathf.Deg2Rad);
        float t = Mathf.InverseLerp(Airfoil.HighPeakAlpha, Airfoil.HighPeakAlpha + 12f, alpha);
        return Mathf.Lerp(constant, sinFunc, t);
    }
    public virtual ForceAtPoint SimulatePointForce(FlightConditions flightConditions)
    {
        return SimulatedForceOnQuad(quad, flightConditions);
    }
    public ForceAtPoint CurrentForces()
    {
        return SimulatePointForce(new FlightConditions(sofModular, false));
    }

#if UNITY_EDITOR

    protected virtual bool ShowGUI => SofWindow.showFuselageOverlay;
    protected virtual void OnDrawGizmos()
    {
        if (ShowGUI && quad != null && quad.tr != null)
        {
            Draw();
        }
    }
#endif
}
