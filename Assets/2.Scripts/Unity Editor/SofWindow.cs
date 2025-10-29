using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
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
    public static bool showWingsSpars;
    public static bool showCrewMembers;

    public static AircraftsList aircraftsList;


    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();

        showFuselageOverlay = EditorGUILayout.Toggle("Fuselage Overlay", showFuselageOverlay);
        showWingsOverlay = EditorGUILayout.Toggle("Wings Overlay", showWingsOverlay);
        showWingsSpars = EditorGUILayout.Toggle("Wings Spars", showWingsSpars);
        showCrewMembers = EditorGUILayout.Toggle("Show Crew Members", showCrewMembers);

        EditorGUILayout.HelpBox("Use an aircraft list and apply a custom function (code it in SofWindow.CustomFunction). Useful for making a change to all the aircrafts in the game", MessageType.Info);

        aircraftsList = EditorGUILayout.ObjectField(aircraftsList, typeof(AircraftsList),true) as AircraftsList;

        if (aircraftsList && GUILayout.Button("Execute Custom Function"))
        {
            foreach (AircraftCard card in aircraftsList.list)
            {
                SofAircraft aircraft = card.aircraft.GetComponent<SofAircraft>();
                string path = AssetDatabase.GetAssetPath(aircraft);
                GameObject contentsRoot = PrefabUtility.LoadPrefabContents(path);
                aircraft = contentsRoot.GetComponent<SofAircraft>();

                CustomFunction(aircraft);

                PrefabUtility.SaveAsPrefabAsset(contentsRoot, path);
                PrefabUtility.UnloadPrefabContents(contentsRoot);
            }
        }

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
        showWingsSpars = EditorPrefs.GetBool("WingsSpars", true);
        showCrewMembers = EditorPrefs.GetBool("ShowCrewMembers", true);
    }
    private void SaveEditorPrefs()
    {
        EditorPrefs.SetBool("FuselageOverlay", showFuselageOverlay);
        EditorPrefs.SetBool("WingsOverlay", showWingsOverlay);
        EditorPrefs.SetBool("WingsSpars", showWingsSpars);
        EditorPrefs.SetBool("ShowCrewMembers", showCrewMembers);
    }
    private void OnDisable()
    {
        SaveEditorPrefs();
    }

    //Write a custom function here to trigger with the SofWindow
    public void CustomFunction(SofAircraft aircraft)
    {
        foreach(SofFrame frame in aircraft.GetComponentsInChildren<SofFrame>())
        {
            if (frame.GetComponent<Collider>() == null) Debug.Log(aircraft.name + " " + frame.name);
        }
    }
}
#endif