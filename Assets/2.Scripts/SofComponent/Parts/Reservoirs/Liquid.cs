using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Liquid", menuName = "Aircraft/Liquid")]
public class Liquid : ScriptableObject
{
    public LiquidType type = LiquidType.Coolant;
    public float density = 1f;
    public float boilingPoint = 100f;
    public ModuleMaterial material;
    public ParticleSystem leakFx;
    public float leakSpeed = 5f;
    public bool ignitable = false;


}
public enum LiquidType
{
    Coolant,
    Oil,
    Fuel
}
