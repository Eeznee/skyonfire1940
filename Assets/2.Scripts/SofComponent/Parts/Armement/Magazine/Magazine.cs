using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Magazine : AmmoContainer
{
    [System.Serializable]
    public class MagazineAmmoMarker
    {
        public int minAmmo;
        public Mesh mesh;
    }
    public Mesh simpleMeshForStorage;
    private MeshFilter filter;

    public MagazineAmmoMarker[] markers;
    private int currentMarker;
    private bool useMarkers;

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        filter = GetComponent<MeshFilter>();
        useMarkers = markers.Length > 1 && filter;
    }
    public override void Rearm()
    {
        base.Rearm();
        ShowMarker(markers.Length - 1);
    }
    public void ShowMarker(int id)
    {
        if (!useMarkers || currentMarker == id) return;
        currentMarker = id;

        filter.sharedMesh = markers[id].mesh;
    }
    public override bool EjectRound()
    {
        if (!base.EjectRound()) return false;

        if (useMarkers && ammo < markers[currentMarker].minAmmo)
            ShowMarker(currentMarker - 1);
        return true;
    }
    public Vector3 MagTravelPos(Vector3 startPos, Vector3 endPos, float animTime)
    {
        float distance = (startPos - endPos).magnitude;
        float t = animTime * animTime;
        if (animTime > 0.2f) t = Mathf.Lerp(0.04f, 1f, (animTime - 0.2f) / 0.8f);
        Vector3 travelOffset = (endPos - (startPos + ejectVector)) * t;
        return travelOffset + startPos + ejectVector * Mathf.Clamp01(animTime * distance * 3f);
    }
}