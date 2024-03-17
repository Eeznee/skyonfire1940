using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


public abstract class SofAirframe : SofModule
{
    public float area = 5f;
    public Airfoil foil;
    public Aero aero;

    public bool vital = false;
    public SofModule[] ripOnRip;

    protected float left = 1f;
    protected float stress = -1f;
    protected float randToughness = 1f;
    protected float floatLevel;

    public override bool Detachable() { return true; }
    public virtual float MaxSpd() { return aircraft.maxSpeed; }
    public virtual float MaxG() { return aircraft.maxG * 1.5f; }
    public virtual float ApproximateMass() { return Mathf.Pow(area, 1.5f); }
    public virtual float AreaCd() { return 0f; }
    protected virtual Quad CreateQuad() { return null; }
    protected virtual Aero CreateAero() { return new Aero(this, CreateQuad(), foil); }

    protected virtual void Awake()
    {
        aero = null;
    }
    public virtual void CalculateAerofoilStructure()
    {
        aero = CreateAero();
        area = aero.Area();
    }
    public override void SetReferences(SofComplex _complex)
    {
        base.SetReferences(_complex);
        CalculateAerofoilStructure();
    }
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        vital = (this.GetComponentInActualChildren<Wing>() || this.GetComponentInActualChildren<Stabilizer>());
        randToughness = Random.Range(0.8f, 1.3f);
        floatLevel = 10f;
        maxHp = area * material.hpPerSq;
    }
    protected virtual void FixedUpdate()
    {
        if (!aircraft) aero.ApplyForces();
    }
    public override void Rip()
    {
        if (ripped) return;
        base.Rip();
        if (vital && aircraft) aircraft.destroyed = true;  //If vital down the airplane
        foreach (SofModule module in ripOnRip)
            if (module) module.Rip();           //Rip the assigned surface is there is one
        if (Detachable()) Detach();
    }
    public override void DamageTick(float dt)
    {
        base.DamageTick(dt);

        if (!aircraft || !Detachable()) return;
        float damageCoeff = Mathv.SmoothStop(StructureIntegrity(), 2);
        float maxG = MaxG() * damageCoeff;
        float maxSpd = MaxSpd() * damageCoeff;
        if (stress <= -1f && Mathf.Abs(data.gForce) < maxG && data.ias.Get < maxSpd) return;    //If no stress and low g/speed no need to compute anything

        //Compute torque and stress
        float excessG = Mathf.Abs(data.gForce) - maxG;
        float excessSpeed = data.ias.Get - maxSpd;
        float gStress = excessG / maxG * 5f;
        float speedStress = excessSpeed / maxSpd * 10f;
        stress += Mathf.Max(gStress, speedStress) * dt;
        stress = Mathf.Clamp(stress, -1f, 5f);
        if (stress > 0f) DamageIntegrity(stress * 0.2f * randToughness * dt);
    }
    public void Floating()
    {
        Vector3 center = tr.position;
        if (center.y < 0f)
        {
            float displacementMultiplier = Mathf.Clamp(-center.y, 0f, 0.5f);
            float force = displacementMultiplier * Mass() * 10f * floatLevel;
            if (!aircraft) force /= 7f;
            rb.AddForceAtPosition(Vector3.up * force, center);
            floatLevel = Mathf.Max(floatLevel - Time.fixedDeltaTime / 12f, 1f);
        }
    }

#if UNITY_EDITOR
    protected virtual void Draw() { aero.quad.Draw(new Color(), Color.yellow, false); }

    private void OnValidate()
    {
        CalculateAerofoilStructure();
    }
    private void OnDrawGizmos()
    {
        Draw();
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(SofAirframe)), CanEditMultipleObjects]
public class AirframeEditor : PartEditor
{
    SerializedProperty ripOnRip;
    protected override void OnEnable()
    {
        base.OnEnable();
        ripOnRip = serializedObject.FindProperty("ripOnRip");
    }
    protected override string BasicName()
    {
        return "Airframe";
    }
    protected override void BasicFoldout()
    {
        base.BasicFoldout();

        SofAirframe frame = (SofAirframe)target;

        EditorGUILayout.PropertyField(ripOnRip);
        EditorGUILayout.LabelField("Area", frame.area.ToString("0.0") + " m²");
        ModuleMaterial material = frame.aircraft.materials.Material(frame);
        float hp = material.hpPerSq * frame.area;
        EditorGUILayout.LabelField("HP", hp.ToString("0") + " HP");
    }
}
#endif
