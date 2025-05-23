﻿using UnityEngine;


public class Lever : MonoBehaviour
{
    public Transform xrGrip;
    public enum LeverInput
    {
        Throttle,
        Mixture,
        PropSpeed,
        Flaps,
        Gear,
        Brake
    }
    public LeverInput inputType;
    public float maxAngleOffset = 20f;
    public float speed = 5f;

    Quaternion originalRot;
    float input;
    float trueInput;

    SofAircraft controller;

    private void Start()
    {
        input = 0f;
        trueInput = 0f;
        controller = GetComponentInParent<SofAircraft>();
        originalRot = transform.localRotation;
    }

    private void Update()
    {
        switch (inputType)
        {
            case LeverInput.Throttle:
                input = controller.engines.Throttle;
                break;
            case LeverInput.Mixture:
                input = controller.engines.Throttle;
                break;
            case LeverInput.Flaps:
                input = controller.hydraulics.flaps.stateInput;
                break;
            case LeverInput.Gear:
                input = controller.hydraulics.gear.stateInput;
                break;
            case LeverInput.Brake:
                input = controller.controls.brake;
                break;
        }
        trueInput = Mathf.MoveTowards(trueInput, input, speed * Time.deltaTime);
        transform.localRotation = originalRot;
        transform.Rotate(Vector3.right * maxAngleOffset * trueInput);
    }
}

