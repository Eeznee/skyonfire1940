using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

public enum MassCategory
{
    Real,
    Loaded,
    Empty
}
public interface IMassComponent
{
    public float Mass(MassCategory category)
    {
        if (category == MassCategory.Real) return RealMass;
        if (category == MassCategory.Loaded) return LoadedMass;
        else return EmptyMass;
    }
    public float LoadedMass { get; }
    public float RealMass { get; }
    public float EmptyMass { get; }
}