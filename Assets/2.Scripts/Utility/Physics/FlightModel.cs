using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FlightModel
{
    const float fireMaxDmg = 1.5f;
    public static Transform AirfoilShapeTransform(Transform transform,Transform tr)
    {
        if (!tr || tr.parent != transform || tr == transform) tr = transform.Find(transform.name + " Shape");
        if(!tr) tr = new GameObject().transform;
        tr.parent = transform;
        tr.gameObject.SetActive(false);
        tr.name = transform.name + " Shape";
        return tr;
    }
}
[System.Serializable]
public class Mass
{
    public float mass;
    public Vector3 center;
    public Mass(float _mass, Vector3 _center)
    {
        mass = _mass;
        center = _center;
    }
    public Mass(Part[] parts, bool empty)
    {
        mass = 0f;
        center = Vector3.zero;

        foreach(Part part in parts)
        {
            float partMass = empty ? part.EmptyMass() : part.Mass();
            mass += partMass;
            center += partMass * part.transform.root.InverseTransformPoint(part.transform.position);
        }
        if (mass > 0f) center /= mass;
    }

    public static Vector3 InertiaMoment(Part[] parts, bool empty)
    {
        Vector3 inertiaMoment = Vector3.zero;
        foreach (Part part in parts)
        {
            Vector3 localPos = part.transform.root.InverseTransformPoint(part.transform.position);
            float x = new Vector2(localPos.y, localPos.z).sqrMagnitude;
            float y = new Vector2(localPos.x, localPos.z).sqrMagnitude;
            float z = new Vector2(localPos.x, localPos.y).sqrMagnitude;
            inertiaMoment += new Vector3(x, y, z) * (empty ? part.EmptyMass() : part.Mass());
        }
        return inertiaMoment;
    }

    public static Mass ApproximateMass(AirframeBase[] airframes)
    {
        Mass approximated = new Mass(0f, Vector3.zero);
        foreach (AirframeBase airframe in airframes)
        {
            Vector3 localPos = airframe.transform.root.InverseTransformPoint(airframe.transform.position);
            approximated.mass += airframe.ApproximateMass();
            approximated.center += localPos * airframe.ApproximateMass();
        }
        approximated.center /= approximated.mass;
        return approximated;
    }
    public static void ComputeAutoMass(SofObject sofObject, Mass targetEmptyMass) 
    {
        Part[] parts = sofObject.GetComponentsInChildren<Part>();
        AirframeBase[] airframes = sofObject.GetComponentsInChildren<AirframeBase>();
        foreach (AirframeBase airframe in airframes)
            airframe.emptyMass = airframe.ApproximateMass();
        Mass fixedMass = new Mass(parts, true) - new Mass(airframes, true);
        Mass targetAirframeMass = targetEmptyMass - fixedMass;
        if (targetAirframeMass.mass < 0f) Debug.LogError("Target mass is too small, fixed mass parts already go above that weight !", sofObject);

        //Approximate airframe mass to match target mass
        Mass approximated = ApproximateMass(airframes);
        float factor = targetAirframeMass.mass / approximated.mass;
        foreach (AirframeBase airframe in airframes)
            airframe.emptyMass = factor * airframe.ApproximateMass();

        //Balance front and back to match target center of gravity
        List<AirframeBase> frontFrames = new List<AirframeBase>();
        List<AirframeBase> backFrames = new List<AirframeBase>();
        foreach (AirframeBase airframe in airframes)
        {
            Vector3 localPos = airframe.transform.root.InverseTransformPoint(airframe.transform.position);
            if (localPos.z > targetAirframeMass.center.z)
                frontFrames.Add(airframe);
            else if (localPos.z < targetAirframeMass.center.z)
                backFrames.Add(airframe);
        }
        if (frontFrames.Count == 0 || backFrames.Count == 0) Debug.LogError("Target center of gravity is impossible to reach");
        Mass front = new Mass(frontFrames.ToArray(), true);
        Mass back = new Mass(backFrames.ToArray(), true);
        
        float centerShiftMass = (front.mass * (targetAirframeMass.center.z - front.center.z) + back.mass * (targetAirframeMass.center.z - back.center.z)) / (front.center.z - back.center.z);
        float frontFactor = 1f + centerShiftMass / front.mass;
        float backFactor = 1f - centerShiftMass / back.mass;

        foreach (AirframeBase airframe in frontFrames)
            airframe.emptyMass *= frontFactor;
        foreach (AirframeBase airframe in backFrames)
            airframe.emptyMass *= backFactor;
    }

    public static Mass operator +(Mass m1, Mass m2)
    {
        float total = m1.mass + m2.mass;
        return new Mass(total,(m1.center * m1.mass + m2.center * m2.mass)/total);
    }
    public static Mass operator -(Mass m1, Mass m2)
    {
        float total = m1.mass - m2.mass;
        if (total < 0f) return new Mass(0f, Vector3.zero);
        return new Mass(total, (m1.center * m1.mass - m2.center * m2.mass) / total);
    }
}
