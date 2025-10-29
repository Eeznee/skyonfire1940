using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public abstract class SofFrame : SofModule, IDamageTick, IMassComponent
{
    public float EmptyMass => mass;
    public float LoadedMass => mass;
    public float RealMass => mass;

    public float mass;

    public bool vital { get; private set; }


    public SofModule[] ripOnRip;

    protected float stress = -1f;
    protected float randToughness = 1f;
    protected float floatLevel;

    public override ModuleArmorValues Armor(Collider collider) { return ModulesHPData.DuraluminArmor; }

    private FrameCustomRipSpeed customRipSpeed;

    public override bool Detachable => true;
    public virtual float MaxSpd => aircraft.SpeedLimitMps;
    public virtual float MaxG => aircraft.MaxGForce * 1.5f;

    public virtual float StructuralIntegrity => structureDamage;
    public abstract float ApproximateMass();

    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);
        vital = (this.GetComponentInActualChildren<Wing>() || this.GetComponentInActualChildren<Stabilizer>());
        randToughness = Random.Range(0.8f, 1.3f);
        floatLevel = 10f;
    }
    public void CreateCustomRipSpeed(float _ripSpeedMps, HydraulicSystem _hydraulics)
    {
        customRipSpeed = new FrameCustomRipSpeed(_ripSpeedMps, _hydraulics);
    }
    public override void Rip()
    {
        if (ripped) return;
        base.Rip();
        if (vital && aircraft) aircraft.Destroy();  //If vital down the airplane
        foreach (SofModule module in ripOnRip)
            if (module) module.Rip();           //Rip the assigned surface is there is one
        if (Detachable) DetachAndCreateDebris();
    }
    public void DamageTick(float dt)
    {
        if (!aircraft || !Detachable) return;
        float maxG = MaxG * StructuralIntegrity;
        float maxSpd = (customRipSpeed != null ? customRipSpeed.MaxSpeed(this) : MaxSpd) * StructuralIntegrity;
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
            float force = displacementMultiplier * RealMass * 10f * floatLevel;
            if(rb.linearVelocity.y > 1f) force /= rb.linearVelocity.y;
            if (!aircraft) force /= 7f;
            rb.AddForceAtPosition(Vector3.up * force, center);
            floatLevel = Mathf.Max(floatLevel - Time.fixedDeltaTime / 12f, 1f);
        }
    }
    protected Bounds GetBounds()
    {
        MeshCollider mesh = GetComponent<MeshCollider>();
        if (mesh && mesh.sharedMesh) return mesh.sharedMesh.bounds;

        BoxCollider box = GetComponent<BoxCollider>();
        if (box) return new Bounds(box.center, box.size);

        MeshFilter visualMesh = GetComponent<MeshFilter>();
        if (visualMesh && visualMesh.sharedMesh) return visualMesh.sharedMesh.bounds;

        return new Bounds(Vector3.zero, Vector3.zero);
    }
    public virtual void Draw() { }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (SofWindow.showFuselageOverlay)
        {
            Draw();
        }
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(SofFrame)), CanEditMultipleObjects]
public class FrameEditor : ModuleEditor
{
    SerializedProperty ripOnRip;
    protected override void OnEnable()
    {
        base.OnEnable();
        ripOnRip = serializedObject.FindProperty("ripOnRip");
    }
    protected override string BasicName()
    {
        return "Frame";
    }

    protected override void BasicFoldout()
    {
        SofFrame frame = (SofFrame)target;

        EditorGUILayout.PropertyField(ripOnRip);

        base.BasicFoldout();
    }
}
#endif
