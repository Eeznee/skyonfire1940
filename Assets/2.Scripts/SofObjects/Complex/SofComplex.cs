using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SofComplex : SofObject
{
    public override int DefaultLayer()
    {
        return 9;
    }
    public ObjectLOD lod;
    public ObjectBubble bubble;
    public ObjectData data;
    public ObjectAudio avm;

    public event Action<SofComplex> onPartDetached;

    public List<SofComponent> components;
    public List<SofPart> parts;
    public List<SofModule> modules;
    public List<SofAirframe> airframes;
    public CrewMember[] crew;
    public override void SetReferences()
    {
        base.SetReferences();

        crew = GetComponentsInChildren<CrewMember>();

        components = new List<SofComponent>();
        parts = new List<SofPart>();
        modules = new List<SofModule>();
        airframes = new List<SofAirframe>();
        data = this.GetCreateComponent<ObjectData>();

        foreach (SofComponent component in GetComponentsInChildren<SofComponent>())
            component.SetReferencesAndRegister(this);
    }
    public void RegisterComponent(SofComponent component)
    {
        components.Add(component);
        SofPart part = component as SofPart;
        if (part) parts.Add(part);
        SofModule module = component as SofModule;
        if (module) modules.Add(module);
        SofAirframe airframe = component as SofAirframe;
        if (airframe) airframes.Add(airframe);
    }
    public void RemoveComponent(SofComponent component)
    {
        components.Remove(component);
        SofPart part = component as SofPart;
        if (part) parts.Remove(part);
        SofModule module = component as SofModule;
        if (module) modules.Remove(module);
        SofAirframe airframe = component as SofAirframe;
        if (airframe) airframes.Remove(airframe);
    }
    protected override void Initialize()
    {
        if (aircraft)
        {
            if (!simpleDamage) bubble = tr.CreateChild("Object Bubble").gameObject.AddComponent<ObjectBubble>();
            lod = GetComponentInChildren<ObjectLOD>();
            if (!lod) lod = tr.CreateChild("LOD Manager").gameObject.AddComponent<ObjectLOD>();
        }
        avm = transform.CreateChild("Audio Visual Manager").gameObject.AddComponent<ObjectAudio>();

        base.Initialize();

        foreach (SofComponent component in components.ToArray())
            component.InitializeComponent(this);

        SetMassFromParts();
        SetRigidbody();
    }
    private void SetRigidbody()
    {
        if (rb == GameManager.gm.mapRb) return;
        rb.angularDrag = 0f;
        rb.drag = 0f;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Extrapolate;
    }

    public float cogForwardDistance = 0f;
    public float targetEmptyMass = 3000f;

    private Mass mass;
    public float GetMass() { return mass.mass; }
    public Vector3 GetCenterOfMass() { return mass.center; }
    public void ShiftMass(float shift) { mass.mass += shift; UpdateRbMass(false); }
    public void ShiftMass(Mass shift) { mass += shift; UpdateRbMass(true); }
    public void UpdateRbMass(bool centerOfMass) { complex.rb.mass = mass.mass; if (centerOfMass) complex.rb.centerOfMass = mass.center; }
    public void SetMassFromParts()
    {
        mass = new Mass(0f, Vector3.zero);
        foreach (SofPart part in parts)
            mass += new Mass(part, false);

        UpdateRbMass(true);
        ResetInertiaTensor();
    }
    public void ResetInertiaTensor()
    {
        if (aircraft != null)
        {
            Vector3 inertiaTensor = Mass.InertiaMoment(complex.parts.ToArray(), true);
            inertiaTensor *= 1.1f;
            complex.rb.inertiaTensor = inertiaTensor;
        }
        else complex.rb.ResetInertiaTensor();
    }
    const int damageTickCycle = 30;
    private int damageTickFrameCount = 0;
    protected void DamageTickFixedUpdate()
    {
        if (++damageTickFrameCount >= damageTickCycle)
        {
            foreach (SofModule module in modules.ToArray())
                module.DamageTick(Time.fixedDeltaTime * damageTickCycle);
            damageTickFrameCount = 0;
        }
    }
    public Collider Bubble()
    {
        if (bubble) return bubble.bubble;
        return null;
    }
    public override void Explosion(Vector3 center, float tnt)
    {
        base.Explosion(center, tnt);
        if (!bubble) return;
        float sqrDis = (center - transform.position).sqrMagnitude;
        if (tnt * 2000f < sqrDis) return;   //no calculations if too far
        foreach (SofModule m in modules.ToArray()) if (m) m.ExplosionDamage(center, tnt);
    }
    public void OnPartDetach(SofComplex detachedDebris)
    {
        ShiftMass(new Mass(-detachedDebris.mass.mass, detachedDebris.mass.center));
        if (mass.mass <= 0f) Debug.LogError("Mass below zero", gameObject);

        foreach (SofComponent component in detachedDebris.components) RemoveComponent(component);

        onPartDetached?.Invoke(detachedDebris);
    }

    private bool submerged = false;
    protected void WaterPhysics()
    {
        if (data.altitude.Get < 5f) foreach (SofAirframe airframe in airframes) if (airframe) airframe.Floating();

        bool newSubmerged = data.altitude.Get < 1f;
        if (newSubmerged != submerged)
        {
            submerged = newSubmerged;
            rb.angularDrag = submerged ? 0.5f : 0f;
            rb.drag = submerged ? 1f : 0f;
        }
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(SofComplex))]
public class SofComplexEditor : SofObjectEditor
{
    SerializedProperty targetEmptyMass;
    SerializedProperty cogForwardDis;

