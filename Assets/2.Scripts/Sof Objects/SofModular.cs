using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SofModular : SofObject
{
    public override int DefaultLayer()
    {
        return 9;
    }


    public ObjectLOD lod;
    public ObjectAudio objectAudio;

    [NonSerialized] public ObjectData data;

    public event Action<SofComponent> onComponentAdded;
    public event Action<SofComponent> onComponentRootRemoved;

    public List<SofComponent> components = new List<SofComponent>();
    public List<SofModule> modules = new List<SofModule>();
    public List<IDamageTick> damageTickers = new List<IDamageTick>();
    public List<SofAirframe> airframes = new List<SofAirframe>();
    public CrewMember[] crew;

    public Action OnAttachPlayer;
    public Action OnDetachPlayer;
    public Action OnInitialize;
    public Action OnFixedUpdate;



    public virtual int SquadronId => GameManager.squadrons.Count;
    public virtual int PlaceInSquad => GameManager.crewedModulars.IndexOf(this);

    public override void SetReferences()
    {
        base.SetReferences();

        SofComponent[] allComponents = GetComponentsInChildren<SofComponent>();

        components = new List<SofComponent>(allComponents);
        modules = new List<SofModule>(GetComponentsInChildren<SofModule>());
        damageTickers = new List<IDamageTick>(GetComponentsInChildren<IDamageTick>());
        airframes = new List<SofAirframe>(GetComponentsInChildren<SofAirframe>());
        crew = GetComponentsInChildren<CrewMember>();

        foreach (SofComponent component in allComponents)
            component.SetReferences(this);

        data ??= new ObjectData(this);
    }
    protected virtual void InitializeImportantComponents()
    {
        objectAudio = GetComponentInChildren<ObjectAudio>();
        if(objectAudio == null) objectAudio = transform.CreateChild("Object Audio").gameObject.AddComponent<ObjectAudio>();
        InvokeRepeating("DamageTick",UnityEngine.Random.Range(0f, damageTickInterval), damageTickInterval);
    }
    protected virtual void InitializePhysics()
    {

    }
    protected virtual void InitializeReferencesAndPlayer()
    {

        if (Player.modular == this)
        {
            OnAttachPlayer?.Invoke();
            Player.crew.OnAttachPlayer?.Invoke();
        }
    }
    protected override void GameInitialization()
    {
        InitializeImportantComponents();

        base.GameInitialization();

        InitializeSofComponents();

        InitializePhysics();

        InitializeReferencesAndPlayer();

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
    public void Repair()
    {
        foreach (SofModule module in modules) module.Repair();
        Burning = false;
    }
    private void DamageTick()
    {
        foreach (IDamageTick damageTicker in damageTickers.ToArray())
            damageTicker.DamageTick(damageTickInterval);
    }
    public virtual void AddInstantiatedComponent(SofComponent component)
    {
        if (!component.transform.IsChildOf(transform))
        {
            Debug.LogError("There was an attempt to attach a component that is not in the hierarchy of its complex", gameObject);
            return;
        }

        components.Add(component);

        SofModule module = component as SofModule;
        if (module) modules.Add(module);
        SofAirframe airframe = component as SofAirframe;
        if (airframe) airframes.Add(airframe);
        IDamageTick damageTicker = component as IDamageTick;
        if (damageTicker != null) damageTickers.Add(damageTicker);

        onComponentAdded?.Invoke(component);
    }
    public virtual SofComponent[] RemoveComponentRoot(SofComponent rootComponent)
    {
        SofComponent[] debrisComponents = rootComponent.GetComponentsInChildren<SofComponent>();

        foreach (SofComponent component in debrisComponents)
        {
            if (!components.Remove(component))
            {
                Debug.LogError("There was an attempt to remove a component that does not belong to a complex", gameObject);
                return debrisComponents;
            }

            SofModule module = component as SofModule;
            if (module) modules.Remove(module);
            SofAirframe airframe = component as SofAirframe;
            if (airframe) airframes.Remove(airframe);
            IDamageTick damageTicker = component as IDamageTick;
            if (damageTicker != null) damageTickers.Remove(damageTicker);
        }

        onComponentRootRemoved?.Invoke(rootComponent);

        return debrisComponents;
    }
    private bool submerged = false;
    protected void WaterPhysics()
    {
        if (data.altitude.Get < 5f) foreach (SofAirframe airframe in airframes) if (airframe) airframe.Floating();

        bool newSubmerged = data.altitude.Get < 1f && rb;
        if (newSubmerged != submerged)
        {
            submerged = newSubmerged;
            rb.angularDamping = submerged ? 0.5f : 0f;
            rb.linearDamping = submerged ? 1f : 0f;
        }
    }

    protected void OnValidate()
    {
        if (Application.isPlaying) return;
        SetReferences();
    }
}