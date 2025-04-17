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

    SofAircraft controller;

    private float currentYawInput;

    private void Start()
    {
        controller = GetComponentInParent<SofAircraft>();
        originalPos = transform.localPosition;
        originalRot = transform.localRotation;
    }

    private void Update()
    {
        if (!aircraft || aircraft.lod.LOD() != 0) return;

        if (currentYawInput == controller.controls.current.yaw) return;
        currentYawInput = controller.controls.current.yaw;

        if (offset)
        {
            transform.localPosition = originalPos;
            transform.localPosition += Vector3.forward * maxOffset * controller.controls.current.yaw;
        }
        if (rotation)
        {
            transform.localRotation = originalRot;
            transform.Rotate(axis, currentYawInput * maxRotation);
        }
    }
}

