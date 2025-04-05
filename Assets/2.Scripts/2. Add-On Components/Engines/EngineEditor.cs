using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
#if UNITY_EDITOR


[CustomEditor(typeof(Engine))]
public class EngineEditor : ModuleEditor
{
    SerializedProperty oil;
    SerializedProperty water;
    protected override void OnEnable()
    {
        base.OnEnable();
        oil = serializedObject.FindProperty("oil");
        water = serializedObject.FindProperty("water");
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        Engine engine = (Engine)target;

        if (engine.Preset)
        {
            EditorGUILayout.PropertyField(oil, new GUIContent("Oil Tank"));
            if (engine.Preset.LiquidCooled) EditorGUILayout.PropertyField(water, new GUIContent("Water Tank"));
        }
        else
        {
            GUILayout.Space(20f);
            EditorGUILayout.HelpBox("Please assign an engine preset", MessageType.Warning);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(PistonEngine)), CanEditMultipleObjects]
public class PistonEngineEditor : EngineEditor
{
    SerializedProperty pistonPreset;

    protected override void OnEnable()
    {
        base.OnEnable();
        pistonPreset = serializedObject.FindProperty("pistonPreset");
    }

    public override void OnInspectorGUI()
    {
        PistonEngine engine = (PistonEngine)target;

        serializedObject.Update();
        EditorGUILayout.PropertyField(pistonPreset);

        base.OnInspectorGUI();

        if (!engine.GetComponentInChildren<Propeller>())
        {
            GUI.color = Color.red;
            GUILayout.Space(20f);
            EditorGUILayout.HelpBox("You must create a propeller as a child of this engine", MessageType.Warning);
        }
    }
}
#endif
#if UNITY_EDITOR
[CustomEditor(typeof(JetEngine))]
public class JetEngineEditor : EngineEditor
{
    SerializedProperty jetPreset;
    SerializedProperty inlet;
    protected override void OnEnable()
    {
        base.OnEnable();
        inlet = serializedObject.FindProperty("inlet");
        jetPreset = serializedObject.FindProperty("jetPreset");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(jetPreset);
        EditorGUILayout.PropertyField(inlet);

        base.OnInspectorGUI();

        serializedObject.ApplyModifiedProperties();
    }
}
#endif