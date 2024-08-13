using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Liquid", menuName = "SOF/Materials/Liquid")]
public class Liquid : ScriptableObject
{
    public LiquidType type = LiquidType.Coolant;
    public float density = 1f;
    public float boilingPoint = 100f;
    public ParticleSystem leakFx;
    public float leakSpeed = 5f;

    public bool ignitable = false;
    public float burningChance = 0.2f;
    public ParticleSystem burningFx;
}
public enum LiquidType
{
    Coolant,
    Oil,
    Fuel
}
