using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FlightModel
{
    const float fireMaxDmg = 1.5f;
    public static float[] BurningCollateralRatios(Part part)
    {
        Part[] parts = part.data.parts;
        float[] dmgRatios = new float[parts.Length];
        for(int i = 0; i < parts.Length; i++) 
        {
            float distance = (parts[i].transform.position - part.transform.position).magnitude;
            if (distance < fireMaxDmg) dmgRatios[i] = Random.Range(0.7f, 1.4f);
            else dmgRatios[i] = Mathf.Pow(2f, -distance + fireMaxDmg) * Random.Range(0.7f,1.4f);
        }
        return dmgRatios;
    }
    public static float OverSpeedCoeff(bool parent,bool child)
    {
        float coeff = 1f;
        if (!parent) coeff += 0.1f;
        if (child) coeff += 0.1f;
        return coeff;
    }
    public static float OverGCoeff(bool parent,bool child)
    {
        float coeff = 1f;
        if (parent) coeff += 0.15f;
        if (!child) coeff += 0.15f;
        return coeff;
    }
    public static Transform AirfoilShapeTransform(Transform transform,Transform tr)
    {
        if (!tr || tr.parent != transform || tr == transform) tr = transform.Find(transform.name + " Shape");
        if(!tr) tr = new GameObject().transform;
        tr.parent = transform;
        tr.gameObject.SetActive(false);
        tr.name = transform.name + " Shape";
        return tr;
    }
    public static float TotalMass(Part[] parts,bool empty)
    {
        float total = 0f;
        foreach (Part p in parts)
            total += empty ? p.EmptyMass() : p.Mass();
        return total;
    }

    public static float StaticMoment(Part obj)
    {
        Part[] attachedPart = obj.GetComponentsInChildren<Part>();
        float load = 0f;
        foreach (Part p in attachedPart)
            load += p.Mass() * (p.transform.position - obj.transform.position).magnitude;
        return load + obj.Mass();
    }
}
