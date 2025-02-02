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
    SerializedProperty preset;
    SerializedProperty oil;
    SerializedProperty water;
    protected override void OnEnable()
    {
        base.OnEnable();
        preset = serializedObject.FindProperty("preset");
        oil = serializedObject.FindProperty("oil");
        water = serializedObject.FindProperty("water");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        Engine engine = (Engine)target;
        EditorGUILayout.PropertyField(preset);

        if (engine.Preset)
        {
            EditorGUILayout.PropertyField(oil, new GUIContent("Oil Tank"));
            bool liquidCooled = engine.Preset.type == EnginePreset.Type.V || engine.Preset.type == EnginePreset.Type.Inverted;
            if (liquidCooled) EditorGUILayout.PropertyField(water, new GUIContent("Water Tank"));
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
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PistonEngine engine = (PistonEngine)target;
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
    SerializedProperty inlet;

    protected override void OnEnable()
    {
        base.OnEnable();
        inlet = serializedObject.FindProperty("inlet");
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        EditorGUILayout.PropertyField(inlet);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif