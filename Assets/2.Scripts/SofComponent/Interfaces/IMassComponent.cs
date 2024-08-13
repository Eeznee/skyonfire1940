using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;


public interface IMassComponent
{
    public float LoadedMass { get; }
    public float EmptyMass { get; }
}