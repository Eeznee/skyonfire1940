using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
public class SofWindow : EditorWindow
{
    [MenuItem("Window/SOF")]
    public static void ShowWindow()
    {
        GetWindow<SofWindow>("SOF Toolbar");
    }



    public static bool showAirframesOverlay;
    public static bool showCrewMembers;

    private void OnGUI()
    {
        showAirframesOverlay = EditorGUILayout.Toggle("Airframes Overlay",showAirframesOverlay);
        //showCrewMembers = EditorGUILayout.Toggle("Show Crew Members", showCrewMembers);
    }


    private void OnEnable()
    {
        showAirframesOverlay = PlayerPrefs.GetInt("AirframesOverlay", 1) == 1;
        showCrewMembers = PlayerPrefs.GetInt("ShowCrewMembers", 1) == 1;
    }

    private void OnValidate()
    {
        PlayerPrefs.SetInt("AirframesOverlay", showAirframesOverlay ? 1 : 0);
        PlayerPrefs.SetInt("ShowCrewMembers", showCrewMembers ? 1 : 0);
    }
}
#endif