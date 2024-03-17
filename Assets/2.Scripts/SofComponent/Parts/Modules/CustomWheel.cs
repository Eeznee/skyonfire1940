using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class CustomWheel : MonoBehaviour
{
    public float mass = 20, radius = 0.5f, steerAngle, brakeTorque;
    public LayerMask collisionLayerMask;
    [Header("Friction")]
    public float tireGripFactor = 1;

    [Header("Suspension")]
    public float suspensionRestDist = .5f;
    public float springStrength, springDamper;

    const float rayOriginOffset = 1f;
    private Vector3 rayOriginLocal;
    private Vector3 defaultSuspensionsPos;
    private Transform suspensions;
    private Transform mainGear;

    private bool grounded;
    private float suspensionDistance = 0.5f;
    private float velocity;
    Rigidbody rb;
    void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();

        suspensions = transform.parent;
        mainGear = transform.parent.parent;

        defaultSuspensionsPos = suspensions.localPosition;
        Vector3 rayOrigin = transform.position + suspensions.up * rayOriginOffset;
        rayOriginLocal = mainGear.InverseTransformPoint(rayOrigin);
    }
    void FixedUpdate()
    {
        if (rb.transform != transform.root) return;

        grounded = Physics.Raycast(mainGear.TransformPoint(rayOriginLocal), Vector3.down, out RaycastHit hit, radius + rayOriginOffset, collisionLayerMask);

        if (grounded)
        {
            float newDistance = hit.distance - rayOriginOffset;
            velocity = (newDistance - suspensionDistance) / Time.fixedDeltaTime;
            suspensionDistance = newDistance;

            float offset = suspensionRestDist - suspensionDistance;
            float force = (offset * springStrength) - (velocity * springDamper);
            rb.AddForceAtPosition(hit.normal * force, hit.point);
        }
    }
    private void Update()
    {
        suspensions.localPosition = defaultSuspensionsPos + Vector3.down * (suspensionDistance - suspensionRestDist);
    }
    void ApplySidewaysForce()
    {
        Vector3 steeringDir = transform.right;
        Vector3 tireWorldVel = rb.GetPointVelocity(transform.position);
        float steeringVel = Vector3.Dot(steeringDir, tireWorldVel);
        float desiredVelChange = -steeringVel * tireGripFactor;
        float desiredAccel = desiredVelChange / Time.fixedDeltaTime;
        rb.AddForceAtPosition(steeringDir * mass * desiredAccel, transform.position);
    }
    public void ApplyBrakeForce()
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