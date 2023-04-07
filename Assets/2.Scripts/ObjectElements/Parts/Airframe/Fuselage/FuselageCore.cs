using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class FuselageCore : Fuselage
{
    public float oldCd = 0.01f;
    public float cd = 0.01f;
    public List<Fuselage> linkedFuselages;
    public bool detachable = false;

    const float attachedMaxOffset = 1.5f;
    const float maxCd = 1.5f;
    const float maxCl = 0.4f;

    public float CombinedArea()
    {
        float areas = 0f;
        foreach (Fuselage fus in linkedFuselages) if (fus.data == data) areas += fus.area;
        return areas;
    }
    public override void CalculateAerofoilStructure()
    {
        base.CalculateAerofoilStructure();

        linkedFuselages = new List<Fuselage>();
        foreach(Fuselage fus in transform.root.GetComponentsInChildren<Fuselage>())
        {
            Vector3 diff = transform.InverseTransformPoint(fus.transform.position);
            if (Mathf.Abs(diff.x) < attachedMaxOffset) linkedFuselages.Add(fus);
        }
    }
    public override bool Detachable()
    {
        return detachable;
    }
    protected override void FixedUpdate()
    {
        if (aircraft) ExcessDrag();

        float areas = CombinedArea();
        Vector2 coefs = Aerodynamics.SimpleCoefficients(data.angleOfSlip, maxCl, cd, maxCd);

        Vector3 lift = Aerodynamics.ComputeLift(rb.velocity, data.tas, transform.up, data.airDensity, areas, coefs.y, StructureIntegrity()); 
        Vector3 drag = Aerodynamics.ComputeDrag(rb.velocity, data.tas, data.airDensity, areas, coefs.x, StructureIntegrity());

        rb.AddForce(lift + drag);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(FuselageCore))]
public class FuselageCoreEditor : FuselageEditor
{
    public override void OnInspectorGUI()
    {
        Color backgroundColor = GUI.backgroundColor;
        base.OnInspectorGUI();

        GUILayout.Space(20f);
        GUI.color = Color.red;
        EditorGUILayout.HelpBox("Fuselage Core", MessageType.None); //Airfoil shape configuration
        GUI.color = backgroundColor;

        FuselageCore fuselage = (FuselageCore)target;
        fuselage.oldCd = EditorGUILayout.FloatField("Old Cd",fuselage.oldCd);
        fuselage.cd = EditorGUILayout.FloatField("Cd", fuselage.cd);
        fuselage.detachable = EditorGUILayout.Toggle("Detachable", fuselage.detachable);
        EditorGUILayout.LabelField("Complete Fuselage Area", fuselage.CombinedArea().ToString("0.0") + " m²");

        if (GUI.changed)
        {
            EditorUtility.SetDirty(fuselage);
            EditorSceneManager.MarkSceneDirty(fuselage.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
