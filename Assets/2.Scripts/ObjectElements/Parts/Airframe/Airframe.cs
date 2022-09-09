using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Airframe : Part
{
    //Attachements and links
    public Part[] ripOnRip;
    public bool vital = false;
    public bool detachable = true;

    //Damage model
    public float area = 5f;

    //Forces Model
    public bool buoyancy = false;
    public float maxG;
    protected float stress = -1f;
    protected float randToughness = 1f;
    protected float floatLevel;

    public virtual float MaxSpeed()
    {
        return aircraft ? aircraft.maxSpeed : 500f;
    }

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime && aircraft)
        {
            maxG = aircraft.maxG * 1.5f;
            randToughness = Random.Range(0.8f, 1.3f);
            maxHp = hp = area * material.hpPerSq;
            floatLevel = 10f;
        }
    }
    public void ForcesStress(bool g, bool spd)
    {
        if (ripped || !detachable) return;

        //Compute torque and stress
        float damageCoeff = Mathf.Sqrt(Mathf.Abs(structureDamage));
        float excessG = g ? Mathf.Abs(data.gForce) - maxG * damageCoeff : 0f;
        float excessSpeed = spd ? data.ias - MaxSpeed() * damageCoeff : 0f;
        stress += Mathf.Max(excessG / maxG * 5f * Time.fixedDeltaTime, excessSpeed / MaxSpeed() * 10f * Time.fixedDeltaTime);
        stress = Mathf.Clamp(stress, -1f, 5f);
        if (stress > 0f) structureDamage -= stress / (5f * randToughness) * Time.fixedDeltaTime;

        //Rip if structure is too damaged
        if (structureDamage < 0f) Rip();
    }
    public void Floating(Vector3 center)
    {
        if (transform.position.y < 0f)
        {
            float displacementMultiplier = Mathf.Clamp01(-transform.position.y);
            float force = displacementMultiplier * Mass() * 10f * floatLevel;
            if (!aircraft) force /= 7f;
            rb.AddForceAtPosition(Vector3.up * force, center);
            floatLevel = Mathf.Max(floatLevel - Time.fixedDeltaTime / 12f, 1f);
        }
    }
    private void FixedUpdate()
    {
        ForcesStress(true, false);
        if (buoyancy) Floating(transform.position);
    }
    public override void Rip()
    {
        if (ripped) return;
        base.Rip();
        if (vital && aircraft) aircraft.destroyed = true;  //If vital down the airplane
        foreach (Airframe airf in ripOnRip)
        {
            if (airf) airf.Rip();                          //Rip the assigned surface is there is one
        }
        if (detachable) Detach();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Airframe))]
public class AirframeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //
        Fuselage frame = (Fuselage)target;

        frame.area = EditorGUILayout.FloatField("One Side Area m²", frame.area);
        frame.emptyMass = EditorGUILayout.FloatField("Mass kg", frame.emptyMass);
        frame.buoyancy = EditorGUILayout.Toggle("Buoyancy", frame.buoyancy);
        frame.detachable = EditorGUILayout.Toggle("Rippable", frame.detachable);
        frame.material = EditorGUILayout.ObjectField("Material", frame.material, typeof(PartMaterial), false) as PartMaterial;
        SerializedProperty ripOnRip = serializedObject.FindProperty("ripOnRip");
        EditorGUILayout.PropertyField(ripOnRip, true);
        frame.brokenModel = EditorGUILayout.ObjectField("Broken Model", frame.brokenModel, typeof(GameObject), true) as GameObject;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(frame);
            EditorSceneManager.MarkSceneDirty(frame.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
