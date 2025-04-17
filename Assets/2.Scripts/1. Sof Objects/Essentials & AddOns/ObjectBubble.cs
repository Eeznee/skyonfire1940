using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBubble : SofComponent
{
    public enum CollidersGroup
    {
        All,
        Solid,
        None
    }
    public override int DefaultLayer()
    {
        return 10;
    }

    public SphereCollider bubble;

    private List<Collider> colliders = new List<Collider>();
    private List<Collider> solidColliders = new List<Collider>();
    private float toggledTimer = 0f;

    public CollidersGroup activeColliders { get; private set; }

    const float timerThreshold = 5f;

    private void AddCollidersArray(Collider[] collidersToAdd)
    {
        foreach (Collider col in collidersToAdd)
        {
            if (col.gameObject.layer == 9)
            {
                colliders.Add(col);
                if (!col.isTrigger) solidColliders.Add(col);
            }
        }
    }

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        colliders = new List<Collider>();
        solidColliders = new List<Collider>();
        AddCollidersArray(transform.root.GetComponentsInChildren<Collider>());

        bubble = gameObject.AddComponent<SphereCollider>();
        bubble.isTrigger = true;
        bubble.radius = aircraft.stats.wingSpan * 0.5f + 5f;

        complex.onComponentRootRemoved += RemoveDetachedPartColliders;
        complex.onComponentAdded += AddInstantiatedCollider;

        foreach (MeshCollider col in aircraft.GetComponentsInChildren<MeshCollider>())
        {
            col.convex = true;
            col.sharedMaterial = StaticReferences.Instance.aircraftPhysicMaterial;
        }

        foreach (Collider col in colliders)
        {
            if (col)
            {
                col.enabled = true;
            }
        }
        //DisableColliders();
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 10 || other.gameObject.layer == 0) EnableColliders(true);
    }
    private void AddInstantiatedCollider(SofComponent component)
    {
        AddCollidersArray(component.GetComponentsInChildren<Collider>());
    }
    private void RemoveDetachedPartColliders(SofComponent component)
    {
        foreach(Collider collider in component.GetComponents<Collider>())
        {
            colliders.Remove(collider);
            if (!collider.isTrigger) solidColliders.Remove(collider);

            collider.enabled = !collider.isTrigger;
        }
    }
    public void EnableColliders(bool solidOnly)
    {
        return;
        toggledTimer = timerThreshold;

        if ((solidOnly && activeColliders == CollidersGroup.Solid) || activeColliders == CollidersGroup.All) return;

        activeColliders = solidOnly ? CollidersGroup.Solid : CollidersGroup.All;

        foreach (Collider col in (solidOnly ? solidColliders : colliders))
        {

            if (col)
            {
                col.enabled = true;
            }
        }

    }
    public void DisableColliders()
    {
        return;
        foreach (Collider col in colliders)
        {
            if (col)
                col.enabled = false;
            else Debug.LogError("Probably have a collider in the cockpit with the wrong layer");
        }

        toggledTimer = 0f;
        activeColliders = CollidersGroup.None;
    }
    void Update()
    {
        return;
        if (aircraft.data.relativeAltitude.Get < 15f) EnableColliders(true);
        if (activeColliders != CollidersGroup.None && toggledTimer < Time.deltaTime) DisableColliders();

        toggledTimer -= Time.deltaTime;
    }
}
