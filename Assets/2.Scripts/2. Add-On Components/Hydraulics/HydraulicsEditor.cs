using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


#if UNITY_EDITOR
[CustomEditor(typeof(HydraulicSystem)), CanEditMultipleObjects]
public class HydraulicSystemEditor : Editor
{
    static bool showMechanism = true;
    protected SerializedProperty control;
    protected SerializedProperty animParameter;
    protected SerializedProperty binary;
    protected SerializedProperty defaultState;

    static bool showSettings = true;
    protected SerializedProperty loweringTime;
    protected SerializedProperty retractingTime;
    protected SerializedProperty essentialParts;

    static bool showAudio = true;
    SerializedProperty clip;
    SerializedProperty extendedLockClip;
    SerializedProperty retractedLockClip;
    SerializedProperty extendOnly;
    SerializedProperty volume;
    SerializedProperty pitch;
    protected virtual void OnEnable()
    {
        control = serializedObject.FindProperty("control");
        animParameter = serializedObject.FindProperty("animParameter");
        binary = serializedObject.FindProperty("binary");
        defaultState = serializedObject.FindProperty("defaultState");

        loweringTime = serializedObject.FindProperty("loweringTime");
        retractingTime = serializedObject.FindProperty("retractingTime");
        essentialParts = serializedObject.FindProperty("essentialParts");

        clip = serializedObject.FindProperty("clip");
        extendedLockClip = serializedObject.FindProperty("extendedLockClip");
        retractedLockClip = serializedObject.FindProperty("retractedLockClip");
        extendOnly = serializedObject.FindProperty("extendOnly");
        volume = serializedObject.FindProperty("volume");
        pitch = serializedObject.FindProperty("pitch");
    }
    protected virtual void AnimParameter()
    {
        HydraulicSystem hydraulic = (HydraulicSystem)target;

        if (hydraulic.control == HydraulicControl.Type.Custom)
            EditorGUILayout.PropertyField(animParameter);
    }

    public override void OnInspectorGUI()
    {
        HydraulicSystem hydraulic = (HydraulicSystem)target;

        serializedObject.Update();

        showMechanism = EditorGUILayout.Foldout(showMechanism, "Mechanism", true, EditorStyles.foldoutHeader);
        if (showMechanism)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(control);

            if (!HydraulicControl.IsAlwaysBinary(hydraulic.control))
                EditorGUILayout.PropertyField(binary);

            AnimParameter();

            if(hydraulic.control.HasCustomDefaultState())
                EditorGUILayout.PropertyField(defaultState);

            EditorGUI.indentLevel--;
        }

        showSettings = EditorGUILayout.Foldout(showSettings, "Settings", true, EditorStyles.foldoutHeader);
        if(showSettings)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(loweringTime);
            EditorGUILayout.PropertyField(retractingTime);

            EditorGUILayout.PropertyField(essentialParts);

            EditorGUI.indentLevel--;
        }

        showAudio = EditorGUILayout.Foldout(showAudio, "Audio", true, EditorStyles.foldoutHeader);
        if (showAudio)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(clip);
            EditorGUILayout.PropertyField(extendedLockClip);
            EditorGUILayout.PropertyField(retractedLockClip);
            EditorGUILayout.PropertyField(extendOnly);
            EditorGUILayout.PropertyField(volume);
            EditorGUILayout.PropertyField(pitch);

            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}

[CustomEditor(typeof(SplitHydraulics)), CanEditMultipleObjects]
public class SplitHydraulicsEditor : HydraulicSystemEditor
{
    SerializedProperty parameters;

    protected override void OnEnable()
    {
        base.OnEnable();
        parameters = serializedObject.FindProperty("parameters");
    }

    protected override void AnimParameter()
    {
        EditorGUILayout.PropertyField(parameters);
    }
}
#endif
