using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class Suspension : SofComponent
{
    public Vector3 axis = Vector3.up;


    public bool preciseValues = false;
    public float springStrength = 300000f;
    public float springDamper = 30000f;
    public float springStrengthFactor = 1f;
    public float springDamperFactor = 1f;


    private Transform parent;
    private Vector3 lowestPos;

    private CustomWheel wheel;
    private Vector3 wheelRestPos;
    private float distance;

    public float Distance => distance;
    public Vector3 LowestWheelPos => parent.TransformPoint(lowestPos) + tr.TransformDirection(wheelRestPos);

    public float forceApplied { get; private set; }

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        axis = axis.normalized;

        parent = tr.parent;
        lowestPos = tr.localPosition;

        wheel = GetComponentInChildren<CustomWheel>();
        if (!wheel) Debug.LogError(name + " has no Wheel child");

        wheelRestPos = tr.InverseTransformPoint(wheel.transform.position);
        if (!preciseValues) SetAutomatedValues();
    }
    private void SetAutomatedValues()
    {
        if (wheel.TailWheel())
        {
            springStrength = aircraft.targetEmptyMass * 20f * springStrengthFactor;
            springDamper = aircraft.targetEmptyMass * 2.5f * springDamperFactor;
        } else
        {
            springStrength = aircraft.targetEmptyMass * 100f * springStrengthFactor;
            springDamper = aircraft.targetEmptyMass * 5f * springDamperFactor;
        }
    }



    public void ActOnSuspension(RaycastHit hit)
    {
        Vector3 desiredWheelPos = hit.point + wheel.radius * hit.normal;

        float newSuspensionDistance = desiredWheelPos.y - LowestWheelPos.y;
        newSuspensionDistance = Mathf.Max(newSuspensionDistance, 0f);

        float velocity = (newSuspensionDistance - distance) / Time.fixedDeltaTime;
        float springForce = springStrength * Mathf.Min(newSuspensionDistance, wheel.radius * 2f);
        float damperForce = (velocity * springDamper);
        float force = springForce + damperForce;
        rb.AddForceAtPosition(hit.normal * force, hit.point);
        forceApplied = force;

        UpdatePosition(newSuspensionDistance);
    }
    public void RestSuspension()
    {
        UpdatePosition(0f);
        forceApplied = 0f;
    }
    private void UpdatePosition(float newDistance)
    {
        distance = Mathf.Max(newDistance, 0f);
        tr.localPosition = lowestPos + axis * distance;
    }
}