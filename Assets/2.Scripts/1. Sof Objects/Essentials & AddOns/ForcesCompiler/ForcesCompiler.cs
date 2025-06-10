using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForcesCompiler : SofComponent
{
    IAircraftForce[] forcesComponents;
    ForceAtPoint[] forcesAtPoints;

    ResultingForce force;

#if UNITY_EDITOR
    private void LateUpdate()
    {
        return;
        for (int i = 0; i < forcesAtPoints.Length; i++)
        {
            Vector3 force = forcesAtPoints[i].force;
            Vector3 point = forcesAtPoints[i].point + tr.position;

            Vector3 drag = Vector3.Project(force, rb.GetPointVelocity(point));
            Vector3 other = force - drag;
            Debug.DrawRay(point, drag * 0.01f, Color.red);
            Debug.DrawRay(point, other * 0.001f, Color.cyan);
        }
    }
#endif

    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);

        ReloadForceComponents();

        sofModular.onComponentAdded += OnComponentAdded;
        sofModular.onComponentRootRemoved += OnComponentRemoved;
    }
    public void OnComponentAdded(SofComponent component)
    {
        ReloadForceComponents();
    }
    public void OnComponentRemoved(SofComponent component)
    {
        ReloadForceComponents();
    }
    public void ReloadForceComponents()
    {
        forcesComponents = aircraft.GetComponentsInChildren<IAircraftForce>();
        forcesAtPoints = new ForceAtPoint[forcesComponents.Length];
    }
    public void ApplyForcesOnFixedUpdate()
    {
        force = ComputeCurrent();
        rb.AddForce(force.force);
        rb.AddTorque(force.torque);
    }
    public ResultingForce ComputeCurrent()
    {
        FlightConditions flightConditions = new FlightConditions(aircraft, false);
        return Compute(flightConditions);
    }

    public ResultingForce Compute(FlightConditions flightConditions)
    {
        for (int i = 0; i < forcesComponents.Length; i++)
        {
            forcesAtPoints[i] = forcesComponents[i].SimulatePointForce(flightConditions);
            forcesAtPoints[i].point -= flightConditions.WorldCenterOfMass;
        }

        return new ResultingForce(forcesAtPoints);
    }
}
