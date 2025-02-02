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
    SerializedProperty airfoil;
    SerializedProperty oswald;
    SerializedProperty skinMesh;



    static bool showAirfoil = true;

    protected override void OnEnable()
    {
        base.OnEnable();
        airfoil = serializedObject.FindProperty("airfoil");
        oswald = serializedObject.FindProperty("oswald");
        skinMesh = serializedObject.FindProperty("skinMesh");
    }
    protected override void BasicFoldout()
    {
        EditorGUILayout.PropertyField(skinMesh, new GUIContent("Skin Collider"));
        base.BasicFoldout();
        Wing wing = (Wing)target;
        EditorGUILayout.LabelField("Full Wing Area", wing.EntireWingArea.ToString("0.00") + " m2");
    }
    protected void AirfoilFoldout()
    {
        EditorGUILayout.PropertyField(airfoil);
        EditorGUILayout.Slider(oswald, 0.3f, 1f);
    }
    protected bool ShowAirfoilFoldout()
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

        if (ShowAirfoilFoldout())
        {
            showAirfoil = EditorGUILayout.Foldout(showAirfoil, "Airfoil", true, EditorStyles.foldoutHeader);
            if (showAirfoil)
            {
                EditorGUI.indentLevel++;
                AirfoilFoldout();
                EditorGUI.indentLevel--;
            }
        }

        base.OnInspectorGUI();
    }
}
#endif
