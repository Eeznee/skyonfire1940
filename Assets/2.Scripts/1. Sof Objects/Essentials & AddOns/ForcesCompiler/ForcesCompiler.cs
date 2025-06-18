using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForcesCompiler : SofComponent
{
    IAircraftForce[] forcesComponents;
    ForceAtPoint[] forcesAtPoints;

    ResultingForce force;

    FlightConditions realTimeFlightConditions;

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

        realTimeFlightConditions = new(aircraft, false);
        previousMass = aircraft.GetMass();
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

    float previousMass;
    public ResultingForce ComputeCurrent()
    {
        bool differentMass = previousMass != aircraft.GetMass();
        if (differentMass)
        {
            previousMass = aircraft.GetMass();
        }
        realTimeFlightConditions.UpdateFlightConditions(differentMass);

        return Compute(realTimeFlightConditions);
    }

    public ResultingForce Compute(FlightConditions flightConditions)
    {
        Vector3 worldCenterOfMass = flightConditions.WorldCenterOfMass;
        for (int i = 0; i < forcesComponents.Length; i++)
        {
            forcesAtPoints[i] = forcesComponents[i].SimulatePointForce(flightConditions);
            forcesAtPoints[i].point -= worldCenterOfMass;
        }

        return new ResultingForce(forcesAtPoints);
    }
}
