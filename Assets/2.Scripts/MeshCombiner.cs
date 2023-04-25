using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshCombiner : MonoBehaviour
{
    public MeshFilter[] meshFilters;


    public static Mesh MergeMeshFilters(MeshFilter[] filters, Transform root,bool disable)
    {
        CombineInstance[] combine = new CombineInstance[filters.Length];

        for (int i = 0; i < filters.Length; i++)
        {
            if (!filters[i].gameObject.activeInHierarchy) continue;
            combine[i].mesh = filters[i].sharedMesh;
            Vector3 translate = root.InverseTransformPoint(filters[i].transform.position);
            Quaternion rotate = Quaternion.Inverse(root.rotation) * filters[i].transform.rotation;
            combine[i].transform = Matrix4x4.TRS(translate, rotate, Vector3.one);
            if (disable) filters[i].GetComponent<Renderer>().enabled = false;
        }

        Mesh mesh = new Mesh();
        mesh.name = "Combined";
        mesh.CombineMeshes(combine);
        return mesh;
    }
    private void Start()
    {
        GetComponent<MeshFilter>().sharedMesh = MergeMeshFilters(meshFilters, transform.root, true);
    }
}
