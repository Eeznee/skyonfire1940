using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



[AddComponentMenu("Sof Components/Undercarriage/Suspension")]
public class Suspension : BarFrame
{
    public enum Type
    {
        Spring,
        Solid
    }
    public Type type = Type.Spring;
    public Vector3 deformationOrigin = Vector3.zero;
    public Vector3 axis = Vector3.up;

    public float springStrength = 300000f;
    public float springDamper = 30000f;
    public float springStrengthFactor = 1f;
    public float springDamperFactor = 1f;


    private Transform parent;
    private Vector3 lowestPos;

    private Wheel wheel;
    private Vector3 wheelRestPos;
    private float distance;

    public float Distance => distance;
    public Vector3 LowestWheelPos => parent.TransformPoint(lowestPos) + tr.TransformDirection(wheelRestPos);

    public override bool Detachable => false;

    public float forceApplied { get; private set; }

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        axis = axis.normalized;

        parent = tr.parent;
        lowestPos = tr.localPosition;

        wheel = GetComponentInChildren<Wheel>();
        if (!wheel) Debug.LogError(name + " has no Wheel child");

        wheelRestPos = tr.InverseTransformPoint(wheel.transform.position);
        SetAutomatedValues();
    }
    private void SetAutomatedValues()
    {
        if (wheel.autoValuesType == Wheel.AutoValuesType.CustomWheel) return;

        if (wheel.autoValuesType == Wheel.AutoValuesType.TailWheel)
        {
            springStrength = aircraft.card.standardLoadedMass * 20f * springStrengthFactor;
            springDamper = aircraft.card.standardLoadedMass * 2.5f * springDamperFactor;
        }
        else
        {
            springStrength = aircraft.card.standardLoadedMass * 100f * springStrengthFactor;
            springDamper = aircraft.card.standardLoadedMass * 5f * springDamperFactor;
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
        if (distance == newDistance) return;

        distance = Mathf.Max(newDistance, 0f);

        if (type == Type.Spring)
        {
            tr.localPosition = lowestPos + axis * distance;
        }
        else if (type == Type.Solid)
        {
            float lowestPosDis = Vector3.Project(wheelRestPos, axis).magnitude;
            float lowestPosToOriginDis = Vector3.Project(deformationOrigin - wheelRestPos, axis).magnitude;

            float deformationRequired =  distance / lowestPosToOriginDis;
            float offsetRequired = lowestPosDis * distance;

            tr.localScale = Vector3.one - axis * deformationRequired;
            tr.localPosition = lowestPos + axis * offsetRequired;
        }
    }
}
