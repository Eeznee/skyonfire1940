using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SofShip : SofComplex
{
    public float rollSpring = 1f;
    public float rollDamper = 1f;


    private float rollAngle;
    private float rollVelocity;



    protected override void GameInitialization()
    {
        base.GameInitialization();

        rollAngle = 0f;
        rollVelocity = 0f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O)) rollVelocity += 15f;


        float springForce = -rollAngle * rollSpring;
        float damperForce = -rollVelocity * rollDamper;

        float rollForce = springForce + damperForce;

        rollVelocity += rollForce * Time.deltaTime;

        rollAngle += rollVelocity * Time.deltaTime;

        transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
        transform.Rotate(rollAngle * Vector3.forward);
    }
}
