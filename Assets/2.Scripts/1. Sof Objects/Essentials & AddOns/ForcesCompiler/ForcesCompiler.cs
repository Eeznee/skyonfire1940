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
            Debug.DrawRay(forcesAtPoints[i].point + tr.position, forcesAtPoints[i].force * 0.001f, Color.red);
        }
    }
#endif

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        ReloadForceComponents();

        complex.onComponentAdded += OnComponentAdded;
        complex.onComponentRootRemoved += OnComponentRemoved;
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
