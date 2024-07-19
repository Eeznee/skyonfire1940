using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomWheel : SofModule
{
    public enum BrakeSystem
    {
        None,
        Standard,
        Differential
    }


    public float radius = 0.5f;
    public TireFriction tire;
    public BrakeSystem brakes;
    public float maxBrakeTorque = 3000f;

    private float radiusInvert;
    private float inertia;
    private float inertiaInvert;

    private LayerMask mask;
    public Suspension suspension;
    private ParticleSystem frictionFx;


    public bool grounded { get; private set; }
    public float suspensionPos { get; private set; }

    public Vector3 forward { get; private set; }
    public Vector3 right { get; private set; }
    public Vector3 up { get; private set; }
    public Vector3 hitSideDir { get; private set; }
    public Vector3 pointVelocity { get; private set; }
    public float forwardSpeed { get; private set; }
    public float sideSpeed { get; private set; }
    public float angularVelocity { get; private set; }

    public float BrakesInput()
    {
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

    const float massConstant = 84f;
    public override bool NoCustomMass => true;
    public override float EmptyMass => Mathv.SmoothStart(radius * 2f, 2) * massConstant;
    public override void Initialize(SofComplex _complex)
    {
        material = aircraft.materials.wheelMat;

        base.Initialize(_complex);

        radiusInvert = 1f / radius;
        inertia = 0.5f * Mass * radius * radius;
        inertiaInvert = 1f / inertia;

        mask = LayerMask.GetMask("Terrain");
        suspension = GetComponentInParent<Suspension>();
        if (!suspension) Debug.LogError(name + " has no Suspension parent");

        if (brakes != BrakeSystem.None) frictionFx = Instantiate(tire.frictionEffect, transform.position, Quaternion.identity, transform);

    }
    private void Update()
    {
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
        up = Vector3.Cross(forward, tr.right);

        hitSideDir = Vector3.Cross(forward, groundNormal).normalized;


        load = Mathf.Max(0f,suspension.forceApplied);

        pointVelocity = rb.GetPointVelocity(tr.position);
        forwardSpeed = Vector3.Dot(pointVelocity, forward);
        sideSpeed = Vector3.Dot(pointVelocity, hitSideDir);
    }

    private float load;
    private bool wheelIsBlocked;



    public NWH.Common.Vehicles.FrictionPreset activeFrictionPreset;
    const float sideStiffness = 1f;
    const float sideGrip = 1f;

    const float rollingResistanceTorque = 10f;

    private void ForwardFriction(RaycastHit hit)
    {
        float forwardLoadFactor = load * 1.35f;

        float overFrictionClamp = Mathf.Abs(load/-Physics.gravity.y * data.gsp.Get / Time.fixedDeltaTime);
        float maxPossibleFriction = activeFrictionPreset.BCDE.z * forwardLoadFactor;
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
            angularVelocity =  0f;
        else
            angularVelocity += angularVelocityCorrectionForce * radius * inertiaInvert * Time.fixedDeltaTime;


        float squatMagnitude = forwardFrictionForce * radius;
        Vector3 squatTorque = squatMagnitude * right;

        // Use base inertia here as the powertrain component orientation is not known and it might not contribute to the 
        // torque around the X-axis.
        float chassisTorqueMag = 0f;// ((wheel.prevAngularVelocity - wheel.angularVelocity) * wheel.baseInertia) / _dt;
        Vector3 chassisTorque = chassisTorqueMag * right;
        rb.AddTorque(squatTorque + chassisTorque);
    }
    private void ApplySquatAndChassisTorque()
    {

    }
    private void SideFriction(RaycastHit hit)
    {
        float sideLoadFactor = load * 1.9f;

        float absForwardSpeed = Mathf.Abs(forwardSpeed);

        float forwardSpeedClamp = 1.5f * (Time.fixedDeltaTime / 0.005f);
        forwardSpeedClamp = Mathf.Clamp(forwardSpeedClamp, 1.5f, 10f);
        float clampedAbsForwardSpeed = Mathf.Max(absForwardSpeed,forwardSpeedClamp);


        // Calculate slip based on the corrected angular velocity
        float slipLoadModifier = 1f;// - Mathf.Clamp01(load / loadRating) * 0.4f;
        float sideSlip = (Mathf.Atan2(sideSpeed, clampedAbsForwardSpeed) * Mathf.Rad2Deg) * 0.01111f;
        sideSlip *= sideStiffness * slipLoadModifier;


        float camberFrictionCoeff = Vector3.Dot(up, hit.normal);
        float peakSideFrictionForce = activeFrictionPreset.BCDE.z * sideLoadFactor * sideGrip;
        float sideFrictionForce = -Mathf.Sign(sideSlip) * activeFrictionPreset.Curve.Evaluate(Mathf.Abs(sideSlip)) * sideLoadFactor * sideGrip * camberFrictionCoeff;


        sideFrictionForce = Mathf.Clamp(sideFrictionForce, -peakSideFrictionForce, peakSideFrictionForce);
        rb.AddForceAtPosition(hitSideDir * sideFrictionForce, hit.point);
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