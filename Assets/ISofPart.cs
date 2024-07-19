using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMassComponent
{
    public float Mass => AdditionalMass() + EmptyMass();
    public float AdditionalMass();
    public float EmptyMass();
    public bool NoCustomMass();
}
public interface ICustomMass : IMassComponent
{

}
public interface IDeterminedMass : IMassComponent
{

}
public interface IHealthComponent
{

}