using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IIgnitable
{
    public bool Ignitable { get; }
    public float BurningChance { get; }
    public float MaxStructureDamageToBurn { get; }
    public ParticleSystem BurningEffect { get; }
}