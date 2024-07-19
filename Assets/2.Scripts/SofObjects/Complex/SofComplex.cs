using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SofComplex : SofObject
{
    public override int DefaultLayer()
    {
        return 9;
    }

    public float cogForwardDistance = 0f;
    public float targetEmptyMass = 3000f;

    protected Mass mass;

    public ObjectLOD lod;
    public ObjectBubble bubble;
    public ObjectData data;
    public ObjectAudio avm;

    public event Action<SofComplex> onPartDetached;

    public List<SofComponent> components;
    public List<SofPart> parts;
    public List<SofModule> modules;
    public List<IDamageTick> damageTickers;
    public List<SofAirframe> airframes;
    public CrewMember[] crew;

    public override void SetReferences()
    {
        base.SetReferences();

        data = this.GetCreateComponent<ObjectData>();

        crew = GetComponentsInChildren<CrewMember>();

        components = new List<SofComponent>(GetComponentsInChildren<SofComponent>());
        parts = new List<SofPart>(GetComponentsInChildren<SofPart>());
        modules = new List<SofModule>(GetComponentsInChildren<SofModule>());
        damageTickers = new List<IDamageTick>(GetComponentsInChildren<IDamageTick>());
        airframes = new List<SofAirframe>(GetComponentsInChildren<SofAirframe>());

        foreach (SofComponent component in components.ToArray())
            component.SetReferences(this);
    }
    public void Repair()
    {
        foreach (SofModule module in modules) module.Repair();
        burning = false;
    }
    protected override void Initialize()
    {
        avm = transform.CreateChild("Audio Visual Manager").gameObject.AddComponent<ObjectAudio>();

        base.Initialize();

        bool debris = GetComponent<SofDebris>();

        foreach (SofComponent component in components.ToArray())
        {
            component.AttachNewComplex(complex);
            if (!debris) component.Initialize(this);
        }

        SetMassFromParts();
        SetRigidbody();

        //InvokeRepeating("DamageTick", damageTickInterval, damageTickInterval);
    }
    const float damageTickInterval = 0.5f;
    private void SetRigidbody()
    {
        if (rb == GameManager.gm.mapmap.rb) return;
        rb.angularDrag = 0f;
        rb.drag = 0f;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Extrapolate;
    }
    public float GetMass() { return mass.mass; }
    public Vector3 GetCenterOfMass() { return mass.center; }
    public void ShiftMass(float shift) { mass.mass += shift; UpdateRbMass(false); }
    public void ShiftMass(Mass shift) { mass += shift; UpdateRbMass(true); }
    public void UpdateRbMass(bool centerOfMass)
    {
        complex.rb.mass = mass.mass;
        if (centerOfMass) complex.rb.centerOfMass = mass.center;
    }
    public void SetMassFromParts()
    {
        mass = new Mass(0f, Vector3.zero);
        foreach (SofPart part in parts)
            mass += new Mass(part, false);

        UpdateRbMass(true);
        ResetInertiaTensor();
    }
    public void ResetInertiaTensor()
    {
        if (aircraft != null)
        {
            Vector3 inertiaTensor = Mass.InertiaMoment(complex.parts.ToArray(), true);
            inertiaTensor *= 1.1f;
            complex.rb.inertiaTensor = inertiaTensor;
        }
        else complex.rb.ResetInertiaTensor();
    }
    private void DamageTick()
    {
        foreach (IDamageTick damageTicker in damageTickers.ToArray())
            damageTicker.DamageTick(damageTickInterval);
    }
    public override void Explosion(Vector3 center, float tnt)
    {
        base.Explosion(center, tnt);
        if (!bubble) return;
        float sqrDis = (center - transform.position).sqrMagnitude;
        if (tnt * 2000f < sqrDis) return;   //no calculations if too far
        foreach (SofModule m in modules.ToArray()) if (m) m.ExplosionDamage(center, tnt);
    }
    public void RegisterComponent(SofComponent component)
    {
        components.Add(component);
        SofPart part = component as SofPart;
        if (part) parts.Add(part);
        SofModule module = component as SofModule;
        if (module) modules.Add(module);
        SofAirframe airframe = component as SofAirframe;
        if (airframe) airframes.Add(airframe);
        IDamageTick damageTicker = component as IDamageTick;
        if (damageTicker != null) damageTickers.Add(damageTicker);
    }
    public void RemoveComponent(SofComponent component)
    {
        components.Remove(component);
        SofPart part = component as SofPart;
        if (part) parts.Remove(part);
        SofModule module = component as SofModule;
        if (module) modules.Remove(module);
        SofAirframe airframe = component as SofAirframe;
        if (airframe) airframes.Remove(airframe);
    }
    public void OnPartDetach(SofComplex detachedDebris)
    {
        ShiftMass(new Mass(-detachedDebris.mass.mass, detachedDebris.mass.center));
        if (mass.mass <= 0f) Debug.LogError(name + ": Mass below zero", gameObject);

        foreach (SofComponent component in detachedDebris.components) RemoveComponent(component);

        onPartDetached?.Invoke(detachedDebris);
    }

    private bool submerged = false;
    protected void WaterPhysics()
    {
        if (data.altitude.Get < 5f) foreach (SofAirframe airframe in airframes) if (airframe) airframe.Floating();

        bool newSubmerged = data.altitude.Get < 1f;
        if (newSubmerged != submerged)
        {
            submerged = newSubmerged;
            rb.angularDrag = submerged ? 0.5f : 0f;
            rb.drag = submerged ? 1f : 0f;
        }
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        SetReferences();
    }
#endif
}