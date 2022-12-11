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

    //Damage model
    public float area = 5f;

    //Forces Model
    public float maxG;
    protected float stress = -1f;
    protected float randToughness = 1f;
    protected float floatLevel;

    public virtual float MaxSpeed()
    {
        return aircraft ? aircraft.maxSpeed : 500f;
    }
    public virtual bool Detachable()
    {
        return true;
    }
    public Vector3 Bounds()
    {
        Vector3 bounds = Vector3.zero;
        if (GetComponent<MeshCollider>())
            bounds = GetComponent<MeshCollider>().sharedMesh.bounds.size;
        else if (GetComponent<BoxCollider>())
            bounds = GetComponent<BoxCollider>().bounds.size;
        else if (GetComponent<MeshFilter>())
            bounds = GetComponent<MeshFilter>().sharedMesh.bounds.size;
        return bounds;
    }
    public virtual float RecalculateArea()
    {
        float a = area;
        Vector3 bounds = Bounds();
        if (bounds != Vector3.zero) a = bounds.z * Mathf.PI / Mathf.Sqrt(2f) * Mathf.Sqrt(bounds.x * bounds.x + bounds.y * bounds.y);
        return a;
    }
    public virtual float AutoMassCoefficient()
    {
        return RecalculateArea();
    }

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime && aircraft)
        {
            vital = !(GetComponent<Airfoil>() || GetComponent<Stabilizer>()) && (GetComponentInChildren<Airfoil>() || GetComponentInChildren<Stabilizer>());
            maxG = aircraft.maxG * 1.5f;
            randToughness = Random.Range(0.8f, 1.3f);
            maxHp = hp = area * material.hpPerSq;
            floatLevel = 10f;
        }
    }
    public void ForcesStress(bool g, bool spd)
    {
        if (ripped || !Detachable()) return;

        //Compute torque and stress
        float damageCoeff = Mathv.SmoothStop(StructureIntegrity(),2);
        float excessG = g ? Mathf.Abs(data.gForce) - maxG * damageCoeff : 0f;
        float excessSpeed = spd ? data.ias - MaxSpeed() * damageCoeff : 0f;
        stress += Mathf.Max(excessG / maxG * 5f * Time.fixedDeltaTime, excessSpeed / MaxSpeed() * 10f * Time.fixedDeltaTime);
        stress = Mathf.Clamp(stress, -1f, 5f);
        if (stress > 0f) structureDamage -= stress / (5f * randToughness) * Time.fixedDeltaTime;

        //Rip if structure is too damaged
        if (structureDamage < 0f) Rip();
    }
    public void Floating()
    {
        if (data.altitude > 10f) return;
        Vector3 center = transform.position;
        if (center.y < 0f)
        {
            float displacementMultiplier = Mathf.Clamp(-center.y,0f,0.5f);
            float force = displacementMultiplier * Mass() * 10f * floatLevel;
            if (!aircraft) force /= 7f;
            rb.AddForceAtPosition(Vector3.up * force, center);
            floatLevel = Mathf.Max(floatLevel - Time.fixedDeltaTime / 12f, 1f);
        }
    }
    private void FixedUpdate()
    {
        ForcesStress(true, false);
        Floating();
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
        if (Detachable()) Detach();
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
        Airframe frame = (Airframe)target;

        EditorGUILayout.LabelField("Auto Area", frame.RecalculateArea().ToString("0.0"));
        frame.area = EditorGUILayout.FloatField("One Side Area m²", frame.area);
        frame.emptyMass = EditorGUILayout.FloatField("Mass kg", frame.emptyMass);
        frame.material = EditorGUILayout.ObjectField("Material", frame.material, typeof(PartMaterial), false) as PartMaterial;
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
