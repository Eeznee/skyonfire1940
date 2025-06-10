using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkersManager : MonoBehaviour
{
    public static MarkersManager instance;

    public Marker markerPrefab;
    public Marker aircraftMarkerPrefab;

    [HideInInspector] public List<Marker> markers;


    public static bool AlliedMarkersEnabled { get; private set; }
    public static bool EnnemiesMarkersEnabled { get; private set; }

    private void Awake()
    {
        instance = this;
        AlliedMarkersEnabled = PlayerPrefs.GetInt("AlliedMarkers", 1) == 1;
        EnnemiesMarkersEnabled = PlayerPrefs.GetInt("EnnemiesMarkers", 1) == 1;
    }
    public static void Add(SofObject sofObj)
    {
        Marker marker;
        if (sofObj.aircraft != null)
        {
            marker = Instantiate(instance.aircraftMarkerPrefab, instance.transform);
        }
        else
        {
            marker = Instantiate(instance.markerPrefab, instance.transform);
        }
        marker.Init(sofObj);
        instance.markers.Add(marker);
    }

    private void Update()
    {
        SortMarkers();

        SetMarkersOpacity();
    }

    private void SortMarkers()
    {
        markers.Sort((a, b) => a.SqrDistance.CompareTo(b.SqrDistance));
    }

    int currentMarkerToUpdate = 0;

    private void SetMarkersOpacity()
    {
        int i = currentMarkerToUpdate;
        currentMarkerToUpdate = (currentMarkerToUpdate + 1) % markers.Count;

        if (!markers[i].ShouldBeVisible()) return;

        Marker marker = markers[i];

        marker.reticleOverlapOpacity = 1f;
        marker.textOverlapOpacity = 1f;

        for (int j = 0; j < markers.Count; j++)
        {
            if (j == i) continue;
            if (!markers[j].ShouldBeVisible()) continue;

            CheckOverlapAndMultiplyOpacity(marker, markers[j]);
        }
    }
    private void CheckOverlapAndMultiplyOpacity(Marker marker, Marker otherMarker)
    {
        float textOverlapFactor = RectTransformExtensions.RectsOverlapFactor(marker.TextBound, otherMarker.ReticleBound);
        marker.textOverlapOpacity *= Mathf.Clamp01(1f - textOverlapFactor * 8f);

        if (marker.SqrDistance > otherMarker.SqrDistance)
        {
            textOverlapFactor = RectTransformExtensions.RectsOverlapFactor(marker.TextBound, otherMarker.TextBound);
            marker.textOverlapOpacity *= Mathf.Clamp01(1f - textOverlapFactor * 8f * otherMarker.textOverlapOpacity);

            float reticleOverlapFactor = RectTransformExtensions.RectsOverlapFactor(marker.ReticleBound, otherMarker.ReticleBound);
            marker.reticleOverlapOpacity *= Mathf.Clamp01(1f - reticleOverlapFactor * 2f);
        }
    }
}
