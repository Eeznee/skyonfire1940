using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(Suspension)), CanEditMultipleObjects]
public class SuspensionEditor : PartEditor
{
    static bool showSuspension = true;
    private SerializedProperty axis;
    private SerializedProperty springStrength;
    private SerializedProperty springDamper;

    private SerializedProperty compensateWeight;

    private SerializedProperty canSteer;
    private SerializedProperty maxSteerAngle;

    Mass emptyMass;

    protected override void OnEnable()
    {
        base.OnEnable();

        axis = serializedObject.FindProperty("axis");
        springStrength = serializedObject.FindProperty("springStrength");
        springDamper = serializedObject.FindProperty("springDamper");

        compensateWeight = serializedObject.FindProperty("compensateWeight");

        canSteer = serializedObject.FindProperty("canSteer");
        maxSteerAngle = serializedObject.FindProperty("maxSteerAngle");

        Suspension suspension = (Suspension)target;
        SofPart[] partsArray = suspension.complex.parts.ToArray();
        emptyMass = new Mass(partsArray, true);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Suspension suspension = (Suspension)target;

        serializedObject.Update();

        showSuspension = EditorGUILayout.Foldout(showSuspension, "Suspension", true, EditorStyles.foldoutHeader);
        if (showSuspension)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(axis);
            EditorGUILayout.PropertyField(springStrength, new GUIContent("Spring Strength N/m"));
            EditorGUILayout.PropertyField(springDamper, new GUIContent("Spring Damper N/(m/s)"));


            float recommendedMain = emptyMass.mass * 100f;
            float recommendedTail = emptyMass.mass * 20f;
            EditorGUILayout.LabelField("suggested main spring", recommendedMain.ToString("0") + " N/m");
            EditorGUILayout.LabelField("suggested tail spring", recommendedTail.ToString("0") + " N/m");

            EditorGUILayout.Space(10f);

            EditorGUILayout.PropertyField(canSteer);
            if (suspension.canSteer)
                EditorGUILayout.PropertyField(maxSteerAngle);


            if (Application.isPlaying) EditorGUILayout.LabelField("Force", suspension.forceApplied.ToString("0.0") + " N");
            if (Application.isPlaying) EditorGUILayout.LabelField("Distance", suspension.distance.ToString(""));

            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
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