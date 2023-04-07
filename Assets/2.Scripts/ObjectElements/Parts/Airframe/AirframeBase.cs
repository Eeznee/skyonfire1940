using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


public abstract class AirframeBase : Module
{
    public float area = 5f;
    public Airfoil foil;
    public Aero aero;

    public bool vital = false;
    public Module[] ripOnRip;

    protected float left = 1f;
    protected float stress = -1f;
    protected float randToughness = 1f;
    protected float floatLevel;

    public override bool Detachable() { return true; }
    public virtual float MaxSpd() { return aircraft.maxSpeed; }
    public virtual float MaxG() { return aircraft.maxG * 1.5f; }
    public virtual float ApproximateMass() { return Mathf.Pow(area, 1.5f); }
    protected virtual Quad CreateQuad() { return null; }
    protected virtual Aero CreateAero() { return new Aero(this, CreateQuad(), foil); }
    public virtual void CalculateAerofoilStructure()
    {
        aero = CreateAero();
        area = aero.Area();
    }
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime && aircraft)
        {
            vital = !(GetComponentInChildren<Wing>() || GetComponentInChildren<Stabilizer>());
            randToughness = Random.Range(0.8f, 1.3f);
            floatLevel = 10f;
            CalculateAerofoilStructure();
            maxHp = area * material.hpPerSq;
        }
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
        foreach (Module module in ripOnRip)
            if (module) module.Rip();           //Rip the assigned surface is there is one
        if (Detachable()) Detach();
    }
    public void ForcesStress()
    {
        if (!aircraft || !Detachable()) return;
        float damageCoeff = Mathv.SmoothStop(StructureIntegrity(), 2);
        float maxG = MaxG() * damageCoeff;
        float maxSpd = MaxSpd() * damageCoeff;
        if (stress <= -1f && Mathf.Abs(data.gForce) < maxG && data.ias < maxSpd) return;    //If no stress and low g/speed no need to compute anything
        //Compute torque and stress
        float excessG = Mathf.Abs(data.gForce) - maxG;
        float excessSpeed = data.ias - maxSpd;
        float gStress = excessG / maxG * 5f;
        float speedStress = excessSpeed / maxSpd * 10f;
        stress += Mathf.Max(gStress, speedStress) * Time.fixedDeltaTime;
        stress = Mathf.Clamp(stress, -1f, 5f);
        if (stress > 0f) DamageIntegrity(stress * 0.2f * randToughness * Time.fixedDeltaTime);

        //Rip if structure is too damaged
        if (Integrity < 0f) Rip();
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
    protected virtual void Draw() { aero.quad.Draw(new Color(), Color.yellow,false); }
    private void OnDrawGizmos()
    {
        CalculateAerofoilStructure();
        Draw();
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(AirframeBase))]
public class AirframeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Color backgroundColor = GUI.backgroundColor;
        serializedObject.Update();

        AirframeBase frame = (AirframeBase)target;
        frame.CalculateAerofoilStructure();

        GUI.color = Color.green;
        EditorGUILayout.HelpBox("Airframe", MessageType.None);
        GUI.color = backgroundColor;

 
        frame.emptyMass = EditorGUILayout.FloatField("Empty Mass", frame.emptyMass);
        EditorGUILayout.LabelField("Area", frame.area.ToString("0.0") + " m²");
        frame.material = EditorGUILayout.ObjectField("Material", frame.material, typeof(ModuleMaterial), false) as ModuleMaterial;
        SerializedProperty ripOnRip = serializedObject.FindProperty("ripOnRip");
        EditorGUILayout.PropertyField(ripOnRip, true);


        if (GUI.changed)
        {
            EditorUtility.SetDirty(frame);
            EditorSceneManager.MarkSceneDirty(frame.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
