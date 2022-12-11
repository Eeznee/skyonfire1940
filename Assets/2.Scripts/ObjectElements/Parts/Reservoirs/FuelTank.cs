using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class FuelTank : LiquidTank
{
    public FuelTank symmetry;
    public bool selfSealing = true;

    public override void Initialize(ObjectData d,bool firstTime)
    {
        base.Initialize(d,firstTime);
        burningRatios = FlightModel.BurningCollateralRatios(this);
    }

    private void Update()
    {
        if (structureDamage != 1f)
            circuit.Leaking();
        Burning();
        if (structureDamage > 0.85f && selfSealing) circuit.holesArea = 0f;
    }
    public override void Damage(float damage, float caliber, float fireCoeff)
    {
        base.Damage(damage, caliber, fireCoeff);
        if (!burning) TryBurn(caliber, fireCoeff);
    }

    public void ConsumeControlled(float amount)
    {
        if (symmetry && symmetry.currentAmount > 0f)
        {
            amount /= 2f;
            symmetry.Consume(amount);
        }
        Consume(amount);
    }
}
//
#if UNITY_EDITOR
[CustomEditor(typeof(FuelTank))]
public class FuelTankEditor : LiquidTankEditor
{
    Color backgroundColor;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        backgroundColor = GUI.backgroundColor;
        //
        FuelTank tank = (FuelTank)target;
        //
        serializedObject.Update();
        tank.symmetry = EditorGUILayout.ObjectField("Symmetric Tank", tank.symmetry,typeof(FuelTank), true) as FuelTank;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(tank);
            EditorSceneManager.MarkSceneDirty(tank.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
