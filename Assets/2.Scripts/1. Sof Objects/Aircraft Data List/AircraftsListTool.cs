using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class AircraftsListTool : MonoBehaviour
{
    public AircraftsList list;

    void CheckFramesColliders(SofAircraft aircraft)
    {
        SofFrame[] boundedFrames = aircraft.GetComponentsInChildren<SofFrame>();
        foreach (SofFrame frame in boundedFrames)
        {
            if (!frame.GetComponent<Collider>() && frame.Detachable) Debug.Log(aircraft.name + " : " + frame.name + " has no collider");
        }
    }

    ///WRITE YOUR CUSTOM CODE HERE
    public void CustomFunction(SofAircraft aircraft)
    {
        foreach(Wing wing in aircraft.GetComponentsInChildren<Wing>())
        {
            foreach(MeshCollider meshCollider in wing.GetComponents<MeshCollider>())
            {
                DestroyImmediate(meshCollider);
            }
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