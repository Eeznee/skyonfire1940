using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[RequireComponent(typeof(HydraulicSystem))]
public class HydraulicsDragAndRip : SofComponent
{
    public bool ripsAtHighSpeed;
    public float maxSpeedKph = 300f;

    public float maxDrag = 0.2f;
    public float dragPartsRipped = 0f;

    [HideInInspector] public HydraulicSystem hydraulics;

    const float kphToMps = 1 / 3.6f;

    public float MaxSpeeedMps => maxSpeedKph * kphToMps;

    private void Start()
    {
        hydraulics = GetComponent<HydraulicSystem>();
        if (hydraulics.control == HydraulicControl.Type.Flaps) Debug.LogError("Flaps already manage drag and rip");

        if (!ripsAtHighSpeed) return;
        
        foreach(SofModule module in hydraulics.essentialParts)
        {
            SofFrame frame = module.GetComponent<SofFrame>();
            if(frame == null) continue;

            frame.CreateCustomRipSpeed(maxSpeedKph * kphToMps, hydraulics);
        }
    }

    void FixedUpdate()
    {
        float cd = hydraulics.state * (hydraulics.disabled ? dragPartsRipped : maxDrag);
        if (cd > 0f)
        {
            Vector3 velocity = rb.velocity;
            Vector3 drag = Aerodynamics.Drag(velocity, data.tas.Get, data.density.Get, 1f, cd, 1f);
            rb.AddForceAtPosition(drag, transform.position, ForceMode.Force);
        }
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(HydraulicsDragAndRip)), CanEditMultipleObjects]
public class HydraulicsDragAndRipEditor : SofComponentEditor
{
    SerializedProperty ripsAtHighSpeed;
    SerializedProperty maxSpeedKph;
    SerializedProperty maxDrag;
    SerializedProperty dragPartsRipped;


    protected override void OnEnable()
    {
        base.OnEnable();
        ripsAtHighSpeed = serializedObject.FindProperty("ripsAtHighSpeed");
        maxSpeedKph = serializedObject.FindProperty("maxSpeedKph");
        maxDrag = serializedObject.FindProperty("maxDrag");
        dragPartsRipped = serializedObject.FindProperty("dragPartsRipped");
    }
    protected override string BasicName()
    {
        return "Drag";
    }

    static bool rip = true;
    public override void OnInspectorGUI()
    {
        HydraulicsDragAndRip dragAndRip = (HydraulicsDragAndRip)target;
        base.OnInspectorGUI();

        rip = EditorGUILayout.Foldout(rip, "Rip Speed", true, EditorStyles.foldoutHeader);
        if (rip)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(ripsAtHighSpeed);

            if (dragAndRip.ripsAtHighSpeed) EditorGUILayout.PropertyField(maxSpeedKph, new GUIContent("Max Speed Km/h"));

            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
    protected override void BasicFoldout()
    {
        base.BasicFoldout();



        EditorGUILayout.PropertyField(maxDrag, new GUIContent("Drag Parts Intact"));
        EditorGUILayout.PropertyField(dragPartsRipped, new GUIContent("Drag Parts Ripped"));
    }
}
#endif


