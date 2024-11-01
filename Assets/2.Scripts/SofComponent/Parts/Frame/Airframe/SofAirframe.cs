using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


public abstract class SofAirframe : SofFrame, IDamageTick, IMassComponent
{
    const float minimumHp = 10f;
    public override ModuleArmorValues Armor => ModulesHPData.DuraluminArmor;
    public virtual float Integrity => structureDamage;
    public override float MaxHp => area * HpPerSquareMeter + minimumHp;
    public abstract float HpPerSquareMeter { get; }


    public float area { get; protected set; }
    public SurfaceQuad quad { get; protected set; }


    public abstract IAirfoil Airfoil { get; }


    public override float ApproximateMass() { return Mathf.Pow(area, 1.5f); }
    public virtual float AreaCd() { return 0f; }
    public virtual void UpdateArea()
    {
        area = quad.Area;
    }
    public abstract void UpdateQuad();
    public virtual void UpdateAerofoil()
    {
        UpdateQuad();
        UpdateArea();
    }


    public override void SetReferences(SofComplex _complex)
    {
        base.SetReferences(_complex);
        UpdateAerofoil();
    }
    protected virtual void FixedUpdate()
    {
        if (!aircraft) ApplyForces();
    }

    public virtual float PropSpeedEffect() { return 0f; }

    public virtual Vector2 Coefficients(float angleOfAttack)
    {
        return Airfoil.Coefficients(angleOfAttack);
    }
    private float AngleOfAttack(SurfaceQuad q, Vector3 airflow)
    {
        Vector3 aeroDir = q.AeroDir(true);
        Vector3 projectedVel = Vector3.ProjectOnPlane(airflow, aeroDir);
        Vector3 chord = Vector3.ProjectOnPlane(q.ChordDir(true), aeroDir);

        return Vector3.SignedAngle(chord, projectedVel, aeroDir);
    }
    protected float ApplyForces()
    {
        Vector3 center = quad.CenterAero(true);
        Vector3 aeroDir = quad.AeroDir(true);
        Vector3 vel = rb.GetPointVelocity(center) + tr.root.forward * PropSpeedEffect();

        float alpha = AngleOfAttack(quad, vel);

        if (center.y <= 0f) return alpha;

        Vector2 coeffs = Coefficients(alpha);

        Vector3 force = Aerodynamics.Lift(vel, data.tas.Get, aeroDir, data.density.Get, quad.Area, coeffs.y, Integrity);
        force += Aerodynamics.Drag(vel, data.tas.Get, data.density.Get, quad.Area, coeffs.x, Integrity);
        lift = Vector3.Cross(vel, aeroDir).normalized;
        lift = lift.normalized * coeffs.y;

        data.rb.AddForceAtPosition(force, center);

        return alpha;
    }
    Vector3 lift;
    void Update()
    {
        Vector3 center = quad.CenterAero(true);
        Debug.DrawRay(center, lift, Color.red);
    }




#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (SofWindow.showAirframesOverlay)
        {
            Draw();
        }
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(SofAirframe)), CanEditMultipleObjects]
public class AirframeEditor : FrameEditor
{
    protected override string BasicName()
    {
        return "Airframe";
    }

    protected override void BasicFoldout()
    {
        base.BasicFoldout();

        SofAirframe frame = (SofAirframe)target;
        EditorGUILayout.LabelField("Area", frame.area.ToString("0.0") + " m²");
    }
}
#endif
