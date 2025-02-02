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
    SerializedProperty seats;

    protected override void OnEnable()
    {
        base.OnEnable();
        seats = serializedObject.FindProperty("seats");
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
                if (crew.seats == null) crew.seats = new List<CrewSeat>();

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

                if (crew.seats.Count == 0) EditorGUILayout.HelpBox("You must assign a seat to this crewmember", MessageType.Warning);
            }




            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif

