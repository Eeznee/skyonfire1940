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
    SerializedProperty card;
    SerializedProperty materials;
    SerializedProperty convergence;
    SerializedProperty axesSpeed;
    SerializedProperty maxG;
    SerializedProperty maxSpeed;
    SerializedProperty stations;

    static bool showAutoPilot = true;
    SerializedProperty cruiseSpeed;
    SerializedProperty bankTurnAngle;
    SerializedProperty customPID;
    SerializedProperty pidElevator;
    SerializedProperty pidPitch;
    SerializedProperty pidRoll;


    protected override void OnEnable()
    {
        base.OnEnable();

        card = serializedObject.FindProperty("card");
        materials = serializedObject.FindProperty("materials");
        convergence = serializedObject.FindProperty("convergeance");
        axesSpeed = serializedObject.FindProperty("axesSpeed");
        maxG = serializedObject.FindProperty("maxG");
        maxSpeed = serializedObject.FindProperty("maxSpeed");
        stations = serializedObject.FindProperty("stations");

        cruiseSpeed = serializedObject.FindProperty("cruiseSpeed");
        bankTurnAngle = serializedObject.FindProperty("bankTurnAngle");
        customPID = serializedObject.FindProperty("customPIDValues");
        pidElevator = serializedObject.FindProperty("pidElevator.pidValues");
        pidPitch = serializedObject.FindProperty("pidPitch.pidValues");
        pidRoll = serializedObject.FindProperty("pidRoll.pidValues");
    }
    public string deletePassWord;
    public static float altitudeTopSpeed;


    static bool showDeprecated = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SofAircraft aircraft = (SofAircraft)target;

        showMain = EditorGUILayout.Foldout(showMain, "Main Settings", true, EditorStyles.foldoutHeader);
        if (showMain)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(materials);
            EditorGUILayout.PropertyField(maxG, new GUIContent("G-Force Limit"));
            EditorGUILayout.PropertyField(maxSpeed,new GUIContent("Speed Limit"));
            EditorGUILayout.PropertyField(axesSpeed);
            EditorGUILayout.PropertyField(convergence, new GUIContent("Gun Convergence"));
            EditorGUILayout.PropertyField(stations);
            foreach (Station s in aircraft.stations) if (aircraft.stations != null && s != null) s.UpdateOptions();

            EditorGUI.indentLevel--;
        }

        showAutoPilot = EditorGUILayout.Foldout(showAutoPilot, "Pilot Assistance", true, EditorStyles.foldoutHeader);
        if (showAutoPilot)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(cruiseSpeed);
            EditorGUILayout.PropertyField(customPID);

            if (aircraft.customPIDValues)
            {
                EditorGUILayout.HelpBox("Only use if controls are unstable. If you don't understand what it is disable this option or ask ISNI for help on discord. Do small changes from default values. Pitch & Roll PID are only used with mouse tracking. Elevator is the most important and is used to stabilize turn rate", MessageType.Info);
                EditorGUILayout.PropertyField(pidElevator, new GUIContent("Elevator PID"));
                EditorGUILayout.PropertyField(pidPitch, new GUIContent("Pitch PID"));
                EditorGUILayout.PropertyField(pidRoll, new GUIContent("Roll PID"));

                if (GUILayout.Button("Default Values=")) aircraft.SetDefaultPIDValues();
            }

            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();


        showDeprecated = EditorGUILayout.Foldout(showDeprecated, "Deprecated", true, EditorStyles.foldoutHeader);
        if (showDeprecated)
        {
            EditorGUI.indentLevel++;

            aircraft.turnRadius = EditorGUILayout.FloatField("Turn Radius", aircraft.turnRadius);
            aircraft.minCrashDelay = EditorGUILayout.FloatField("Minimum Crash Prevention Delay (sec)", aircraft.minCrashDelay);
            aircraft.minInvertAltitude = EditorGUILayout.FloatField("Minimum Altitude Inverted Flight", aircraft.minInvertAltitude);

            EditorGUI.indentLevel--;
        }

        altitudeTopSpeed = EditorGUILayout.Slider("Top Speed Altitude m", altitudeTopSpeed, 0f, 6000f);
        //EditorGUILayout.LabelField("Max Speed", (aircraft.MaxSpeed(altitudeTopSpeed, 1f) * 3.6f).ToString("0.0") + " km/h");

        if (aircraft.crew[0] && aircraft.crew[0].seats.Count > 0 && aircraft.crew[0].seats[0].GetComponent<PilotSeat>() == null)
        {
            EditorGUILayout.HelpBox("First seat must be pilot", MessageType.Warning);
        }


        deletePassWord = EditorGUILayout.PasswordField("Delete All Parts", deletePassWord);
        if (deletePassWord == "isni")
        {
            if (GUILayout.Button("Delete all parts"))
            {
                foreach (SofComponent p in aircraft.GetComponentsInChildren<SofComponent>())
                    DestroyImmediate(p);
                DestroyImmediate(aircraft.data.rb);
                DestroyImmediate(aircraft.data);
                DestroyImmediate(aircraft);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
