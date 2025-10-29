using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubsurfacesCollection<T> where T : Subsurface
{
    private T[] subsurfaces;
    private float[] overlaps;

    public float TotalOverlap { get; private set; }
    public T MainSurface { get; private set; }

    public SubsurfacesCollection(MainSurface mainSurface)
    {
        T[] possibleSubSurfaces = mainSurface.SubSurfaceParent.GetComponentsInChildren<T>();

        List<T> subSurfaceList = new List<T>();
        List<float> overlapsList = new List<float>();


        foreach (T surface in possibleSubSurfaces)
        {
            surface.UpdateAerofoil();
            float overlap = SurfaceQuad.Overlap(mainSurface.quad, surface.quad);
            if (overlap > 0.1f)
            {
                subSurfaceList.Add(surface);
                overlapsList.Add(overlap);
            }
        }

        subsurfaces = subSurfaceList.ToArray();
        overlaps = overlapsList.ToArray();

        UpdateTotalOverlapAndMain();
    }

    public void CheckSubsurfacesArray()
    {
        foreach (Subsurface surface in subsurfaces)
        {
            if (!surface || surface.ripped)
            {
                UpdateSubsurfacesArray();
                return;
            }
        }
    }
    private void UpdateSubsurfacesArray()
    {
        List<T> subSurfaceList = new List<T>();
        List<float> overlapsList = new List<float>();

        for(int i = 0; i < subsurfaces.Length; i++)
        {
            T surface = subsurfaces[i];

            if(surface && !surface.ripped)
            {
                subSurfaceList.Add(surface);
                overlapsList.Add(overlaps[i]);
            }
        }

        subsurfaces = subSurfaceList.ToArray();
        overlaps = overlapsList.ToArray();

        UpdateTotalOverlapAndMain();
    }
    private void UpdateTotalOverlapAndMain()
    {
        MainSurface = null;
        float highestOverlap = 0f;
        TotalOverlap = 0f;

        for (int i = 0; i < subsurfaces.Length; i++)
        {
            T surface = subsurfaces[i];

            TotalOverlap += overlaps[i];
            if (overlaps[i] > highestOverlap) MainSurface = surface;
        }

        TotalOverlap = Mathf.Clamp01(TotalOverlap);
    }

    public bool None
    {
        get
        {
            CheckSubsurfacesArray();
            if (subsurfaces == null) return true;
            if (subsurfaces.Length == 0) return true;
            return false;
        }
    }
}
