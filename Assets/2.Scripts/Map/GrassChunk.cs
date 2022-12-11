using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassChunk : MonoBehaviour
{
    public MeshFilter grass;
    [HideInInspector] public MeshFilter[] allGrass;
    [HideInInspector] public float size;
    [HideInInspector] public float density;
    [HideInInspector] public float spaceBetween;


    [HideInInspector] public MeshFilter meshFilter;

    [HideInInspector] public int amount;
    [HideInInspector] public bool made = false;


    public void CreateChunk(float _size, float _density)
    {

        meshFilter = GetComponent<MeshFilter>();
        size = _size;
        density = _density;
        spaceBetween = 1f / density;
        Vector3 pos = transform.position;
        pos.y = GameManager.map.HeightAtPoint(pos);
        transform.position = pos;

        amount = Mathf.CeilToInt(size / spaceBetween);
        allGrass = new MeshFilter[amount * amount * 3];
        for (int x = 0; x < amount; x++)
        {
            for (int z = 0; z < amount; z++)
            {
                for (int s = 0; s < 3; s++)
                {
                    allGrass[x * amount * 3 + z * 3 + s] = Instantiate(grass, pos, Quaternion.identity, transform);
                    allGrass[x * amount * 3 + z * 3 + s].gameObject.SetActive(false);
                }
            }
        }
        made = true;
        gameObject.SetActive(false);
    }
    public void WakeChunk()
    {
        gameObject.SetActive(true);
        Vector3 pos = transform.position;
        pos.y = GameManager.map.HeightAtPoint(pos);
        if (pos.y < 10f) {
            gameObject.SetActive(false);
            return;
        }
        transform.position = pos;

        CombineInstance[] combine = new CombineInstance[amount * amount * 3];
        Matrix4x4 worldToLocal = transform.worldToLocalMatrix;
        int i = 0;
        for (int x = 0; x < amount; x++)
        {
            for (int z = 0; z < amount; z++)
            {
                for (int s = 0; s < 3; s++)
                {
                    Vector2 hexPos = Mathv.HexPoint(x * spaceBetween, z * spaceBetween, s);
                    Vector3 grassPos = new Vector3(hexPos.x, 0f, hexPos.y);
                    grassPos += pos;
                    grassPos.x += Random.Range(-spaceBetween, spaceBetween);
                    grassPos.z += Random.Range(-spaceBetween, spaceBetween);
                    grassPos.y = GameManager.map.HeightAtPoint(grassPos);
                    if (grassPos.y < 1f) grassPos.y = -15f;
                    allGrass[x * amount * 3 + z * 3 + s].transform.position = grassPos;

                    combine[i].mesh = allGrass[x * amount * 3 + z * 3 + s].sharedMesh;
                    combine[i].transform = worldToLocal * allGrass[x * amount * 3 + z * 3 + s].transform.localToWorldMatrix;
                    i++;
                }
            }
        }
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.CombineMeshes(combine, true, true, false);
    }
    public void Remove()
    {
        if (meshFilter && meshFilter.mesh) Destroy(meshFilter.mesh);
        Destroy(gameObject);
    }
}