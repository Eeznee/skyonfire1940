using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[RequireComponent(typeof(MeshRenderer))]
public class VegetationRandomizer : MonoBehaviour
{
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
    public Mesh[] meshes;

    public bool tree;
    public Mesh[] trunkMeshes;
    void OnEnable()
    {
        float height = GameManager.mapTool.HeightAtPoint(transform.position);
        transform.Translate(Vector3.up * (height - transform.position.y));
        transform.localScale *= Mathf.Lerp(minScale, maxScale, Random.value);
        transform.Rotate(transform.up * Random.Range(0, 360));

        if (tree)
        {
            transform.GetChild(0).Rotate(transform.up * Random.Range(0, 360));
            GetComponent<MeshFilter>().mesh = trunkMeshes[Random.Range(0, trunkMeshes.Length)];
            transform.GetChild(0).GetComponent<MeshFilter>().mesh = meshes[Random.Range(0, meshes.Length - 1)];
        }
        else
        {
            GetComponent<MeshFilter>().mesh = meshes[Random.Range(0, meshes.Length)];
        }
    }
    private void Start()
    {
        if (GameManager.weather.winter && tree)
            transform.GetChild(0).gameObject.SetActive(false);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(VegetationRandomizer))]
public class VegetationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        VegetationRandomizer veg = (VegetationRandomizer)target;

        veg.minScale = EditorGUILayout.FloatField("Minimum Scale", veg.minScale);
        veg.maxScale = EditorGUILayout.FloatField("Maximum Scale", veg.maxScale);
        SerializedProperty meshes = serializedObject.FindProperty("meshes");
        EditorGUILayout.PropertyField(meshes, true);

        veg.tree = EditorGUILayout.Toggle("Is a tree", veg.tree);
        if (veg.tree)
        {
            SerializedProperty trunkMeshes = serializedObject.FindProperty("trunkMeshes");
            EditorGUILayout.PropertyField(trunkMeshes, true);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(veg);
            EditorSceneManager.MarkSceneDirty(veg.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
