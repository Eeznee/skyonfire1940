using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


#if UNITY_EDITOR
[CustomEditor(typeof(Wheel))]
public class WheelEditor : PartEditor
{
    SerializedProperty wheelCollider;
    SerializedProperty brakeMode;
    SerializedProperty brakeTorque;
    SerializedProperty brakeEffect;
    SerializedProperty steering;
    SerializedProperty maxSteerAngle;
    protected override void OnEnable()
    {
        base.OnEnable();
        wheelCollider = serializedObject.FindProperty("wheel");
        brakeMode = serializedObject.FindProperty("brakeMode");
        brakeTorque = serializedObject.FindProperty("brakeTorque");
        brakeEffect = serializedObject.FindProperty("brakeEffect");
        steering = serializedObject.FindProperty("steering");
        maxSteerAngle = serializedObject.FindProperty("maxSteerAngle");
    }

    static bool showWheel = true;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        Wheel wheel = (Wheel)target;

        showWheel = EditorGUILayout.Foldout(showWheel, "Wheel", true, EditorStyles.foldoutHeader);
        if (showWheel)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(wheelCollider);
            EditorGUILayout.PropertyField(brakeMode);
            EditorGUILayout.PropertyField(brakeTorque);
            EditorGUILayout.PropertyField(brakeEffect);
            EditorGUILayout.PropertyField(steering);
            if (wheel.steering)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox("The parent will be rotated", MessageType.Info);
                EditorGUILayout.PropertyField(maxSteerAngle);
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
    protected override void BasicFoldout()
    {
        base.BasicFoldout();

        Wheel wheel = (Wheel)target;

        ModuleMaterial material = wheel.aircraft.materials.Material(wheel);
        EditorGUILayout.LabelField("HP", material.hpPerSq.ToString("0") + " HP");
    }
}
#endif