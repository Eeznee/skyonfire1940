using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BrakeMode
{
    None,
    Simple,
    Split
}

public class Wheel : SofModule
{
    private float brakeInput;
    private float steeringInput;

    public float diameter = 0.6f;
    public BrakeMode brakeMode = BrakeMode.Simple;
    public float brakeTorque = 600f;

    public bool steering = false;
    public float maxSteerAngle = 30f;
    public ParticleSystem brakeEffect;

    private float radius;

    public WheelCollider wheel;
    private Quaternion defaultLocalRot;
    private Vector3 rootPos;
    private MeshFilter meshFilter;

    private bool isGrounded;
    private float rpm;
    private bool wheelDisabled = false;

    public override bool Detachable()
    {
        return true;
    }
    const float suspensionLength = 0.5f;
    const float stablePosition = 0.4f;
    const float springMultiplier = 100f;
    const float dampMultiplier = 2f;

    private void SetWheelCollider()
    {
        WheelFrictionCurve frictionForward = new WheelFrictionCurve();
        frictionForward.extremumSlip = 0.4f;
        frictionForward.extremumValue = 1f;
        frictionForward.asymptoteSlip = 0.8f;
        frictionForward.asymptoteValue = 0.5f;
        frictionForward.stiffness = 1f;

        WheelFrictionCurve frictionSide = new WheelFrictionCurve();
        frictionSide.extremumSlip = 0.2f;
        frictionSide.extremumValue = 1f;
        frictionSide.asymptoteSlip = 0.5f;
        frictionSide.asymptoteValue = 0.75f;
        frictionSide.stiffness = 1f;

        wheel.forwardFriction = frictionForward;
        wheel.sidewaysFriction = frictionSide;
        wheel.transform.localPosition = transform.localPosition;
        wheel.transform.localPosition += Vector3.up * suspensionLength * (1f - stablePosition);
        wheel.suspensionDistance = suspensionLength;
        
        JointSpring spring = new JointSpring();
        float springStrength = aircraft.targetEmptyMass * springMultiplier;
        spring.spring = springStrength;
        spring.damper = dampMultiplier * aircraft.targetEmptyMass;
        spring.targetPosition = stablePosition;
        wheel.suspensionSpring = spring;

        wheel.radius = radius;
        wheel.mass = 5f;// Mass(radius * 2f) * 0.01f;
        wheel.wheelDampingRate = 0.05f;
    }
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        meshFilter = GetComponent<MeshFilter>();
        radius = meshFilter.sharedMesh.bounds.size.y * 0.5f;

        if (radius > 0.3f) SetWheelCollider();

        if (brakeMode != BrakeMode.None) brakeEffect = Instantiate(brakeEffect, transform.position, Quaternion.identity, transform);
        defaultLocalRot = transform.parent.localRotation;
        rootPos = transform.root.InverseTransformPoint(transform.position);
    }
    const float maximumSteerableSpeed = 60f / 3.6f;
    const float steerAngularSpeed = 30f;

    void FixedUpdate()
    {
        if (!wheel || Time.timeScale == 0f) return;

        rpm = wheel.rpm;
        isGrounded = wheel.isGrounded;

        wheel.motorTorque = isGrounded ? 1f : 0f;

        if (!isGrounded) return;

        wheel.GetGroundHit(out WheelHit hit);
        if (hit.force > wheel.suspensionSpring.spring)
            Rip();

        if (!aircraft || brakeMode == BrakeMode.None) return;

        if (steering)
        {
            float speedEff = Mathf.InverseLerp(maximumSteerableSpeed, 0f, data.gsp.Get);

            steeringInput = aircraft.inputs.current.yaw * maxSteerAngle * speedEff;

            wheel.steerAngle = Mathf.MoveTowards(wheel.steerAngle, steeringInput, Time.fixedDeltaTime * steerAngularSpeed);

            Quaternion rotation = defaultLocalRot * Quaternion.Euler(Vector3.up * wheel.steerAngle * -Mathf.Sign(rootPos.z));
            transform.parent.localRotation = rotation;
        }

        bool forcedBrake = data.gsp.Get < 2f && ((int)aircraft.engines.state <= 1 || aircraft.engines.throttle < 0.05f);

        brakeInput = brakeMode == BrakeMode.Split ? -Mathf.Sign(rootPos.x) * aircraft.inputs.target.yaw : 0f;
        brakeInput = Mathf.Max(aircraft.inputs.brake, brakeInput);
        if (forcedBrake) brakeInput = 1f;
        if (wheel.radius == 0f) brakeInput = 0f;

        wheel.brakeTorque = brakeInput * brakeTorque;
        rb.AddTorque(-transform.root.right * wheel.brakeTorque);
    }
    private void Update()
    {
        if (aircraft && aircraft.hydraulics.gear && rootPos.x != 0f)
        {
            bool newWheelDisabled = aircraft.hydraulics.gear.state < 0.8f;
            if (newWheelDisabled != wheelDisabled) { wheelDisabled = newWheelDisabled; wheel.radius = wheelDisabled ? 0f : radius; }
        }
        wheel.GetWorldPose(out Vector3 pos, out Quaternion rot);
        transform.position = pos;
        if (rpm > 1f) transform.Rotate(Vector3.right * rpm * 6f * Time.deltaTime * Mathf.Sign(Vector3.Dot(tr.right, tr.root.right)));

        if (!aircraft || brakeMode == BrakeMode.None) return;

        //Effects
        bool effect = brakeInput > 0.1f && isGrounded && complex.lod.LOD() <= 1;
        if (effect != brakeEffect.isPlaying)
        {
            if (effect) brakeEffect.Play();
            else brakeEffect.Stop();
        }
    }
    public override void Rip()
    {
        if (ripped) return;
        base.Rip();

        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.isTrigger = false;
        meshCollider.convex = true;
        meshCollider.sharedMesh = meshFilter.sharedMesh;
        meshCollider.sharedMaterial = aircraft.materials.aircraftMat;

        Detach();
        rb.angularVelocity = tr.right * wheel.rpm / 30f * Mathf.PI;

        Destroy(wheel.gameObject);
        Destroy(this);

    }
    const float massConstant = 84f;
    public static float Mass(float wheelDiameter)
    {
        return Mathv.SmoothStart(wheelDiameter, 2) * massConstant;
    }
    public JointSpring Suspension()
    {
        JointSpring spring = new JointSpring();
        spring.spring = 0f;
        spring.damper = 0f;
        spring.targetPosition = 0.5f;
        return spring;
    }
}

public class Steering : SofModule
{
    private float steeringInput;

    public bool steering = false;
    public float maxSteerAngle = 30f;
    const float maxSteerSpeed = 60f / 3.6f;

    public WheelCollider wheel;
    Quaternion defaultLocalRot;
    Vector3 rootPos;
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        defaultLocalRot = transform.parent.localRotation;
        rootPos = transform.root.InverseTransformPoint(transform.position);
    }

    void FixedUpdate()
    {
        if (!wheel || Time.timeScale == 0f) return;

        if (steering && aircraft)
        {
            float speedEff = Mathf.InverseLerp(maxSteerSpeed, 0f, data.gsp.Get);
            wheel.steerAngle = aircraft.inputs.current.yaw * maxSteerAngle * -Mathf.Sign(rootPos.z) * speedEff;
            transform.parent.localRotation = defaultLocalRot;
            transform.parent.Rotate(Vector3.up * wheel.steerAngle);
        }
    }
}
