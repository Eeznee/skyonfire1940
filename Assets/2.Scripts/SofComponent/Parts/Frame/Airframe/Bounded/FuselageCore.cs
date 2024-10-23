using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Airframe/Fuselage Core")]
public class FuselageCore : Fuselage
{
    public float cd = 0.01f;
    public List<Fuselage> linkedFuselages;
    public bool detachable = false;

    const float attachedMaxOffset = 1.5f;
    const float maxCl = 1.2f;

    private SimpleAirfoil airfoil;

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

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        airfoil = new SimpleAirfoil(maxCl, cd, 0f);
    }
    public override void UpdateAerofoil()
    {
        base.UpdateAerofoil();

        linkedFuselages = new List<Fuselage>();
        foreach(Fuselage fus in transform.root.GetComponentsInChildren<Fuselage>())
        {
            Vector3 diff = transform.InverseTransformPoint(fus.transform.position);
            if (Mathf.Abs(diff.x) < attachedMaxOffset) linkedFuselages.Add(fus);
        }
    }
    public override bool Detachable => detachable;

    protected override void FixedUpdate()
    {
        float areas = CombinedArea();
        Vector2 coefs = airfoil.Coefficients(data.angleOfSlip.Get);

        Vector3 lift = Aerodynamics.Lift(rb.velocity, data.tas.Get, transform.up, data.density.Get, areas, coefs.y, structureDamage); 
        Vector3 drag = Aerodynamics.Drag(rb.velocity, data.tas.Get, data.density.Get, areas, coefs.x, structureDamage);

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

            EditorGUILayout.PropertyField(cd, new GUIContent("Drag Coefficient"));
            EditorGUILayout.PropertyField(detachable);
            EditorGUILayout.LabelField("Complete Fuselage Area", fuselage.CombinedArea().ToString("0.0") + " m²");

            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