    public Mass emptyMass;
    public Mass loadedMass;

    private void UpdateMassGUI()
    {
        SofComplex complex = (SofComplex)target;
        SofPart[] parts = complex.parts.ToArray();
        emptyMass = new Mass(parts, true);
        loadedMass = new Mass(parts, false);
    }
    protected virtual void OnEnable()
    {
        targetEmptyMass = serializedObject.FindProperty("targetEmptyMass");
        cogForwardDis = serializedObject.FindProperty("cogForwardDistance");

        SofComplex complex = (SofComplex)target;
        complex.SetReferences();
        UpdateMassGUI();

        serializedObject.ApplyModifiedProperties();
    }

    static bool showMass = true;
    static bool showAutoMass = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        SofComplex complex = (SofComplex)target;

        showMass = EditorGUILayout.Foldout(showMass, "Mass Infos", true, EditorStyles.foldoutHeader);
        if (showMass)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Empty Mass", emptyMass.mass.ToString("0.00") + " kg");
            EditorGUILayout.LabelField("Loaded Mass", loadedMass.mass.ToString("0.00") + " kg");
            EditorGUILayout.LabelField("Empty COG", emptyMass.center.ToString("F2"));
            EditorGUILayout.LabelField("Loaded COG", loadedMass.center.ToString("F2"));

            EditorGUI.indentLevel--;
        }

        showAutoMass = EditorGUILayout.Foldout(showAutoMass, "Auto Mass Tool", true, EditorStyles.foldoutHeader);
        if (showAutoMass)
        {
            EditorGUI.indentLevel++;


            EditorGUILayout.HelpBox("Set a target mass and center of gravity position (Z axis) then press the button. A new mass will be assigned to each airframe to match desired values.", MessageType.Info);

            EditorGUILayout.PropertyField(targetEmptyMass);
            EditorGUILayout.PropertyField(cogForwardDis);

            if (GUILayout.Button("Compute new parts mass to match target mass & COG"))
            {
                Mass.ComputeAutoMass(complex, new Mass(complex.targetEmptyMass, Vector3.forward * complex.cogForwardDistance));
                UpdateMassGUI();
            }

            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif