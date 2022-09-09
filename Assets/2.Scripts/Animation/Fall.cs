using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fall : MonoBehaviour
{
    public float verticalSpeed = 1f;
    public float verticalAcceleration = 1f;
    public float rotateSpeed = 5f;
    public float rotateAcceleration = 5f;

    float currentVspeed;
    float currentRspeed;
    float angle;
    Vector3 rotateAround;
    Quaternion defaultRot;

    void Start()
    {
        currentVspeed = currentRspeed = 0f;
        rotateAround = Quaternion.Euler(0f, Random.value * 360f, 0f) * Vector3.forward;
        defaultRot = transform.rotation;
    }

    void Update()
    {
        currentVspeed = Mathf.MoveTowards(currentVspeed, verticalSpeed, verticalAcceleration * Time.deltaTime);
        transform.position += Vector3.down * currentVspeed * Time.deltaTime;

        currentRspeed = Mathf.MoveTowards(currentRspeed, rotateSpeed, rotateAcceleration * Time.deltaTime);
        angle = Mathf.Min(angle + currentRspeed * Time.deltaTime, 90f);
        transform.rotation = defaultRot * Quaternion.AngleAxis(angle, rotateAround);
    }
}
