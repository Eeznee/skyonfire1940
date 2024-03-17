using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class FuselageCore : Fuselage
{
    public float cd = 0.01f;
    public List<Fuselage> linkedFuselages;
    public bool detachable = false;

    const float attachedMaxOffset = 1.5f;
    const float maxCd = 1.5f;
    const float maxCl = 1.2f;

    public override float AreaCd()
    {
        return CombinedArea() * cd;
    }
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
        Vector2 coefs = Aerodynamics.SimpleCoefficients(data.angleOfSlip.Get, maxCl, cd, maxCd);

        Vector3 lift = Aerodynamics.ComputeLift(rb.velocity, data.tas.Get, transform.up, data.density.Get, areas, coefs.y, StructureIntegrity()); 
        Vector3 drag = Aerodynamics.ComputeDrag(rb.velocity, data.tas.Get, data.density.Get, areas, coefs.x, StructureIntegrity());

        rb.AddForce(lift + drag);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(FuselageCore)), CanEditMultipleObjects]
public class FuselageCoreEditor : FuselageEditor
{
    SerializedProperty cd;
    SerializedProperty detachable;
    protected override void OnEnable()
    {
        base.OnEnable();
        cd = serializedObject.FindProperty("cd");
        detachable = serializedObject.FindProperty("detachable");
    }
    static bool showCore = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        FuselageCore fuselage = (FuselageCore)target;

        showCore = EditorGUILayout.Foldout(showCore, "Fuselage Core", true, EditorStyles.foldoutHeader);
        if (showCore)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(cd);
            EditorGUILayout.PropertyField(detachable);
            EditorGUILayout.LabelField("Complete Fuselage Area", fuselage.CombinedArea().ToString("0.0") + " m²");

            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
