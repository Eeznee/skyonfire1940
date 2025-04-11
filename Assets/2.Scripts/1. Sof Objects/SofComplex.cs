using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SofComplex : SofObject
{
    public override int DefaultLayer()
    {
        return 9;
    }

    public float cogForwardDistance = 0f;
    public float targetEmptyMass = 3000f;

    public Mass EmptyMass;
    public Mass LoadedMass;
    protected Mass realMass;

    public ObjectLOD lod;
    public ObjectBubble bubble;
    public ObjectAudio avm;

    [NonSerialized] public ObjectData data;

    public event Action<SofComponent> onComponentAdded;
    public event Action<SofComponent> onComponentRootRemoved;

    public List<SofComponent> components = new List<SofComponent>();
    public List<IMassComponent> massComponents = new List<IMassComponent>();
    public List<SofModule> modules = new List<SofModule>();
    public List<IDamageTick> damageTickers = new List<IDamageTick>();
    public List<SofAirframe> airframes = new List<SofAirframe>();
    public CrewMember[] crew;

    public Action OnAttachPlayer;
    public Action OnDetachPlayer;
    public Action OnInitialize;
    public Action OnFixedUpdate;

    public override void SetReferences()
    {
        base.SetReferences();

        SofComponent[] allComponents = GetComponentsInChildren<SofComponent>();

        components = new List<SofComponent>(allComponents);
        massComponents = new List<IMassComponent>(GetComponentsInChildren<IMassComponent>());
        modules = new List<SofModule>(GetComponentsInChildren<SofModule>());
        damageTickers = new List<IDamageTick>(GetComponentsInChildren<IDamageTick>());
        airframes = new List<SofAirframe>(GetComponentsInChildren<SofAirframe>());
        crew = GetComponentsInChildren<CrewMember>();

        foreach (SofComponent component in allComponents)
            component.SetReferences(this);

        data ??= new ObjectData(this);

        EmptyMass = new Mass(massComponents.ToArray(), MassCategory.Empty);
        LoadedMass = new Mass(massComponents.ToArray(), MassCategory.Loaded);
    }
    protected override void GameInitialization()
    {
        avm = transform.CreateChild("Audio Visual Manager").gameObject.AddComponent<ObjectAudio>();


        base.GameInitialization();

        InitializeSofComponents();
        RecomputeRealMass();

        InvokeRepeating("DamageTick", damageTickInterval, damageTickInterval);

        if (Player.complex == this)
        {
            OnAttachPlayer?.Invoke();
            Player.crew.OnAttachPlayer?.Invoke();
        }

        OnInitialize?.Invoke();
    }

    protected virtual void FixedUpdate()
    {
        OnFixedUpdate?.Invoke();
    }

    protected virtual void InitializeSofComponents()
    {
        if (!Application.isPlaying) return;
        if (GetComponent<SofDebris>()) return;

        foreach (SofComponent component in components.ToArray())
            component.Initialize(this);
    }
    const float damageTickInterval = 0.5f;

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
    public void ResetInertiaTensor()
    {
        if (aircraft != null)
        {
            Vector3 inertiaTensor = Mass.InertiaMoment(massComponents.ToArray(), MassCategory.Real);
            inertiaTensor *= 1.1f;
            rb.inertiaTensor = inertiaTensor;
        }
        else rb.ResetInertiaTensor();
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
    public void AddInstantiatedComponent(SofComponent component)
    {
        if (!component.transform.IsChildOf(transform))
        {
            Debug.LogError("There was an attempt to attach a component that is not in the hierarchy of its complex", gameObject);
            return;
        }

        components.Add(component);
        IMassComponent massComponent = component as IMassComponent;
        if (massComponent != null) massComponents.Add(massComponent);
        SofModule module = component as SofModule;
        if (module) modules.Add(module);
        SofAirframe airframe = component as SofAirframe;
        if (airframe) airframes.Add(airframe);
        IDamageTick damageTicker = component as IDamageTick;
        if (damageTicker != null) damageTickers.Add(damageTicker);

        onComponentAdded?.Invoke(component);
    }
    public void RemoveComponentRoot(SofComponent rootComponent)
    {
        SofComponent[] debrisComponents = rootComponent.GetComponentsInChildren<SofComponent>();

        foreach (SofComponent component in debrisComponents)
        {
            if (!components.Remove(component))
            {
                Debug.LogError("There was an attempt to remove a component that does not belong to a complex", gameObject);
                return;
            }

            IMassComponent massComponent = component as IMassComponent;
            if (massComponent != null) massComponents.Remove(massComponent);
            SofModule module = component as SofModule;
            if (module) modules.Remove(module);
            SofAirframe airframe = component as SofAirframe;
            if (airframe) airframes.Remove(airframe);
            IDamageTick damageTicker = component as IDamageTick;
            if (damageTicker != null) damageTickers.Remove(damageTicker);

            IMassComponent imass = component.GetComponent<IMassComponent>();
            if (imass != null)
            {
                ShiftMass(-new Mass(imass, MassCategory.Real));
                if (realMass.mass <= 0f) Debug.LogError(name + ": Mass below zero", gameObject);
            }

        }
        onComponentRootRemoved?.Invoke(rootComponent);
    }
    private bool submerged = false;
    protected void WaterPhysics()
    {
        if (data.altitude.Get < 5f) foreach (SofAirframe airframe in airframes) if (airframe) airframe.Floating();

        bool newSubmerged = data.altitude.Get < 1f && rb;
        if (newSubmerged != submerged)
        {
            submerged = newSubmerged;
            rb.angularDrag = submerged ? 0.5f : 0f;
            rb.drag = submerged ? 1f : 0f;
        }
    }

    protected void OnValidate()
    {
        if (Application.isPlaying) return;
        SetReferences();
    }
}