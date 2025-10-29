using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Weapons/Guns/Magazine")]
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
    public bool rotatesWhenFired = false;
    public float rotatesWhenFiredAngle = 15f;
    private int currentMarker;
    private bool useMarkers;

    public override void SetReferences(SofModular _complex)
    {
        base.SetReferences(_complex);
        filter = GetComponent<MeshFilter>();
        useMarkers = markers.Length > 1 && filter;
    }
    public override void InsertThisMagazine(Gun gun)
    {
        if (rotatesWhenFired)
        {
            gun.OnEjectEvent += RotateMagazine;
        }
        base.InsertThisMagazine(gun);
    }
    public override void UnloadThisMagazine()
    {
        if (attachedGun && rotatesWhenFired)
        {
            attachedGun.OnEjectEvent -= RotateMagazine;
        }
        base.UnloadThisMagazine();
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

    void RotateMagazine()
    {
        StartCoroutine(RotateMagazineWhenFired());
    }
    const float rotateTime = 0.035f;
    IEnumerator RotateMagazineWhenFired()
    {
        float portion = 0f;
        while (portion < 1f)
        {
            float newPortion = Mathf.Clamp01(portion + Time.deltaTime / rotateTime);
            float delta = newPortion - portion;
            portion = newPortion;

            transform.Rotate(Vector3.up * rotatesWhenFiredAngle * delta);
            yield return null;
        }
        yield return null;
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