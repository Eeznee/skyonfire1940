using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomWheel : SofModule, IMassComponent
{
    public enum BrakeSystem
    {
        None,
        Standard,
        Differential
    }

    public float radius = 0.5f;
    public BrakeSystem brakes;
    public float maxBrakeTorque = 3000f;

    public float frictionMultiplier = 1f;

    private float radiusInvert;
    private float inertia;
    private float inertiaInvert;

    private LayerMask mask;
    private Suspension suspension;
    private FrictionPreset currentFriction;
    private ParticleSystem frictionFx;


    public bool grounded { get; private set; }
    public float suspensionPos { get; private set; }

    public float load { get; private set; }
    public Vector3 forward { get; private set; }
    public Vector3 right { get; private set; }
    public Vector3 up { get; private set; }
    public Vector3 hitSideDir { get; private set; }
    public Vector3 pointVelocity { get; private set; }
    public float forwardSpeed { get; private set; }
    public float sideSpeed { get; private set; }
    public float angularVelocity { get; private set; }

    public bool TailWheel()
    {
        return localPos.z < -2f;
    }

    public float BrakesInput()
    {
        if (!aircraft) return 0f;

        bool parkingBrakes = data.gsp.Get < 2f && ((int)aircraft.engines.state <= 1 || aircraft.engines.throttle < 0.05f);
        if (parkingBrakes) return 1f;

        float value = aircraft.inputs.brake;

        if (brakes == BrakeSystem.Differential)
        {
            float differentialBrakes = -Mathf.Sign(localPos.x) * aircraft.inputs.target.yaw;
            value = Mathf.Max(value, differentialBrakes);
        }

        return value;
    }

    public float Area => radius * radius * Mathf.PI;

    const float kgPerSqm = 107f;
    public float EmptyMass => Area * kgPerSqm;
    public float LoadedMass => EmptyMass;

    public override float MaxHp => ModulesHPData.wheelHpPerSq * Area;
    public override ModuleArmorValues Armor => ModulesHPData.WheelArmor;

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        radiusInvert = 1f / radius;
        inertia = 0.5f * LoadedMass * radius * radius;
        inertiaInvert = 1f / inertia;

        mask = LayerMask.GetMask("Terrain");
        suspension = GetComponentInParent<Suspension>();
        if (!suspension) Debug.LogError(name + " has no Suspension parent");

        currentFriction = GameManager.gm.mapmap.frictionPreset;

        if (brakes != BrakeSystem.None)
        {
            frictionFx = Instantiate(currentFriction.frictionEffect, transform.position, Quaternion.identity, transform).GetComponent<ParticleSystem>();
            if (frictionFx == null) Debug.LogError("The friction effect is not assigned or it does not contain a particle system", currentFriction);
        }

    }
    private void Update()
    {
        if (!aircraft) return;

        tr.Rotate(Vector3.right, angularVelocity * Time.deltaTime * Mathf.Rad2Deg);

        //Effects
        if (!frictionFx) return;

        bool playFrictionFX = BrakesInput() > 0.1f && grounded && complex.lod.LOD() <= 1;
        if (playFrictionFX != frictionFx.isPlaying)
        {
            if (playFrictionFX) frictionFx.Play();
            else frictionFx.Stop();
        }
    }
    void FixedUpdate()
    {
        if (!aircraft) return;
        if (rb.transform != transform.root) return;

        Vector3 rayOrigin = tr.position + Vector3.up * 5f;
        grounded = Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 10f, mask);

        PerlinGroundOffset(ref hit);
        if (suspension.LowestWheelPos.y - radius > hit.point.y) grounded = false;

        if (grounded)
        {
            suspension.ActOnSuspension(hit);
            ComputeReferences(hit.normal);

            ForwardFriction(hit);
            SideFriction(hit);

            //AntiCreep(hit);
        }
        else
        {
            suspension.RestSuspension();

            angularVelocity = Mathf.MoveTowards(angularVelocity, 0f, rollingResistanceTorque * inertiaInvert * Time.fixedDeltaTime);
        }

    }
    const float perlinScale = 0.23f;
    private void PerlinGroundOffset(ref RaycastHit hit)
    {
        Vector2 pos = new Vector2(transform.position.x, transform.position.z) * perlinScale;
        float perlinNoiseOffset = Mathf.PerlinNoise(pos.x, pos.y) * 0.4f * radius - 0.01f;
        perlinNoiseOffset *= Mathf.InverseLerp(50f, 0f, data.gsp.Get);

        hit.distance += perlinNoiseOffset;
        hit.point += Vector3.down * perlinNoiseOffset;
    }
    void ComputeReferences(Vector3 groundNormal)
    {
        right = tr.right;
        forward = Vector3.Cross(tr.right, groundNormal).normalized;
        up = Vector3.Cross(forward, tr.right).normalized;

        hitSideDir = Vector3.Cross(forward, groundNormal).normalized;

        load = Mathf.Max(0f, suspension.forceApplied);

        pointVelocity = rb.GetPointVelocity(tr.position);
        forwardSpeed = Vector3.Dot(pointVelocity, forward);
        sideSpeed = Vector3.Dot(pointVelocity, hitSideDir);
    }


    private bool wheelIsBlocked;

    const float rollingResistanceTorque = 10f;

    private void ForwardFriction(RaycastHit hit)
    {
        float forwardLoadFactor = load * 1.35f;

        float overFrictionClamp = Mathf.Abs(load / Physics.gravity.y * data.gsp.Get / Time.fixedDeltaTime);
        float maxPossibleFriction = currentFriction.BCDE.z * forwardLoadFactor;
        float maxForce = Mathf.Min(overFrictionClamp, maxPossibleFriction);

        //Friction Force
        float brakeTorque = maxBrakeTorque * BrakesInput();
        float combinedBrakeForce = -(brakeTorque + rollingResistanceTorque) * radiusInvert * Mathv.SignNoZero(forwardSpeed);
        float forwardFrictionForce = Mathf.Clamp(combinedBrakeForce, -maxForce, maxForce);

        rb.AddForceAtPosition(forward * forwardFrictionForce, hit.point);

        //Angular Velocity
        wheelIsBlocked = false;

        float combinedWheelForce = combinedBrakeForce;

        float absWheelForceClamp = Mathf.Abs(angularVelocity) * inertia * radiusInvert / Time.fixedDeltaTime;
        float wheelForceClampOverflow = Mathf.Max(0f, Mathf.Abs(combinedWheelForce) - absWheelForceClamp);
        combinedWheelForce = Mathf.Clamp(combinedWheelForce, -absWheelForceClamp, absWheelForceClamp);

        angularVelocity += combinedWheelForce * radius * inertiaInvert * Time.fixedDeltaTime;

        float noSlipAngularVelocity = forwardSpeed * radiusInvert;
        float angularVelocityError = angularVelocity - noSlipAngularVelocity;
        float angularVelocityCorrectionForce = -angularVelocityError * inertia * radiusInvert / Time.fixedDeltaTime;
        angularVelocityCorrectionForce = Mathf.Clamp(angularVelocityCorrectionForce, -maxForce, maxForce);

        wheelIsBlocked = brakeTorque > 0f && wheelForceClampOverflow > Mathf.Abs(angularVelocityCorrectionForce);
        if (wheelIsBlocked)
            angularVelocity = 0f;
        else
            angularVelocity += angularVelocityCorrectionForce * radius * inertiaInvert * Time.fixedDeltaTime;

        Vector3 squatTorque = forwardFrictionForce * radius * right;
        rb.AddTorque(squatTorque);
    }
    private void SideFriction(RaycastHit hit)
    {
        rb.AddForceAtPosition(hitSideDir * currentFriction.SideFriction(hit, this), hit.point);
    }

    private bool lowSpeedReferenceIsSet;
    private Vector3 lowSpeedReferencePosition;
    private void AntiCreep(RaycastHit hit)
    {
        if (pointVelocity.sqrMagnitude < 0.12f * 0.12f)
        {
            Vector3 currentPosition = transform.position - up * radius;

            if (!lowSpeedReferenceIsSet)
            {
                lowSpeedReferenceIsSet = true;
                lowSpeedReferencePosition = currentPosition;
            }
            else
            {


                Vector3 referenceError = (lowSpeedReferencePosition - currentPosition) / Time.fixedDeltaTime;
                Vector3 correctiveForce = referenceError * load / -Physics.gravity.y;
                Debug.Log(name + correctiveForce.magnitude);
                if (wheelIsBlocked && Mathf.Abs(angularVelocity) < 0.5f)
                    rb.AddForceAtPosition(Vector3.Project(correctiveForce, forward), hit.point);

                rb.AddForceAtPosition(Vector3.Project(correctiveForce, hitSideDir), hit.point);
            }
        }
        else
            lowSpeedReferenceIsSet = false;
    }
}