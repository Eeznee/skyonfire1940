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
    public class SofMesh
    {
        public Mesh mesh;

        public static Mesh RemoveMesh(Mesh mesh,List<Mesh> subMeshes,int indexToRemove)
        {
            Mesh removedMesh = subMeshes[indexToRemove];

            int vertIndex = 0;
            int trisIndex = 0;

            for (int i = 0; i < indexToRemove; i++)
            {
                vertIndex += subMeshes[i].vertices.Length;
                trisIndex += subMeshes[i].triangles.Length;
            }

            mesh.vertices = IsniArrays.RemoveElementsFromArray(mesh.vertices, vertIndex, removedMesh.vertices.Length);
            mesh.triangles = IsniArrays.RemoveElementsFromArray(mesh.triangles, trisIndex, removedMesh.triangles.Length);

            return mesh;
        }
    }
}
