using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Magazine : AmmoContainer
{
    public Mesh fullMesh;
    public Mesh simpleMesh;
    public int[] markers = new int[0];
    public MeshRenderer[] markerRenderers = new MeshRenderer[0];

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
    }
    public override bool EjectRound()
    {
        if (!base.EjectRound()) return false;

        for (int i = 0; i < markers.Length; i++)
            if (ammo < markers[i]) markerRenderers[i].gameObject.SetActive(false);
        return true;
    }
}
/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Magazine : AmmoContainer
{
    private Mesh originalMesh;
    public Mesh fullMesh;
    public Mesh simpleMesh;
    public int[] markers = new int[0];
    public MeshRenderer[] markerRenderers = new MeshRenderer[0];

    private MeshFilter filter;

    private bool merged;

    public void UnMerge()
    {
        return;
        if (!merged) return;
        merged = false;
        UpdateLOD(complex.lod.LOD());
    }
    private void UpdateLOD(int lod)
    {
        if (!merged)
        {
            foreach (MeshRenderer marker in markerRenderers) marker.enabled = lod == 0;
            filter.mesh = lod == 0 ? originalMesh : simpleMesh;
        }
        else filter.mesh = lod == 0 ? fullMesh : simpleMesh;
    }
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            filter = GetComponent<MeshFilter>();
            //complex.lod.OnSwitchEvent += UpdateLOD;
            return;
            if (markers.Length > 0)
            {
                originalMesh = filter.sharedMesh;
                filter.sharedMesh = complex.lod.LOD() == 0 ? fullMesh : simpleMesh;
                foreach (MeshRenderer marker in markerRenderers) marker.enabled = false;
                merged = true;
            }
        }
    }
    private void OnDestroy()
    {
        //complex.lod.OnSwitchEvent -= UpdateLOD;
    }
    public override bool EjectRound()
    {
        if (!base.EjectRound()) return false;

        for (int i = 0; i < markers.Length; i++)
            if (ammo < markers[i]) markerRenderers[i].gameObject.SetActive(false);

        if (markers.Length > 0) UnMerge();
        return true;
    }
}

 */