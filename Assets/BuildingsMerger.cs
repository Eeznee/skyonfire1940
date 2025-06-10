using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BuildingsMerger : MonoBehaviour
{
    private List<SimpleDamageModel> allStaticDamageModels;
    private List<MeshFilter> allStaticMeshFilters;
    public void Awake()
    {
        MeshFilter thisMeshFilter = this.GetCreateComponent<MeshFilter>();
        MeshRenderer thisMeshRenderer = this.GetCreateComponent<MeshRenderer>();

        Material material = this.GetComponentInActualChildren<MeshRenderer>().sharedMaterial;
        thisMeshRenderer.sharedMaterial = material;

        SimpleDamageModel[] allSimples = GetComponentsInChildren<SimpleDamageModel>();
        allStaticDamageModels = new List<SimpleDamageModel>();
        allStaticMeshFilters = new List<MeshFilter>();

        foreach (SimpleDamageModel building in allSimples)
        {
            if (building.gameObject.isStatic)
            {
                allStaticDamageModels.Add(building);
                building.OnDestroy += UpdateCombinedMesh;
            }
        }

        MeshFilter[] allMeshFilters = GetComponentsInChildren<MeshFilter>(true);
        foreach (MeshFilter meshFilter in allMeshFilters)
        {
            if (meshFilter == thisMeshFilter) continue;
            if (!meshFilter.gameObject.isStatic) continue;

            MeshRenderer meshRend = meshFilter.gameObject.GetComponent<MeshRenderer>();

            if (meshRend.sharedMaterial != material) continue;

            meshRend.enabled = false;
            allStaticMeshFilters.Add(meshFilter);

        }

        UpdateCombinedMesh();
    }
    private void Start()
    {
        UpdateCombinedMesh();
    }

    private void UpdateCombinedMesh()
    {
        CombineInstance[] combine = new CombineInstance[allStaticMeshFilters.Count];

        for (int i = 0; i < combine.Length; i++)
        {
            MeshFilter filter = allStaticMeshFilters[i];

            if (filter && filter.gameObject.activeSelf)
            {
                combine[i].mesh = filter.sharedMesh;
                Vector3 translate = transform.InverseTransformPoint(filter.transform.position);
                Quaternion rotate = Quaternion.Inverse(transform.rotation) * filter.transform.rotation;
                combine[i].transform = Matrix4x4.TRS(translate, rotate, Vector3.one);
            }
            else
            {
                combine[i].mesh = new Mesh();
            }
        }

        MeshFilter mergeFilter = GetComponent<MeshFilter>();
        if (mergeFilter.sharedMesh == null) mergeFilter.sharedMesh = new Mesh();
        mergeFilter.sharedMesh.CombineMeshes(combine);
    }
}
