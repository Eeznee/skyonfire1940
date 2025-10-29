using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[CreateAssetMenu(fileName = "New Aircraft List", menuName = "SOF/Game Data/Aircraft List")]
public class AircraftsList : ScriptableObject
{
    public AircraftCard[] list;
    

    public void UpdateCards()
    {
        for (int i = 0; i < list.Length; i++)
        {
            list[i]?.UpdateAircraft(i);
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

        var property = serializedObject.FindProperty("list");
        serializedObject.Update();
        EditorGUILayout.PropertyField(property, true);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif

