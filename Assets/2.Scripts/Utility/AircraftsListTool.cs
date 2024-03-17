using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class AircraftsListTool : MonoBehaviour
{
    public AircraftsList list;


    ///WRITE YOUR CUSTOM CODE HERE
    public void CustomFunction(SofAircraft aircraft)
    {
        foreach (Wheel wheeler in aircraft.GetComponentsInChildren<Wheel>())
        {

        }
    }

}
#if UNITY_EDITOR
[CustomEditor(typeof(AircraftsListTool))]
public class AircraftsListToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Write your custom code in this script to make changes to every aircraft in the list at once.", MessageType.Info);

        base.OnInspectorGUI();
        serializedObject.Update();
        //
        AircraftsListTool tool = (AircraftsListTool)target;
        AircraftsList list = tool.list;


        if (list && GUILayout.Button("Execute Tool"))
        {
            foreach (AircraftCard card in list.list)
            {
                SofAircraft aircraft = card.aircraft.GetComponent<SofAircraft>();
                string path = AssetDatabase.GetAssetPath(aircraft);
                GameObject contentsRoot = PrefabUtility.LoadPrefabContents(path);
                aircraft = contentsRoot.GetComponent<SofAircraft>();

                tool.CustomFunction(aircraft);

                PrefabUtility.SaveAsPrefabAsset(contentsRoot, path);
                PrefabUtility.UnloadPrefabContents(contentsRoot);
            }
        }
        if (GUI.changed)
        {
            EditorUtility.SetDirty(tool);
            EditorSceneManager.MarkSceneDirty(tool.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif