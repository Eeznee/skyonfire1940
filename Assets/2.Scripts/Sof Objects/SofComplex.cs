using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SofComplex : SofModular
{
    public float cogForwardDistance = 0f;
    public float targetEmptyMass = 3000f;

    public Mass EmptyMass;
    public Mass LoadedMass;
    protected Mass realMass;

    public List<IMassComponent> massComponents = new List<IMassComponent>();

    public override void SetReferences()
    {
        base.SetReferences();

        massComponents = new List<IMassComponent>(GetComponentsInChildren<IMassComponent>());

        EmptyMass = new Mass(massComponents.ToArray(), MassCategory.Empty);
        LoadedMass = new Mass(massComponents.ToArray(), MassCategory.Loaded);
    }

    protected override void InitializePhysics()
    {
        base.InitializePhysics();
        RecomputeRealMass();
    }
    public float GetMass() { return realMass.mass; }
    public Vector3 GetCenterOfMass() { return realMass.center; }
    public void ShiftMass(float shift) { realMass.mass += shift; UpdateRbMass(false); }
    public void ShiftMass(Mass shift) { realMass += shift; UpdateRbMass(true); }
    public void UpdateRbMass(bool centerOfMass)
    {
        if (realMass.mass < 1f) realMass.mass = 1f;
        rb.mass = realMass.mass;
        if (centerOfMass) rb.centerOfMass = realMass.center;
    }
    public void RecomputeRealMass()
    {
        realMass = new Mass(massComponents.ToArray(), MassCategory.Real);

        UpdateRbMass(true);
        ResetInertiaTensor();
    }
    public virtual void ResetInertiaTensor()
    {
        if (aircraft != null)
        {
            Vector3 inertiaTensor = Mass.InertiaMoment(massComponents.ToArray(), MassCategory.Real);
            inertiaTensor *= 1.1f;
            rb.inertiaTensor = inertiaTensor;
        }
        else
            rb.ResetInertiaTensor();
    }
    public override void AddInstantiatedComponent(SofComponent component)
    {
        base.AddInstantiatedComponent(component);

        IMassComponent massComponent = component as IMassComponent;
        if (massComponent != null) massComponents.Add(massComponent);
    }
    public override SofComponent[] RemoveComponentRoot(SofComponent rootComponent)
    {
        SofComponent[] sofComponents = base.RemoveComponentRoot(rootComponent);

        foreach(SofComponent component in sofComponents)
        {
            IMassComponent massComponent = component as IMassComponent;
            if (massComponent != null) massComponents.Remove(massComponent);
        }

        RecomputeRealMass();

        return sofComponents;
    }
}
