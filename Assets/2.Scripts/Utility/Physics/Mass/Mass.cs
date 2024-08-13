using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Mass
{
    public float mass;
    public Vector3 center;
    public Mass(float _mass, Vector3 _center)
    {
        mass = _mass;
        center = _center;
    }
    public Mass(IMassComponent part, bool empty)
    {
        float partMass = empty ? part.EmptyMass : part.LoadedMass;
        mass = partMass;
        SofComponent component = part as SofComponent;
        center = component.sofObject.transform.InverseTransformPoint(component.transform.position);
    }
    public Mass(IMassComponent[] massComponents, bool empty)
    {
        mass = 0f;
        center = Vector3.zero;

        foreach (IMassComponent massComponent in massComponents)
        {
            Mass partMass = new Mass(massComponent, empty);
            mass += partMass.mass;
            center += partMass.mass * partMass.center;
        }
        if (mass > 0f) center /= mass;
    }

    public static Vector3 InertiaMoment(IMassComponent[] massComponents, bool empty)
    {
        Vector3 inertiaMoment = Vector3.zero;
        foreach (IMassComponent massComponent in massComponents)
        {
            Transform tr = (massComponent as SofComponent).transform;
            Vector3 localPos = tr.root.InverseTransformPoint(tr.position);
            float x = new Vector2(localPos.y, localPos.z).sqrMagnitude;
            float y = new Vector2(localPos.x, localPos.z).sqrMagnitude;
            float z = new Vector2(localPos.x, localPos.y).sqrMagnitude;
            inertiaMoment += new Vector3(x, y, z) * (empty ? massComponent.EmptyMass : massComponent.LoadedMass);
        }
        return inertiaMoment;
    }
    public static Mass operator +(Mass m1, Mass m2)
    {
        float total = m1.mass + m2.mass;
        if (total <= 0f) return new Mass(0f, Vector3.zero);
        return new Mass(total, (m1.center * m1.mass + m2.center * m2.mass) / total);
    }
    public static Mass operator -(Mass m1, Mass m2)
    {
        m2.mass = -m2.mass;
        return m1 + m2;
    }
}
