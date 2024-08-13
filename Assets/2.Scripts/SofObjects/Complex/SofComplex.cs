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

    public List<SofComponent> components = new List<SofComponent>();
    public List<IMassComponent> massComponents = new List<IMassComponent>();
    public List<SofModule> modules = new List<SofModule>();
    public List<IDamageTick> damageTickers = new List<IDamageTick>();
    public List<SofAirframe> airframes = new List<SofAirframe>();
    public CrewMember[] crew;

    protected override void SetReferences()
    {
        base.SetReferences();

        data = this.GetCreateComponent<ObjectData>();
    }
    public override void EditorInitialization()
    {
        base.EditorInitialization();
        GetSofComponentsAndSetReferences();
    }
    protected override void GameInitialization()
    {
        avm = transform.CreateChild("Audio Visual Manager").gameObject.AddComponent<ObjectAudio>();

        base.GameInitialization();
        GetSofComponentsAndSetReferences();
        InitializeSofComponents();

        SetMassFromParts();
        SetRigidbody();

        InvokeRepeating("DamageTick", damageTickInterval, damageTickInterval);
    }

    protected virtual void GetSofComponentsAndSetReferences()
    {
        SofComponent[] allComponents = GetComponentsInChildren<SofComponent>();

        components = new List<SofComponent>(allComponents);
        massComponents = new List<IMassComponent>(GetComponentsInChildren<IMassComponent>());
        modules = new List<SofModule>(GetComponentsInChildren<SofModule>());
        damageTickers = new List<IDamageTick>(GetComponentsInChildren<IDamageTick>());
        airframes = new List<SofAirframe>(GetComponentsInChildren<SofAirframe>());
        crew = GetComponentsInChildren<CrewMember>();

        foreach (SofComponent component in allComponents)
            component.SetReferences(this);
    }
    protected virtual void InitializeSofComponents()
    {
        if (!Application.isPlaying) return;
        if (GetComponent<SofDebris>()) return;

        foreach (SofComponent component in components.ToArray())
            component.Initialize(this);
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
        if (mass.mass < 1f) mass.mass = 1f;
        complex.rb.mass = mass.mass;
        if (centerOfMass) complex.rb.centerOfMass = mass.center;
    }
    public void SetMassFromParts()
    {
        mass = new Mass(0f, Vector3.zero);
        foreach (IMassComponent massComponent in massComponents)
            mass += new Mass(massComponent, false);

        UpdateRbMass(true);
        ResetInertiaTensor();
    }
    public void ResetInertiaTensor()
    {
        if (aircraft != null)
        {
            Vector3 inertiaTensor = Mass.InertiaMoment(complex.massComponents.ToArray(), true);
            inertiaTensor *= 1.1f;
            complex.rb.inertiaTensor = inertiaTensor;
        }
        else complex.rb.ResetInertiaTensor();
    }
    public void Repair()
    {
        foreach (SofModule module in modules) module.Repair();
        burning = false;
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
        IMassComponent massComponent = component as IMassComponent;
        if (massComponent != null) massComponents.Add(massComponent);
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
        IMassComponent massComponent = component as IMassComponent;
        if (massComponent != null) massComponents.Remove(massComponent);
        SofModule module = component as SofModule;
        if (module) modules.Remove(module);
        SofAirframe airframe = component as SofAirframe;
        if (airframe) airframes.Remove(airframe);
        IDamageTick damageTicker = component as IDamageTick;
        if (damageTicker != null) damageTickers.Remove(damageTicker);
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
        EditorInitialization();
    }
#endif
}