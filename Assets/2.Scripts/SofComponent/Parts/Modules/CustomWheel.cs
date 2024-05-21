using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomWheel : SofModule
{
    public float radius = 0.5f;
    public bool canSteer;
    public float steerAngle;

    public float suspensionOffset = 0f;
    public float springStrength;
    public float springDamper;

    public float brakeTorque;
    public float tireGripFactor = 1;

    private Transform suspensions;
    private Transform mainGear;
    private float maxDistanceFromGround;
    private Vector3 rayOriginLocal;
    private Vector3 defaultSuspensionsPos;
    private LayerMask mask;

    public bool grounded { get; private set; }
    public float suspensionPos { get; private set; }
    public float velocity { get; private set; }

    const float massConstant = 84f;
    public override float EmptyMass()
    {
        return Mathv.SmoothStart(radius * 2f, 2) * massConstant;
    }
    public override void Initialize(SofComplex _complex)
    {
        material = aircraft.materials.wheelMat;

        base.Initialize(_complex);

        mask = LayerMask.GetMask("Terrain");

        suspensions = transform.parent;
        mainGear = transform.parent.parent;

        defaultSuspensionsPos = suspensions.localPosition;
        maxDistanceFromGround = radius * 4f;
        Vector3 rayOrigin = transform.position + suspensions.up * (maxDistanceFromGround - radius);
        rayOriginLocal = mainGear.InverseTransformPoint(rayOrigin);
    }
    private void Update()
    {
        suspensions.localPosition = defaultSuspensionsPos + Vector3.up * (suspensionPos - suspensionOffset);
    }

    void FixedUpdate()
    {
        if (rb.transform != transform.root) return;

        Vector3 origin = mainGear.TransformPoint(rayOriginLocal + Vector3.down * suspensionOffset);
        grounded = Physics.Raycast(origin, -suspensions.up, out RaycastHit hit, maxDistanceFromGround, mask);
        hit.distance += PerlinGroundOffset();

        Steer();
        if (grounded)
        {
            ApplySuspensionsForce(hit);
            //ApplySidewaysForce();
        }
        else
        {
            MoveSuspensionToDefaultPosition();
        }
    }
    const float perlinScale = 0.23f;
    const float maxGroundHole = 0.15f;
    private float PerlinGroundOffset()
    {
        Vector2 pos = new Vector2(transform.position.x, transform.position.z) * perlinScale;
        float perlinNoiseOffset = Mathf.PerlinNoise(pos.x, pos.y);
        return perlinNoiseOffset * maxGroundHole;
    }

    void ApplySuspensionsForce(RaycastHit hit)
    {
        float newDistance = maxDistanceFromGround - hit.distance;
        velocity = (newDistance - suspensionPos) / Time.fixedDeltaTime;
        suspensionPos = newDistance;

        float offset = suspensionPos;
        float force = (offset * springStrength) + (velocity * springDamper);
        rb.AddForceAtPosition(hit.normal * force, hit.point);
    }
    void MoveSuspensionToDefaultPosition()
    {
        suspensionPos = Mathf.MoveTowards(suspensionPos, 0f, Time.fixedDeltaTime * 0.1f);
    }
    private float steer = 0f;
    void Steer()
    {
        if (!canSteer) return;

        steer = aircraft ? aircraft.inputs.current.yaw * steerAngle : 0f;
        suspensions.transform.localRotation = Quaternion.Euler(0f,steer, 0f);
    }
    void ApplySidewaysForce()
    {
        Vector3 steeringDir = transform.right;
        Vector3 tireWorldVel = rb.GetPointVelocity(transform.position);
        float steeringVel = Vector3.Dot(steeringDir, tireWorldVel);
        float desiredVelChange = -steeringVel * 0.02f;
        float desiredAccel = desiredVelChange / Time.fixedDeltaTime;
        rb.AddForceAtPosition(steeringDir * rb.mass * desiredAccel, transform.position);
    }
    void ApplyBrakeForce()
    {
        if (brakeTorque > 0)
        {
            Vector3 tireWorldVel = rb.GetPointVelocity(transform.position);
            Vector3 forwardDir = transform.forward;
            Vector3 brakeDir = -Vector3.Project(tireWorldVel, forwardDir.normalized).normalized;
            rb.AddForceAtPosition(brakeDir * brakeTorque, transform.position);
        }
    }

    const int segments = 32;
    Color color = Color.red;
    private void OnDrawGizmos()
    {
        Gizmos.color = color;

        Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one);

        Vector3 startPoint = Vector3.forward * radius;
        startPoint = rotationMatrix.MultiplyPoint3x4(startPoint) + transform.position;

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * 2.0f * Mathf.PI / segments;

            Vector3 localEndPoint = new Vector3(0.0f, Mathf.Sin(angle), Mathf.Cos(angle)) * radius;

            Vector3 endPoint = rotationMatrix.MultiplyPoint3x4(localEndPoint) + transform.position;

            Gizmos.DrawLine(startPoint, endPoint);
            startPoint = endPoint;
        }
    }
}