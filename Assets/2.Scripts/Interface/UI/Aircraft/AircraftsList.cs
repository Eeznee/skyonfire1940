using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[CreateAssetMenu(fileName = "New Aircraft List", menuName = "Aircraft/Aircraft List")]
public class AircraftsList : ScriptableObject
{
    public AircraftCard[] list;
    

    public void UpdateCards()
    {
        for (int i = 0; i < list.Length; i++)
        {
            SofAircraft aircraft = list[i].aircraft.GetComponent<SofAircraft>();
            if (list[i] != null)
            {
                list[i].id = i;
                if (list[i].aircraft) list[i].sofAircraft = list[i].aircraft.GetComponent<SofAircraft>();
            }


            aircraft.emptyCOG.x = 0f;
        }
        
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(AircraftsList))]
public class AircraftsListEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        AircraftsList aircraftsList = (AircraftsList)target;
        //
        GUI.color = GUI.backgroundColor;

        var property = serializedObject.FindProperty("list");
        serializedObject.Update();
        EditorGUILayout.PropertyField(property, true);

        aircraftsList.UpdateCards();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(aircraftsList);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif

