using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


#if UNITY_EDITOR
[CustomEditor(typeof(CrewMember)), CanEditMultipleObjects]
public class CrewEditor : ModuleEditor
{
    static bool showSeats = true;
    SerializedProperty material;
    SerializedProperty seats;

    static bool showParachutes = true;
    SerializedProperty parachute;
    SerializedProperty specialPlayerParachute;

    protected override void OnEnable()
    {
        base.OnEnable();
        material = serializedObject.FindProperty("material");
        seats = serializedObject.FindProperty("seats");

        parachute = serializedObject.FindProperty("parachute");
        specialPlayerParachute = serializedObject.FindProperty("specialPlayerParachute");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        CrewMember crew = (CrewMember)target;

        GUILayout.Space(15f);
        showSeats = EditorGUILayout.Foldout(showSeats, "Seats", true, EditorStyles.foldoutHeader);
        if (showSeats)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(seats);

            if (crew.complex)
            {
                string[] names = new string[crew.seats.Count];
                int[] values = new int[crew.seats.Count];

                for (int i = 0; i < names.Length; i++)
                {
                    CrewSeat seat = crew.seats[i];
                    if (seat == null) names[i] = "NULL";
                    else names[i] = seat.gameObject.activeInHierarchy ? seat.name : seat.name + " (UNACTIVE)";
                    values[i] = i;
                }
                crew.seatIdTest = EditorGUILayout.IntPopup("Test Seat Animation", crew.seatIdTest, names, values);
            }


            EditorGUI.indentLevel--;
        }
        GUILayout.Space(15f);
        showParachutes = EditorGUILayout.Foldout(showParachutes, "Parachute", true, EditorStyles.foldoutHeader);
        if (showParachutes)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(parachute);
            EditorGUILayout.PropertyField(specialPlayerParachute);

            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif

