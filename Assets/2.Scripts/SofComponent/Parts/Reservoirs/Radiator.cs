using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Radiator : SofModule, IDamageTick
{
    public LiquidTank mainTank;
    public Circuit circuit;

    public override float EmptyMass => 0f;
    public override bool NoCustomMass => true;

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        circuit = new Circuit(transform, mainTank);
    }
    public void DamageTick(float dt)
    {
        if (structureDamage < 1f) circuit.Leaking(Time.fixedDeltaTime);
    }
    public override void KineticDamage(float damage, float caliber, float fireCoeff)
    {
        base.KineticDamage(damage, caliber, fireCoeff);
        circuit.Damage(caliber);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Radiator))]
public class RadiatorEditor : ModuleEditor
{
    static bool showRadiator = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Radiator rad = (Radiator)target;
        serializedObject.Update();

        showRadiator = EditorGUILayout.Foldout(showRadiator, "Radiator", true, EditorStyles.foldoutHeader);
        if (showRadiator)
        {
            EditorGUI.indentLevel++;
            rad.mainTank = EditorGUILayout.ObjectField("Attached Tank", rad.mainTank, typeof(LiquidTank), true) as LiquidTank;
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
