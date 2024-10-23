using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Airframe/Nose")]
public class Nose : Fuselage
{
    public Mesh brokenModel;

    public override bool Detachable => false;
    public override void Rip()
    {
        if (ripped) return;
        base.Rip();
        if (brokenModel) GetComponent<MeshFilter>().mesh = brokenModel;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Nose))]
public class NoseEditor : FuselageEditor
{
    SerializedProperty brokenModel;
    protected override void OnEnable()
    {
        base.OnEnable();
        brokenModel = serializedObject.FindProperty("brokenModel");
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        EditorGUILayout.PropertyField(brokenModel);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
