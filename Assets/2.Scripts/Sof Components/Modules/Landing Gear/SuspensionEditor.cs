using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(Suspension)), CanEditMultipleObjects]
public class SuspensionEditor : BarFrameEditor
{
    private SerializedProperty type;
    private SerializedProperty deformationOrigin;
    private SerializedProperty axis;

    private SerializedProperty springStrength;
    private SerializedProperty springDamper;

    private SerializedProperty springStrengthFactor;
    private SerializedProperty springDamperFactor;

    protected override void OnEnable()
    {
        base.OnEnable();

        type = serializedObject.FindProperty("type");
        deformationOrigin = serializedObject.FindProperty("deformationOrigin");
        axis = serializedObject.FindProperty("axis");


        springStrength = serializedObject.FindProperty("springStrength");
        springDamper = serializedObject.FindProperty("springDamper");
        springStrengthFactor = serializedObject.FindProperty("springStrengthFactor");
        springDamperFactor = serializedObject.FindProperty("springDamperFactor");
    }

    static bool showSuspension = true;
    public override void OnInspectorGUI()
    {
        Suspension suspension = (Suspension)target;
        if (suspension.GetComponentInChildren<Wheel>() == null)
            EditorGUILayout.HelpBox("This suspension needs a wheel as its child", MessageType.Warning);

        base.OnInspectorGUI();

        showSuspension = EditorGUILayout.Foldout(showSuspension, "Suspension", true, EditorStyles.foldoutHeader);
        if (showSuspension)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(type);

            if (suspension.type == Suspension.Type.Solid)
            {
                EditorGUILayout.PropertyField(deformationOrigin);
            }

            EditorGUILayout.PropertyField(axis);

            Wheel wheel = suspension.GetComponentInChildren<Wheel>();

            if (!wheel) return;

            if (wheel.autoValuesType == Wheel.AutoValuesType.CustomWheel)
            {
                EditorGUILayout.PropertyField(springStrength, new GUIContent("Strength N/m"));
                EditorGUILayout.PropertyField(springDamper, new GUIContent("Damper kg/s"));

                EditorGUILayout.HelpBox("strength recommended value is mass * 100 for main gear", MessageType.Info);
                EditorGUILayout.HelpBox("damper recommended value is strength / 20 for main gear", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Higher damper values decrease oscillation", MessageType.Info);
                EditorGUILayout.Slider(springStrengthFactor, 0f, 3f, new GUIContent("Strength"));
                EditorGUILayout.Slider(springDamperFactor, 0f, 3f, new GUIContent("Damper"));
            }
            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }

    protected void OnSceneGUI()
    {
        Suspension suspension = (Suspension)target;

        Vector3 lowerPoint = suspension.transform.position;
        Vector3 higherPoint = suspension.transform.TransformPoint(suspension.axis.normalized * 2f);

        if(suspension.type == Suspension.Type.Solid)
        {
            lowerPoint = suspension.transform.TransformPoint(suspension.deformationOrigin);
            higherPoint = lowerPoint + suspension.transform.TransformDirection(-suspension.axis.normalized * 2f);
        }

        Handles.color = Color.red;
        Handles.DrawLine(lowerPoint, higherPoint, 2f);
    }
}
#endif