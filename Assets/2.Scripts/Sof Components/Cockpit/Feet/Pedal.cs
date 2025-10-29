using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Pedal : SofComponent
{
    public bool offset;
    public float maxOffset = 0.1f;
    public bool rotation;
    public float maxRotation = 10f;
    public Vector3 axis = Vector3.right;

    Vector3 originalPos;
    Quaternion originalRot;

    private float currentYawInput;

    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);
        originalPos = transform.localPosition;
        originalRot = transform.localRotation;

        aircraft.OnUpdateLOD0 += UpdatePedals;
    }

    private void UpdatePedals()
    {
        if (currentYawInput == aircraft.controls.current.yaw) return;
        currentYawInput = aircraft.controls.current.yaw;

        if (offset)
        {
            transform.localPosition = originalPos;
            transform.localPosition += Vector3.forward * maxOffset * aircraft.controls.current.yaw;
        }
        if (rotation)
        {
            transform.localRotation = originalRot;
            transform.Rotate(axis, currentYawInput * maxRotation);
        }
    }
}

