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

}
#if UNITY_EDITOR
[CustomEditor(typeof(AircraftsListTool))]
public class AircraftsListToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        //
        AircraftsListTool tool = (AircraftsListTool)target;
        AircraftsList list = tool.list;

        if (list && GUILayout.Button("Use Tool"))
        {
            foreach (AircraftCard card in list.list)
            {
                SofAircraft aircraft = card.aircraft.GetComponent<SofAircraft>();
                string path = AssetDatabase.GetAssetPath(aircraft);
                GameObject contentsRoot = PrefabUtility.LoadPrefabContents(path);
                aircraft = contentsRoot.GetComponent<SofAircraft>();
/*
                if (PrefabUtility.GetCorrespondingObjectFromSource(contentsRoot) == null)
                {
                    string p = AssetDatabase.GetAssetPath(aircraft.GetComponentInChildren<Stabilizer>().GetComponent<MeshFilter>().sharedMesh);
                    p = p.Replace(".fbx", "");
                    p = p.Replace("Assets/Resources/", "");
                    Mesh[] meshes = Resources.LoadAll<Mesh>(p);
                    foreach (Airfoil foil in aircraft.GetComponentsInChildren<Airfoil>())
                    {
                        foil.skinMesh = null;
                        foreach (Mesh m in meshes)
                        {
                            string meshName = m.name.ToLower();
                            if (meshName.Contains(foil.name.ToLower()) && meshName.Contains("collider"))
                                foil.skinMesh = m;
                        }
                        if (foil.skinMesh == null)
                        {
                            foreach (Mesh m in meshes)
                            {
                                string meshName = m.name.ToLower();
                                if (meshName.Contains(foil.name.ToLower()))
                                    foil.skinMesh = m;
                            }
                            if (foil.skinMesh == null)
                            {
                                Debug.LogError(foil.transform.root.name + " " + foil.name + " has no skin mesh", foil.gameObject);
                            }
                        }
                    }
                }
                /*

                */
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