using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


public abstract class SofAirframe : SofModule,IDamageTick, IMassComponent
{
    public float EmptyMass => mass;
    public float LoadedMass => mass;

    public float mass;

    public float area { get; private set; }
    public Airfoil foil;
    public AirfoilSurface foilSurface;

    public bool vital = false;
    public SofModule[] ripOnRip;

    protected float stress = -1f;
    protected float randToughness = 1f;
    protected float floatLevel;

    public override ModuleArmorValues Armor => ModulesHPData.DuraluminArmor;

    public virtual float PropSpeedEffect() { return 0f; }
    public override bool Detachable => true;
    public virtual float MaxSpd() { return aircraft.maxSpeed; }
    public virtual float MaxG() { return aircraft.maxG * 1.5f; }
    public virtual float ApproximateMass() { return Mathf.Pow(area, 1.5f); }
    public virtual float AreaCd() { return 0f; }
    protected virtual Quad CreateQuad() { return null; }
    protected virtual AirfoilSurface CreateFoilSurface() { return new AirfoilSurface(this, CreateQuad(), foil); }

    public virtual float AirframeDamage => Mathv.SmoothStop(structureDamage, 2);

    public virtual void UpdateAerofoil()
    {
        foilSurface = CreateFoilSurface();
        area = foilSurface.Area();
    }
    public override void SetReferences(SofComplex _complex)
    {
        base.SetReferences(_complex);
        UpdateAerofoil();
    }
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        vital = (this.GetComponentInActualChildren<Wing>() || this.GetComponentInActualChildren<Stabilizer>());
        randToughness = Random.Range(0.8f, 1.3f);
        floatLevel = 10f;
    }
    protected virtual void FixedUpdate()
    {
        if (!aircraft) foilSurface.ApplyForces();
    }
    public override void Rip()
    {
        if (ripped) return;
        base.Rip();
        if (vital && aircraft) aircraft.destroyed = true;  //If vital down the airplane
        foreach (SofModule module in ripOnRip)
            if (module) module.Rip();           //Rip the assigned surface is there is one
        if (Detachable) Detach();
    }
    public void DamageTick(float dt)
    {
        if (!aircraft || !Detachable) return;
        float maxG = MaxG() * AirframeDamage;
        float maxSpd = MaxSpd() * AirframeDamage;
        if (stress <= -1f && Mathf.Abs(data.gForce) < maxG && data.ias.Get < maxSpd) return;    //If no stress and low g/speed no need to compute anything

        //Compute torque and stress
        float excessG = Mathf.Abs(data.gForce) - maxG;
        float excessSpeed = data.ias.Get - maxSpd;
        float gStress = excessG / maxG * 5f;
        float speedStress = excessSpeed / maxSpd * 10f;
        stress += Mathf.Max(gStress, speedStress) * dt;
        stress = Mathf.Clamp(stress, -1f, 5f);
        if (stress > 0f) DirectStructuralDamage(stress * 0.2f * randToughness * dt);
    }
    public void Floating()
    {
        Vector3 center = tr.position;
        if (center.y < 0f)
        {
            float displacementMultiplier = Mathf.Clamp(-center.y, 0f, 0.5f);
            float force = displacementMultiplier * LoadedMass * 10f * floatLevel;
            if (!aircraft) force /= 7f;
            rb.AddForceAtPosition(Vector3.up * force, center);
            floatLevel = Mathf.Max(floatLevel - Time.fixedDeltaTime / 12f, 1f);
        }
    }
    public virtual void Draw() { }
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
public class AirframeEditor : ModuleEditor
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
        EditorGUILayout.PropertyField(ripOnRip);

        base.BasicFoldout();

        SofAirframe frame = (SofAirframe)target;
        EditorGUILayout.LabelField("Area", frame.area.ToString("0.0") + " m²");
    }
}
#endif
