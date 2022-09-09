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


    public void CreateChunk(float _size, float _density)
    {
        meshFilter = GetComponent<MeshFilter>();
        size = _size;
        density = _density;
        spaceBetween = 1f / density;
        Vector3 pos = transform.position;
        pos.y = GameManager.map.HeightAtPoint(pos);
        transform.position = pos;

        int amount = Mathf.CeilToInt(size / spaceBetween);
        allGrass = new MeshFilter[3 * amount * amount];
        for (int x = 0; x < amount; x ++)
        {
            for (int z = 0; z < amount; z ++)
            {
                for (int s = 0; s < 3; s++)
                {
                    Vector2 hexPos = Mathv.HexPoint(x * spaceBetween, z * spaceBetween, s);
                    pos = new Vector3(hexPos.x, 0f, hexPos.y);
                    pos += transform.position;
                    MeshFilter spawnedGrass = Instantiate(grass, pos, Quaternion.identity, transform);
                    allGrass[x * amount * 3 + z * 3 + s] = spawnedGrass;
                }
            }
        }
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

        for (int i = 0; i < allGrass.Length; i++)
        {
            Transform child = allGrass[i].transform;
            pos = child.position;
            if (i != 0)
            {
                pos.x += Random.Range(-spaceBetween, spaceBetween);
                pos.z += Random.Range(-spaceBetween, spaceBetween);
            }
            pos.y = GameManager.map.HeightAtPoint(pos);
            if (pos.y < 1f) pos.y = -15f;
            child.position = pos;
        }

        CombineInstance[] combine = new CombineInstance[allGrass.Length];
        Matrix4x4 myTransform = transform.worldToLocalMatrix;
        for (int i = 0; i < allGrass.Length; i++)
        {
            combine[i].mesh = allGrass[i].sharedMesh;
            combine[i].transform = myTransform * allGrass[i].transform.localToWorldMatrix;
            Destroy(allGrass[i].gameObject);
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