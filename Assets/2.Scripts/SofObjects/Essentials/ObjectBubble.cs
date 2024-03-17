using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBubble : SofComponent
{
    public override int DefaultLayer()
    {
        return 10;
    }
    public float radius;
    public SphereCollider bubble;
    private List<Collider> colliders = new List<Collider>();
    private List<Collider> solidColliders = new List<Collider>();
    private float toggledTimer = 0f;

    private bool solidToggled;
    private bool allToggled;

    const float timerThreshold = 5f;

    private void AssignColliders()
    {
        colliders = solidColliders = new List<Collider>();
        foreach (Collider col in transform.root.GetComponentsInChildren<Collider>())
        {
            if (col.gameObject.layer == 9 && !col.GetComponent<WheelCollider>())
            {
                colliders.Add(col);
                if (!col.isTrigger) solidColliders.Add(col);
            }
        }
    }

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        AssignColliders();

        bubble = gameObject.AddComponent<SphereCollider>();
        bubble.isTrigger = true;
        bubble.radius = aircraft.stats.wingSpan * 0.5f + 5f;

        complex.onPartDetached += RemoveDetachedPartColliders;

        DisableColliders();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 10 || other.gameObject.layer == 0) ToggleColliders(true);
    }
    private void OnTriggerExit(Collider other)
    {

    }
    private void RemoveDetachedPartColliders(SofComplex debris)
    {
        foreach(Collider collider in debris.GetComponentsInChildren<Collider>())
        {
            colliders.Remove(collider);
            if (!collider.isTrigger) solidColliders.Remove(collider);

            collider.enabled = !collider.isTrigger;
        }
    }
    public void ToggleColliders(bool solidOnly)
    {
        toggledTimer = timerThreshold;

        if ((solidOnly && solidToggled) || allToggled) return;

        solidToggled = true;
        if (!solidOnly) allToggled = true;

        foreach (Collider col in solidOnly ? solidColliders : colliders)
            if (col) col.enabled = true;
    }
    public void DisableColliders()
    {
        foreach (Collider col in colliders)
        {
            if (col)
                col.enabled = false;
            else Debug.LogError("Probably have a collider in the cockpit with the wrong layer");
        }

        toggledTimer = 0f;
        solidToggled = allToggled = false;
    }
    void Update()
    {
        if (data.relativeAltitude.Get < 15f)
        {
            ToggleColliders(true);
        }

        if (solidToggled && toggledTimer < Time.deltaTime) DisableColliders();
        toggledTimer -= Time.deltaTime;
    }
}
