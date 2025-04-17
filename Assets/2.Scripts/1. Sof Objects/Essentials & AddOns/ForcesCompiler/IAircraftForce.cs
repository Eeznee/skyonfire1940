using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAircraftForce
{
    public ForceAtPoint SimulatePointForce(FlightConditions flightConditions);
    public string name { get; }
}
