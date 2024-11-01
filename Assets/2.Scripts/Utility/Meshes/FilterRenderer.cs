using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FilterRenderer
{
    public MeshFilter filter { get; private set; }
    public Renderer rend { get; private set; }

    public FilterRenderer(GameObject gameObject)
    {
        filter = gameObject.GetComponent<MeshFilter>();
        rend = gameObject.GetComponent<Renderer>();
    }
    public FilterRenderer(Component mono)
    {
        filter = mono.GetComponent<MeshFilter>();
        rend = mono.GetComponent<Renderer>();
    }
    public bool IsMergeable(Material requiredMaterial)
    {
        if (rend.GetComponent<Propeller>() || rend.GetComponentInParent<Propeller>()) return false;
        if (rend.GetComponentInParent<GunMount>()) return false;
        if (rend.GetComponent<MagazineStorage>()) return false;
        if (rend.GetComponent<Bomb>()) return false;
        if (rend.sharedMaterial != requiredMaterial) return false;
        if (rend.GetComponentInParent<Cockpit>()) return false;
        return true;
    }
    public bool IsMobile(HydraulicSystem[] hydraulics, Transform[] mobileFiltersExceptions)
    {
        if (rend.GetComponent<ControlSurface>()) return true;
        if (rend.GetComponent<Flap>()) return true;
        if (rend.GetComponent<Slat>()) return true;
        if (rend.GetComponent<Wheel>()) return true;
        if (rend.GetComponent<Suspension>()) return true;
        if (rend.GetComponent<GunsCover>()) return true;
        if (rend.GetComponent<GunMount>()) return true;

        foreach (HydraulicSystem hydraulic in hydraulics)
            if (hydraulic.IsAnimated(rend.transform)) return true;

        foreach (Transform exception in mobileFiltersExceptions)
            if (exception && rend.transform.IsChildOf(exception)) return true;

        return false;
    }
}
