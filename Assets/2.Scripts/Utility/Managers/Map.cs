using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Map : MonoBehaviour
{
    public Airfield[] airfields;
    public Texture2D texture2d;
    public float spaceBetweenPoints = 20f;
    public float maxAltitude = 400f;

    public Rigidbody rb { get; private set; }
    private void Awake()
    {
        rb = this.GetCreateComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.mass = 100000f;
        rb.interpolation = RigidbodyInterpolation.None;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
    }
    public void CreateHeightMap()
    {
        Bounds bounds = GetComponent<MeshCollider>().bounds;

        int length = Mathf.RoundToInt(bounds.size.x / spaceBetweenPoints);
        int width = Mathf.RoundToInt(bounds.size.z / spaceBetweenPoints);
        length -= length % 4;
        width -= width % 4;
        Debug.Log(bounds.size);
        Vector3 basePos = bounds.center;
        basePos.y = maxAltitude;
        basePos.x -= bounds.size.x * 0.5f;
        basePos.z -= bounds.size.z * 0.5f;
        

        int mask = LayerMask.GetMask("Terrain");

        if (texture2d) texture2d.Reinitialize(length, width);
        else texture2d = new Texture2D(length, width, TextureFormat.BC4, false);
        texture2d.name = "heightmap";
        for (int x = 0; x < length; x++)
        {
            for (int z = 0; z < width; z++)
            {
                Vector3 pos = basePos + new Vector3(x, 0f, z) * spaceBetweenPoints;

                float height = 0f;
                Ray ray = new Ray(pos, Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit hit, maxAltitude, mask)) height = hit.point.y;

                Color color = Color.Lerp(Color.black, Color.white, height / maxAltitude);
                texture2d.SetPixel(x, z, color);
            }
        }
        texture2d.Apply();


        //editor variable causes error on build
        //SerializedObject obj = new SerializedObject(texture2d);
        //obj.ApplyModifiedProperties();
    }

    public float HeightAboveGround(Vector3 point)
    {
        return point.y - GroundHeight(point);
    }
    public float GroundHeight(Vector3 point)
    {
        point -= transform.position;
        point /= spaceBetweenPoints;
        int x = Mathf.Clamp(Mathf.RoundToInt(point.x), 0, texture2d.width - 1);
        int z = Mathf.Clamp(Mathf.RoundToInt(point.z), 0, texture2d.height - 1);
        float height = texture2d.GetPixel(x, z).r;
        return height * maxAltitude;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Map))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Map map = (Map)target;

        base.OnInspectorGUI();

        if (GUILayout.Button("Compute Heightmap"))
            map.CreateHeightMap();

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
