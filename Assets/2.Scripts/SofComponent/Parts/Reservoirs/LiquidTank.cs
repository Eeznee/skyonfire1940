using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


public class LiquidTank : SofModule
{
    public Liquid liquid;
    public float capacity;

    private float capacityInvert;

    public float fluidMass { get; private set; }
    public bool Empty { get { return fluidMass <= 0f; } }

    public Circuit circuit;

    private float massShift;
    const float massLostThreshold = 1f;

    public float fill { get { return fluidMass * capacityInvert; } }
    public override float AdditionalMass() { return Application.isPlaying ? fluidMass : capacity; }
    public override float EmptyMass() { return 0f; }

    public float CurrentAmount() { return fluidMass; }
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
        fluidMass = capacity;
        capacityInvert = 1f / capacity;

        circuit = new Circuit(transform, this);
        maxHp = material.hpPerSq * Mathf.Pow(capacity, 2f / 3f);
        massShift = 0f;
        base.Initialize(_complex);
    }
    public override void DamageTick(float dt)
    {
        base.DamageTick(dt);
        if (Integrity != 1f)
        {
            circuit.Leaking(dt);
            if (liquid.ignitable) Burning(dt);
        }

    }
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
    public override void KineticDamage(float damage, float caliber, float fireCoeff)
    {
        base.KineticDamage(damage, caliber, fireCoeff);
        circuit.Damage(caliber);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(LiquidTank)), CanEditMultipleObjects]
public class LiquidTankEditor : Editor
{
    SerializedProperty capacity;
    SerializedProperty content;

    protected virtual void OnEnable()
    {
        capacity = serializedObject.FindProperty("capacity");
        content = serializedObject.FindProperty("liquid");
    }
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        serializedObject.Update();

        LiquidTank tank = (LiquidTank)target;

        EditorGUILayout.PropertyField(capacity);
        EditorGUILayout.PropertyField(content);
        EditorGUILayout.LabelField("Capacity in Gallons : ", (tank.capacity / 4.55f).ToString("0.0"));
       // EditorGUILayout.LabelField("30 cal empty time : ", (tank.capacity / (Mathf.Pow(7.62f / 2000f, 2) * tank.liquid.leakSpeed * 1000f * Mathf.PI)).ToString("0") + " s");

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
