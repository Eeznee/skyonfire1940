using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class Suspension : SofPart
{
    public Vector3 axis = Vector3.up;
    public float springStrength = 300000f;
    public float springDamper = 30000f;

    public bool canSteer = false;
    public float maxSteerAngle = 20f;

    private Transform parent;
    private Vector3 lowestPos;

    private CustomWheel wheel;
    private Vector3 wheelRestPos;
    public float distance;

    public float CurrentDistance => distance;
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
    }
    private void FixedUpdate()
    {
        if (canSteer)
        {
            Vector3 pointDir = rb.GetPointVelocity(wheel.tr.position);
            if (pointDir.magnitude < 2f) return;
            pointDir = Vector3.ProjectOnPlane(pointDir, transform.up);
            transform.localRotation = Quaternion.identity;
            float steerAngle = Vector3.SignedAngle(transform.forward, pointDir, transform.up);
            transform.localRotation = Quaternion.AngleAxis(steerAngle, axis);
        }
        if (false && canSteer)
        {
            float steerAngle = aircraft ? aircraft.inputs.current.yaw * maxSteerAngle : 0f;
            transform.localRotation = Quaternion.AngleAxis(steerAngle, axis);
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
