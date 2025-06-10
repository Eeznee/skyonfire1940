using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clouds : MonoBehaviour
{
    public int amount = 30;
    public float minAltitude = 1600f;
    public float maxAltitude = 2000f;
    public Material cloudMat;
    public float minCloudSize = 300f;
    public float maxCloudSize = 1000f;
    public int powerSizeChance = 3;
    public float variationFactor = 0.3f;
    public float thickness = 40f;
    public int minSides = 10;
    public int maxSides = 20;
    public float maxStretch = 2f;

    MeshFilter[] clouds;
    void Start()
    {
        transform.Translate(transform.position.y * Vector3.down); //Set ypos to 0

        clouds = new MeshFilter[amount];
        for (int i = 0; i < amount; i++)
        {
            CreateCloud(i);
        }

        gameObject.AddComponent<BuildingsMerger>();
    }

    public void CreateCloud(int index)
    {
        clouds[index] = new GameObject("Cloud " + index).AddComponent<MeshFilter>();
        MeshRenderer meshRend = clouds[index].gameObject.AddComponent<MeshRenderer>();
        meshRend.sharedMaterial = cloudMat;
        meshRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        //Mesh
        Mesh mesh = new Mesh();
        clouds[index].mesh = mesh;
        mesh.Clear();
        float sizeFactor = Mathf.Pow(Random.value, powerSizeChance);
        float size = Mathf.Lerp(minCloudSize, maxCloudSize, sizeFactor );
        int sides = Mathf.RoundToInt(Mathf.Lerp(minSides, maxSides, sizeFactor));
        mesh.SetVertices(CloudVerts(sides, size));
        mesh.triangles = CloudTris(sides);

        //Transfom
        Transform tr = clouds[index].transform;
        tr.parent = transform;
        tr.localScale = new Vector3(Random.Range(1f, maxStretch), 1f, 1f);
        Vector3 pos = Random.Range(minAltitude, maxAltitude) * Vector3.up;
        pos.x = (Random.value - 0.5f) * 1.5f * GameManager.mapTool.transform.localScale.x;
        pos.z = (Random.value - 0.5f) * 1.5f * GameManager.mapTool.transform.localScale.z;
        tr.localPosition = pos;

        clouds[index].gameObject.isStatic = true;
    }

    public Vector3[] CloudVerts(int sides, float radius)
    {
        Vector3[] points = new Vector3[sides * 3 + 2];
        float angle = 0f;
        for (int i = 0; i < sides; i++)
        {
            angle += 1f / sides * Mathf.PI * 2f;                //Turn around a circle and randomize distance from each point
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            float dis = Random.Range(radius * (1 - variationFactor), radius * (1 + variationFactor));
            points[i] = dir * dis;                                                                  //Center points
            points[i + sides] = dir * (dis - thickness) + Vector3.up * thickness / 2f;              //Upper points
            points[i + 2 * sides] = dir * (dis - thickness) - Vector3.up * thickness / 2f;          //Lower points
        }
        points[3 * sides] = new Vector3(0f, thickness / 2f, 0f);             //Upper center
        points[3 * sides + 1] = new Vector3(0f, -thickness / 2f, 0f);       //Lower center

        return points;
    }
    public int[] CloudTris(int sides)
    {
        int[] tris = new int[18 * sides];       //Upper and Lower tris = 2 sides, surrounding tris = 4 sides, multiplied by 3 because tris

        for (int i = 0; i < sides; i++)
        {
            bool closeLoop = i == sides - 1;
            //Upper
            tris[0 + i * 18] = 3 * sides;
            tris[1 + i * 18] = !closeLoop ? i + sides + 1 : sides;
            tris[2 + i * 18] = i + sides;
            //Lower
            tris[3 + i * 18] = 3 * sides + 1;
            tris[4 + i * 18] = i + sides * 2;
            tris[5 + i * 18] = !closeLoop ? i + sides * 2 + 1 : 2 * sides;
            //Upper surrounding 1
            tris[6 + i * 18] = i + sides;
            tris[7 + i * 18] = !closeLoop ? i + sides + 1 : sides;
            tris[8 + i * 18] = i;
            //Upper surrounding 2
            tris[9 + i * 18] = !closeLoop ? i + 1 : 0;
            tris[10 + i * 18] = i;
            tris[11 + i * 18] = !closeLoop ? i + sides + 1 : sides;
            //Lower surrounding 1
            tris[12 + i * 18] = !closeLoop ? i + sides * 2 + 1 : 2 * sides;
            tris[13 + i * 18] = i + sides * 2;
            tris[14 + i * 18] = i;
            //Lower surrounding 2
            tris[15 + i * 18] = i;
            tris[16 + i * 18] = !closeLoop ? i + 1 : 0;
            tris[17 + i * 18] = !closeLoop ? i + sides * 2 + 1 : sides * 2;
        }
        return tris;
    }
}
