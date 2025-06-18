using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightConditions
{
    public  SofModular complex { get; private set; }

    public Vector3 position;// { get; private set; }
    public Quaternion rotation;// { get; private set; }
    public AircraftAxes axes;

    public Vector3 velocity;// { get; private set; }
    public Vector3 angularVelocity;// { get; set; }

    public float airDensity { get; private set; }

    public bool fictionalConditions { get; private set; }

    public Vector3 WorldCenterOfMass => position + rotation * complex.rb.centerOfMass;
    public Vector3 Forward => rotation * Vector3.forward;
    public Vector3 Up => rotation * Vector3.up;
    public Vector3 Right => rotation * Vector3.right;

    public float IAS => Mathf.Sqrt(velocity.sqrMagnitude * airDensity * Aerodynamics.invertSeaLvlDensity);

    private float massInvert;
    private Matrix4x4 tensorInvert;

    public Vector3 PointVelocity(Vector3 worldPoint)
    {
        if (!fictionalConditions) return complex.rb.GetPointVelocity(worldPoint);

        Vector3 localPoint = worldPoint -  WorldCenterOfMass;
        return velocity + Vector3.Cross(angularVelocity, localPoint);
    }
    public Vector3 TransformWorldPos(Vector3 worldPoint)
    {
        Vector3 localPoint = complex.tr.InverseTransformPoint(worldPoint);
        return position + rotation * localPoint;
    }
    public Vector3 TransformWorldDir(Vector3 worldDir)
    {
        Vector3 localDir = complex.tr.InverseTransformDirection(worldDir);
        return rotation * localDir;
    }
    public void CopyValues(FlightConditions other)
    {
        complex = other.complex;
        position = other.position;
        rotation = other.rotation;
        velocity =  other.velocity;
        angularVelocity = other.angularVelocity;
        axes =  other.axes;
        airDensity = other.airDensity;
        fictionalConditions = other.fictionalConditions;

        InitializeMass();
    }
    public void InitializeMass()
    {
        Rigidbody rb = complex.rb;

        massInvert = 1f / complex.rb.mass;

        Matrix4x4 inertiaTensorLocal = Matrix4x4.Rotate(rb.inertiaTensorRotation) * new Matrix4x4(
    new Vector4(rb.inertiaTensor.x, 0, 0, 0),
    new Vector4(0, rb.inertiaTensor.y, 0, 0),
    new Vector4(0, 0, rb.inertiaTensor.z, 0),
    new Vector4(0, 0, 0, 1)
);

        tensorInvert = inertiaTensorLocal.inverse;
    }
    public FlightConditions(SofModular _complex, bool _fictionalConditions)
    {
        complex = _complex;

        position = complex.transform.position;
        rotation = complex.transform.rotation;

        velocity = complex.rb.velocity;
        angularVelocity = complex.rb.angularVelocity;

        if (complex.aircraft) axes = complex.aircraft.controls.current;
        else axes = new AircraftAxes(0f, 0f, 0f);

        airDensity = complex.data.density.Get;

        fictionalConditions = _fictionalConditions;

        InitializeMass();
    }
    public FlightConditions(SofModular _complex, Vector3 _velocity, Vector3 _angularVelocity, AircraftAxes _axes)
    {
        complex = _complex;

        position = _complex.transform.position;
        rotation = _complex.transform.rotation;

        velocity = _velocity;
        angularVelocity = _angularVelocity;

        axes = _axes;

        airDensity = complex.data.density.Get;

        fictionalConditions = true;

        InitializeMass();
    }

    public void UpdateFlightConditions(bool updateMass)
    {
        position = complex.tr.position;
        rotation = complex.tr.rotation;

        velocity = complex.rb.velocity;
        angularVelocity = complex.rb.angularVelocity;

        if (complex.aircraft) axes = complex.aircraft.controls.current;
        else axes = new AircraftAxes(0f, 0f, 0f);

        airDensity = complex.data.density.Get;

        if(updateMass) InitializeMass();
    }
    public void SimulateControls(AircraftAxes target, float dt)
    {
        axes = complex.aircraft.controls.SimulateControls(IAS, axes, target, dt);
    }
    public void ApplyForces(ResultingForce force, bool applyGravity, float dt)
    {
        if (applyGravity) velocity += Physics.gravity * dt;

        Vector3 previousVel = velocity;
        Vector3 previousAngularVel = angularVelocity;
        
        velocity += dt * massInvert * force.force;
        position += 0.5f * dt * (previousVel + velocity);

        Vector3 angularAcceleration = tensorInvert.MultiplyVector(force.torque);
        angularVelocity += angularAcceleration * dt;

        Vector3 averageAngularVelocity = 0.5f * (previousAngularVel + angularVelocity);

        float angleInRadians = averageAngularVelocity.magnitude * dt;
        Quaternion deltaRotation = Quaternion.AngleAxis(angleInRadians * Mathf.Rad2Deg, averageAngularVelocity);
        rotation = deltaRotation * rotation;
        rotation.Normalize();
    }

    public void ApplyForces(float dt)
    {
        if (!complex.aircraft) return;

        ResultingForce force = complex.aircraft.forcesCompiler.Compute(this);
        ApplyForces(force, true, dt);
    }
}
