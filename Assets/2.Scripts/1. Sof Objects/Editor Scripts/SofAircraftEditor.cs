using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(SofAircraft))]
public class SofAircraftEditor : SofComplexEditor
{
    static bool showMain = true;
    SerializedProperty convergence;
    SerializedProperty stickTorqueFactor;
    SerializedProperty stallMarginAngle;
    SerializedProperty hydraulicControls;
    SerializedProperty maxG;
    SerializedProperty speedLimitKph;
    SerializedProperty stations;

    SerializedProperty customPID;
    SerializedProperty pidPitch;
    SerializedProperty pidRoll;


    protected override void OnEnable()
    {
        base.OnEnable();

        convergence = serializedObject.FindProperty("convergeance");
        stickTorqueFactor = serializedObject.FindProperty("stickTorqueFactor");
        stallMarginAngle = serializedObject.FindProperty("stallMarginAngle");
        hydraulicControls = serializedObject.FindProperty("hydraulicControls");
        maxG = serializedObject.FindProperty("maxG");
        speedLimitKph = serializedObject.FindProperty("speedLimitKph");
        stations = serializedObject.FindProperty("stations");

        customPID = serializedObject.FindProperty("customPIDValues");
        pidPitch = serializedObject.FindProperty("pidPitch.pidValues");
        pidRoll = serializedObject.FindProperty("pidRoll.pidValues");
    }
    public string deletePassWord;


    static bool showControlAxes = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SofAircraft aircraft = (SofAircraft)target;

        showMain = EditorGUILayout.Foldout(showMain, "Main Settings", true, EditorStyles.foldoutHeader);
        if (showMain)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(maxG, new GUIContent("G-Force Limit in g"));
            EditorGUILayout.PropertyField(speedLimitKph, new GUIContent("Speed Limit in km/h"));
            EditorGUILayout.PropertyField(convergence, new GUIContent("Gun Convergence"));



            EditorGUILayout.PropertyField(stations);
            foreach (Station s in aircraft.Stations) if (aircraft.Stations != null && s != null) s.SelectAndDisactivate();

            EditorGUI.indentLevel--;
        }

        showControlAxes = EditorGUILayout.Foldout(showControlAxes, "Control Axes, Autopilot", true, EditorStyles.foldoutHeader);
        if (showControlAxes)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.HelpBox("The aircraft targets the stall angle of attack of the airfoil to maximize turn rate. This value is substracted from the target angle of attack to avoid accidental stalling . Default value is 1 deg, increase value if aircraft is stalling/unstable in point tracking mode", MessageType.Info);
            EditorGUILayout.Slider(stallMarginAngle, 1f, 6f, new GUIContent("Stall Margine Angle"));
            EditorGUILayout.HelpBox("Default Value is 1, higher values should be use for large aircraft, it increases the speed at which control surfaces can be used, but makes them slower and less reactive", MessageType.Info);
            EditorGUILayout.Slider(stickTorqueFactor, 0.5f, 8f, new GUIContent("Stick Torque Factor"));
            EditorGUILayout.HelpBox("Completely removes control surfaces resistances, use for modern jet aircrafts", MessageType.Info);
            EditorGUILayout.PropertyField(hydraulicControls, new GUIContent("Hydraulics Assist"));


            EditorGUILayout.PropertyField(customPID);

            if (aircraft.CustomPIDValues)
            {
                EditorGUILayout.HelpBox("Only use if tracking (WT) controls are unstable. If you don't understand what it is disable this option or ask ISNI for help on discord. Do small changes from default values.", MessageType.Info);
                EditorGUILayout.PropertyField(pidPitch, new GUIContent("Pitch PID"));
                EditorGUILayout.PropertyField(pidRoll, new GUIContent("Roll PID"));

                if (GUILayout.Button("Default Values"))
                {
                    aircraft.SetDefaultPIDValues();
                    serializedObject.ApplyModifiedProperties();
                }
            }


            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();

        if (aircraft.crew[0] && aircraft.crew[0].seats.Count > 0 && aircraft.crew[0].seats[0].GetComponent<PilotSeat>() == null)
        {
            EditorGUILayout.HelpBox("First seat must be pilot", MessageType.Warning);
        }

        serializedObject.ApplyModifiedProperties();


    }
}
#endif
