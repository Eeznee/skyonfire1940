using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(Wing)), CanEditMultipleObjects]
public class WingEditor : MainSurfaceEditor
{
    SerializedProperty oswald;
    SerializedProperty split;
    SerializedProperty splitFraction;
    SerializedProperty skinMesh;

    protected override void OnEnable()
    {
        base.OnEnable();
        oswald = serializedObject.FindProperty("oswald");
        split = serializedObject.FindProperty("split");
        splitFraction = serializedObject.FindProperty("splitFraction");
        skinMesh = serializedObject.FindProperty("skinMesh");
    }
    protected override void BasicFoldout()
    {
        EditorGUILayout.PropertyField(skinMesh, new GUIContent("Skin Collider"));
        base.BasicFoldout();
        Wing wing = (Wing)target;
        EditorGUILayout.LabelField("Full Wing Area", wing.EntireWingArea.ToString("0.00") + " m2");
    }
    protected override void ShapeFoldout()
    {
        Wing wing = (Wing)target;

        base.ShapeFoldout();

        EditorGUILayout.PropertyField(split);
        if (wing.split) EditorGUILayout.Slider(splitFraction, 0f, 1f);
    }
    protected override void AirfoilFoldout()
    {
        base.AirfoilFoldout();
        EditorGUILayout.Slider(oswald, 0.3f, 1f);
    }
    protected override bool ShowAirfoilFoldout()
    {
        Wing wing = (Wing)target;

        return !wing.parent;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Wing wing = (Wing)target;

        wing.root.RecursiveSnap();

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
}
#endif
