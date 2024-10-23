using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Liquid Systems/Liquid Tank")]
public class LiquidTank : SofModule, IDamageTick, IMassComponent, IIgnitable
{
    public override float MaxHp => ModulesHPData.liquidTankHpRatio * Mathf.Pow(capacity, 2f / 3f);
    public float EmptyMass => 0f;
    public float LoadedMass => capacity;
    public float RealMass => fluidMass;
    public override ModuleArmorValues Armor => new ModuleArmorValues(armorThickness, Mathf.Sqrt(fluidMass));


    public Liquid liquid;
    public float capacity;
    public bool selfSealing = false;
    public float armorThickness = 0f;

    public float fluidMass { get; private set; }

    private Circuit circuit;

    private float capacityInvert;
    private float massShift;

    public bool Ignitable => liquid.ignitable;
    public float BurningChance => liquid.burningChance;
    public float MaxStructureDamageToBurn => 0f;
    public ParticleSystem BurningEffect => liquid.burningFx;
    public bool Empty => fluidMass <= 0f;
    public float FillRatio => fluidMass * capacityInvert;


    public override void Rearm()
    {
        base.Rearm();
        circuit.holesArea = 0f;
        Repair();
        ShiftFluidMass(capacity - fluidMass);
        circuit.Leaking(Time.deltaTime);
    }
    public override void Initialize(SofComplex _complex)
    {
        if (!liquid) Debug.LogError("This Liquid Tank does not have any liquids assigned", this);
        if(liquid.type != LiquidType.Fuel)
        {
            selfSealing = false;
            armorThickness = 0f;
        }
        fluidMass = capacity;
        capacityInvert = 1f / capacity;

        circuit = new Circuit(transform, this);
        massShift = 0f;
        base.Initialize(_complex);

        OnProjectileDamage += DamageCircuit;
    }
    public void DamageTick(float dt)
    {
        if (structureDamage >= 1f) return;
        if (selfSealing && structureDamage > 0f) { circuit.holesArea = 0f; return; }

        circuit.Leaking(dt);
    }

    const float massLostThreshold = 1f;
    public void ShiftFluidMass(float addedMass)
    {
        if (!complex) return;

        float initialMass = fluidMass;
        fluidMass = Mathf.Clamp(fluidMass + addedMass, 0f, capacity);

        massShift += fluidMass - initialMass;
        if (Mathf.Abs(massShift) > massLostThreshold)
        {
            complex.ShiftMass(new Mass(massShift, localPos));
            massShift = 0f;
        }
    }
    public void DamageCircuit(float damage, float caliber, float fireCoeff)
    {
        circuit.Damage(caliber);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(LiquidTank)), CanEditMultipleObjects]
public class LiquidTankEditor : ModuleEditor
{
    SerializedProperty capacity;
    SerializedProperty content;
    SerializedProperty selfSealing;
    SerializedProperty armorThickness;

    static bool showLiquidTank = true;

    protected override void OnEnable()
    {
        base.OnEnable();
        capacity = serializedObject.FindProperty("capacity");
        content = serializedObject.FindProperty("liquid");
        selfSealing = serializedObject.FindProperty("selfSealing");
        armorThickness = serializedObject.FindProperty("armorThickness");
    }
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        LiquidTank tank = (LiquidTank)target;


        showLiquidTank = EditorGUILayout.Foldout(showLiquidTank, "Liquid Tank", true, EditorStyles.foldoutHeader);
        if (showLiquidTank)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(content);
            EditorGUILayout.PropertyField(capacity);
            EditorGUILayout.LabelField("Capacity in Gallons : ", (tank.capacity / 4.55f).ToString("0.0"));
            if(tank.liquid && tank.liquid.type == LiquidType.Fuel)
            {
                EditorGUILayout.PropertyField(selfSealing);
                EditorGUILayout.PropertyField(armorThickness);
            }

            EditorGUI.indentLevel--;
        }
       // EditorGUILayout.LabelField("30 cal empty time : ", (tank.capacity / (Mathf.Pow(7.62f / 2000f, 2) * tank.liquid.leakSpeed * 1000f * Mathf.PI)).ToString("0") + " s");

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
