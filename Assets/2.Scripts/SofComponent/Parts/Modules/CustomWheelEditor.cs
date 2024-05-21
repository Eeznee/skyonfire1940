using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(CustomWheel)), CanEditMultipleObjects]
public class CustomWheelEditor : ModuleEditor
{
    protected override string BasicName()
    {
        return "Module";
    }
    static bool showWheel = true;
    private SerializedProperty radius;
    private SerializedProperty canSteer;
    private SerializedProperty steerAngle;

    static bool showSuspension = true;
    private SerializedProperty suspensionOffset;
    private SerializedProperty springStrength;
    private SerializedProperty springDamper;

    protected override void OnEnable()
    {
        base.OnEnable();
        radius = serializedObject.FindProperty("radius");

        suspensionOffset = serializedObject.FindProperty("suspensionOffset");
        springStrength = serializedObject.FindProperty("springStrength");
        springDamper = serializedObject.FindProperty("springDamper");

        canSteer = serializedObject.FindProperty("canSteer");
        steerAngle = serializedObject.FindProperty("steerAngle");
    }
    public override void OnInspectorGUI()
    {
        CustomWheel wheel = (CustomWheel)target;

        serializedObject.Update();

        showWheel = EditorGUILayout.Foldout(showWheel, "Wheel", true, EditorStyles.foldoutHeader);
        if (showWheel)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(radius);


            EditorGUILayout.PropertyField(canSteer);
            if (wheel.canSteer)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(steerAngle);
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        showSuspension = EditorGUILayout.Foldout(showSuspension, "Suspension", true, EditorStyles.foldoutHeader);
        if (showSuspension)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(suspensionOffset);
            EditorGUILayout.PropertyField(springStrength);
            EditorGUILayout.PropertyField(springDamper);

            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif