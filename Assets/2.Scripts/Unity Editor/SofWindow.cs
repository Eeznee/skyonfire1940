using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
public class SofWindow : EditorWindow
{
    [MenuItem("Sky On Fire/Window")]
    public static void ShowWindow()
    {
        GetWindow<SofWindow>("SOF Toolbar");
    }



    public static bool showFuselageOverlay;
    public static bool showWingsOverlay;
    public static bool showCrewMembers;

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();

        showFuselageOverlay = EditorGUILayout.Toggle("Fuselage Overlay",showFuselageOverlay);
        showWingsOverlay = EditorGUILayout.Toggle("Wings Overlay", showWingsOverlay);
        showCrewMembers = EditorGUILayout.Toggle("Show Crew Members", showCrewMembers);

        if (EditorGUI.EndChangeCheck())
        {
            SaveEditorPrefs();
            SceneView.RepaintAll();
        }
    }
    private void OnEnable()
    {
        showFuselageOverlay = EditorPrefs.GetBool("FuselageOverlay", true);
        showWingsOverlay = EditorPrefs.GetBool("WingsOverlay", true);
        showCrewMembers = EditorPrefs.GetBool("ShowCrewMembers", true);
    }
    private void SaveEditorPrefs()
    {
        EditorPrefs.SetBool("FuselageOverlay", showFuselageOverlay);
        EditorPrefs.SetBool("WingsOverlay", showWingsOverlay);
        EditorPrefs.SetBool("ShowCrewMembers", showCrewMembers);
    }
    private void OnDisable()
    {
        SaveEditorPrefs();
    }
}
#endif