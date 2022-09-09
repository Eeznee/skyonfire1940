using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class MapTool : MonoBehaviour
{
    GameManager gm;
    public Airfield[] airfields;
    public float spaceBetweenPoints = 5;
    public Array[] heightMap;
    public int xLength;
    public int zLength;

    const float maxAltitude = 1000f;

    public float HeightAtPoint(Vector3 point)
    {
        if (!gm) gm = GetComponent<GameManager>();
        point -= transform.position;
        point /= spaceBetweenPoints;
        int x = Mathf.Clamp(Mathf.RoundToInt(point.x), 0, xLength - 1);
        int z = Mathf.Clamp(Mathf.RoundToInt(point.z), 0, zLength - 1);
        return (gm.mapData.heightMap[x].array[z] + 32768) * gm.mapData.maxHeight / 65536;
    }
    public void ComputeHeightMap()
    {
        if (!gm) gm = GetComponent<GameManager>();
        Vector3 pos = transform.position;
        pos.y = maxAltitude;
        Vector3 scale = transform.localScale;

        xLength = Mathf.RoundToInt(scale.x / spaceBetweenPoints);
        zLength = Mathf.RoundToInt(scale.z / spaceBetweenPoints);
        heightMap = new Array[xLength];

        int mask = LayerMask.GetMask("Terrain");
        for (int x = 0; x < xLength; x++)
        {
            heightMap[x] = new Array(new short[zLength]);
            pos.z = transform.position.z;
            for (int z = 0; z < zLength; z++)
            {
                RaycastHit hit;
                Ray ray = new Ray(pos, Vector3.down);
                if (Physics.Raycast(ray, out hit, maxAltitude, mask)) heightMap[x].array[z]= (short) Mathf.RoundToInt(hit.point.y/gm.mapData.maxHeight * 65536 - 32768);
                else heightMap[x].array[z] = -32768; 
                pos.z += spaceBetweenPoints;
            }
            pos.x += spaceBetweenPoints;
        }
        gm.mapData.heightMap = heightMap;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(MapTool))]
public class MapToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        MapTool map = (MapTool)target;

        map.spaceBetweenPoints = EditorGUILayout.FloatField("Space Between Points", map.spaceBetweenPoints);

        if (GUILayout.Button("Compute Heightmap"))
        {
            map.ComputeHeightMap();
            serializedObject.ApplyModifiedProperties();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(map);
            EditorSceneManager.MarkAllScenesDirty();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif