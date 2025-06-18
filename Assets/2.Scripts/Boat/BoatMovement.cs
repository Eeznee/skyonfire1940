using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[RequireComponent(typeof(SofShip))]
public class BoatMovement : MonoBehaviour
{
    public float maxSpeed = 15f;
    public float maxAngularSpeed = 10f;

    public float speedAcceleration = 1f;
    public float angularAcceleration = 1f;

    [Range(-1f, 1f)] public float turnInput;
    [Range(-0.25f, 1f)] public float throttleInput;



    private SofShip sofShip;
    [NonSerialized] public float currentSpeed;
    [NonSerialized] public float currentAngularSpeed;

    void Start()
    {
        currentSpeed = maxSpeed;
        currentAngularSpeed = 0f;

        throttleInput = 1f;
        turnInput = 0f;

        sofShip = GetComponent<SofShip>();
    }
    void Update()
    {
        Vector3 pos = transform.position;
        Vector3 forward = transform.forward;
        forward.y = 0f;
        pos += currentSpeed * Time.deltaTime * forward;
        pos.y = sofShip.sinkingDepth;
        transform.position = pos;

        Vector3 euler = transform.rotation.eulerAngles;
        euler.y += currentAngularSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(euler);


        float targetSpeed = throttleInput * maxSpeed;
        float targetAngularSpeed = turnInput * maxAngularSpeed;
        targetAngularSpeed *= currentSpeed / maxSpeed;

        if (sofShip.Destroyed) targetSpeed = targetAngularSpeed = 0f;

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedAcceleration * Time.deltaTime);
        currentAngularSpeed = Mathf.MoveTowards(currentAngularSpeed, targetAngularSpeed, angularAcceleration * Time.deltaTime);
    }
    public float TurnRadius()
    {
        float fullCircleTime = 360f / maxAngularSpeed;
        float fullCirclePerimeter = fullCircleTime * maxSpeed;

        return fullCirclePerimeter / (Mathf.PI * 2f);
    }
}
