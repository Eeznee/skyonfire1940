using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Sof Components/Undercarriage/Wheel")]
public class Wheel : SofModule, IMassComponent
{
    public enum AutoValuesType
    {
        MainWheel,
        TailWheel,
        CustomWheel
    }
    public enum BrakeSystem
    {
        None,
        Standard,
        Differential
    }

    public AutoValuesType autoValuesType;

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

    public float BrakesInput()
    {
        if (!aircraft) return 0f;
        if(brakes == BrakeSystem.None) return 0f;

        bool parkingBrakes = data.gsp.Get < 2f && (!aircraft.engines.AtLeastOneEngineOn || aircraft.engines.Throttle < 0.05f);
        if (parkingBrakes) return 1f;

        float value = aircraft.controls.brake;

        if (brakes == BrakeSystem.Differential)
        {
            float differentialBrakes = -Mathf.Sign(localPos.x) * aircraft.controls.target.yaw;
            value = Mathf.Max(value, differentialBrakes);
        }

        return value;
    }

    public float AngularVelocityError => angularVelocity - forwardSpeed * radiusInvert;
    public float Area => radius * radius * Mathf.PI;

    const float kgPerSqm = 107f;
    public float EmptyMass => Area * kgPerSqm;
    public float LoadedMass => EmptyMass;
    public float RealMass => EmptyMass;

    public override float MaxHp => ModulesHPData.wheelHpPerSq * Area;
    public override ModuleArmorValues Armor => ModulesHPData.WheelArmor;

    private Vector3 rotateAxis;
    private bool damaged;

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        radiusInvert = 1f / radius;
        inertia = 0.5f * RealMass * radius * radius;
        inertiaInvert = 1f / inertia;

        mask = LayerMask.GetMask("Terrain");
        suspension = GetComponentInParent<Suspension>();
        if (!suspension) Debug.LogError(name + " has no Suspension parent");

        currentFriction = GameManager.gm.mapmap.frictionPreset;

        rotateAxis = transform.parent.InverseTransformDirection(transform.right);
        damaged = false;

        OnDirectDamage += OnDamageTaken;

        SetAutomatedValues();
    }
    const float brakesToWeightRatio = 1.3f;
    private void SetAutomatedValues()
    {
        if (autoValuesType == AutoValuesType.CustomWheel) return;

        if (autoValuesType == AutoValuesType.TailWheel) brakes = BrakeSystem.None;

        maxBrakeTorque = aircraft.targetEmptyMass * radius * brakesToWeightRatio;
    }
    private void Update()
    {
        if (!aircraft) return;
        if (angularVelocity == 0f) return;

        Vector3 worldRotateAxis = transform.parent.TransformDirection(rotateAxis);
        tr.Rotate(worldRotateAxis, angularVelocity * Time.deltaTime * Mathf.Rad2Deg,Space.World);
    }
    void FixedUpdate()
    {
        if (!aircraft) return;
        if (rb.transform != transform.root) return;

        if (data.relativeAltitude.Get > 20f)
        {
            grounded = false;
            AirborneFixedUpdate();
        }
        else
        {
            Vector3 rayOrigin = tr.position + Vector3.up * 5f;
            grounded = Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 10f, mask);

            PerlinGroundOffset(ref hit);
            if (suspension.LowestWheelPos.y - radius > hit.point.y) grounded = false;

            if (grounded)
            {
                GroundedFixedUpdate(hit);
            }
        }
    }

    private void GroundedFixedUpdate(RaycastHit hit)
    {
        suspension.ActOnSuspension(hit);
        ComputeReferences(hit.normal);

        ForwardFriction(hit);
        SideFriction(hit);
    }

    private void AirborneFixedUpdate()
    {
        suspension.RestSuspension();

        angularVelocity = Mathf.MoveTowards(angularVelocity, 0f, rollingResistanceTorque * inertiaInvert * Time.fixedDeltaTime);
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
    const float groundFrictionLoadFactor = 0.02f;

    private void ForwardFriction(RaycastHit hit)
    {
        float forwardLoadFactor = load * 1.35f;

        float overFrictionClamp = Mathf.Abs(load / Physics.gravity.y * data.gsp.Get / Time.fixedDeltaTime);
        float maxPossibleFriction = currentFriction.BCDE.z * forwardLoadFactor;
        float maxForce = Mathf.Min(overFrictionClamp, maxPossibleFriction);

        //Friction Force
        float brakeTorque = maxBrakeTorque * BrakesInput() * structureDamage;
        float brakeForce = -(brakeTorque + rollingResistanceTorque) * radiusInvert * Mathv.SignNoZero(forwardSpeed);
        float frictionForce = -load * groundFrictionLoadFactor * Mathv.SignNoZero(forwardSpeed);
        float forwardFrictionForce = Mathf.Clamp(frictionForce + brakeForce, -maxForce, maxForce) ; 
        rb.AddForceAtPosition(forward * forwardFrictionForce, hit.point);

        //Angular Velocity
        wheelIsBlocked = false;

        float combinedWheelForce = brakeForce;

        float absWheelForceClamp = Mathf.Abs(angularVelocity) * inertia * radiusInvert / Time.fixedDeltaTime;
        float wheelForceClampOverflow = Mathf.Max(0f, Mathf.Abs(combinedWheelForce) - absWheelForceClamp);
        combinedWheelForce = Mathf.Clamp(combinedWheelForce, -absWheelForceClamp, absWheelForceClamp);

        angularVelocity += combinedWheelForce * radius * inertiaInvert * structureDamage * structureDamage * Time.fixedDeltaTime;

        float angularVelocityCorrectionForce = -AngularVelocityError * inertia * radiusInvert / Time.fixedDeltaTime;
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
        rb.AddForceAtPosition(hitSideDir * currentFriction.SideFriction(hit, this) * structureDamage, hit.point);
    }
    public void OnDamageTaken(float damageTaken)
    {
        if(!damaged && structureDamage < 0.5f)
        {
            damaged = true;
            transform.localScale = new Vector3(1f, 0.9f, 1f);
            rotateAxis += Vector3.up * Random.Range(0.06f, 0.12f) * Mathf.Abs(Random.Range(-1f, 1f));
        }
    }
    public override void Rip()
    {
        base.Rip();

        DetachAndCreateDebris();

        transform.localScale = Vector3.one;

        MeshCollider meshColl = transform.CreateChild("Detached Collider").gameObject.AddComponent<MeshCollider>();
        meshColl.sharedMesh = StaticReferences.Instance.wheelCollisionMesh;
        meshColl.sharedMaterial = StaticReferences.Instance.wheelPhysicMaterial;
        meshColl.convex = true;

        Bounds bounds = GetComponent<MeshFilter>().sharedMesh.bounds;
        meshColl.transform.localScale = new Vector3(bounds.size.x,bounds.size.y,bounds.size.z) * 1.05f;
        meshColl.gameObject.layer = gameObject.layer;

        rb.maxAngularVelocity = Mathf.Abs(angularVelocity) * 2f;
        rb.angularVelocity = transform.right * angularVelocity;

        Destroy(this);
    }
}