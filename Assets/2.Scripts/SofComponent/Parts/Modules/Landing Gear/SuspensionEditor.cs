using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(Suspension)), CanEditMultipleObjects]
public class SuspensionEditor : SofComponentEditor
{
    private SerializedProperty axis;

    private SerializedProperty preciseValues;

    private SerializedProperty springStrength;
    private SerializedProperty springDamper;

    private SerializedProperty springStrengthFactor;
    private SerializedProperty springDamperFactor;


    protected override string BasicName()
    {
        return "Suspension";
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        axis = serializedObject.FindProperty("axis");

        preciseValues = serializedObject.FindProperty("preciseValues");

        springStrength = serializedObject.FindProperty("springStrength");
        springDamper = serializedObject.FindProperty("springDamper");
        springStrengthFactor = serializedObject.FindProperty("springStrengthFactor");
        springDamperFactor = serializedObject.FindProperty("springDamperFactor");
    }

    protected override void BasicFoldout()
    {
        base.BasicFoldout();

        Suspension suspension = (Suspension)target;


        EditorGUILayout.PropertyField(axis);

        EditorGUILayout.PropertyField(preciseValues);

        if (suspension.preciseValues)
        {
            EditorGUILayout.PropertyField(springStrength, new GUIContent("Strength N/m"));
            EditorGUILayout.PropertyField(springDamper, new GUIContent("Damper kg/s"));

            EditorGUILayout.HelpBox("strength close to mass * 100 for main gear \nstrength close to mass * 20 for tail gear", MessageType.Info);
            EditorGUILayout.HelpBox("Damper close to strength / 20 \ndamper close to strength / 8 for tail gear", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("Higher damper values decrease oscillation", MessageType.Info);
            EditorGUILayout.Slider(springStrengthFactor, 0f, 3f, new GUIContent("Strength"));
            EditorGUILayout.Slider(springDamperFactor, 0f, 3f, new GUIContent("Damper"));
        }
    }

    public override void OnInspectorGUI()
    {
        Suspension suspension = (Suspension)target;
        if (suspension.GetComponentInChildren<CustomWheel>() == null)
            EditorGUILayout.HelpBox("This suspension needs a wheel as its child", MessageType.Warning);


        base.OnInspectorGUI();
    }

    protected void OnSceneGUI()
    {
        Suspension suspension = (Suspension)target;

        Vector3 lowerPoint = suspension.transform.position;
        Vector3 higherPoint = suspension.transform.TransformPoint(suspension.axis.normalized * 2f);

        Handles.color = Color.red;
        Handles.DrawLine(lowerPoint, higherPoint, 2f);
    }
}
#endif