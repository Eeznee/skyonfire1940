using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct ModuleArmorValues
{
    public ModuleArmorValues(float _surface, float _fullPen)
    {
        surfaceArmor = _surface;
        fullPenArmor = _fullPen;
    }
    public float surfaceArmor;
    public float fullPenArmor;
}
public static class ModulesHPData
{
    //Fixed HP
    public const float radiator = 10f;
    public const float crewmember = 5f;

    public const float engineInLine = 36f;
    public const float engineRadial = 60f;
    public const float engineJet = 25f;


    //Hp per square meter
    public const float frameHpPerSq = 40f;
    public const float controlHpPerSq = 25f;
    public const float stabilizerHpPerSq = 30f;
    public const float sparHpPerSq = 45f;

    public const float wheelHpPerSq = 70f;


    //Hp based on other ratios
    public const float liquidTankHpRatio = 0.15f;



    //Armor values
    public static ModuleArmorValues DuraluminArmor => new ModuleArmorValues(0.25f,0.5f);
    public static ModuleArmorValues EngineArmor => new ModuleArmorValues(2f, 20f);
    public static ModuleArmorValues WheelArmor => new ModuleArmorValues(0.5f, 2f);
    public static ModuleArmorValues CrewmemberArmor => new ModuleArmorValues(0f, 1f);
    public static ModuleArmorValues SparArmor => new ModuleArmorValues(0f, 4f);
}
