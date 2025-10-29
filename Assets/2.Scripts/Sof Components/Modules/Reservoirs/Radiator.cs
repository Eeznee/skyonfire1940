using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Liquid Systems/Radiator")]
public class Radiator : SofModule, IDamageTick
{
    public LiquidTank mainTank;
    public Circuit circuit;

    public override ModuleArmorValues Armor(Collider collider)
    {
        return ModulesHPData.NoArmor;
    }
    public override float MaxHp => ModulesHPData.radiator;

    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);
        circuit = new Circuit(transform, mainTank);

        OnProjectileOrExplosionDamage += DamageCircuit;
    }
    public void DamageTick(float dt)
    {
        if (structureDamage < 1f) circuit.Leaking(Time.fixedDeltaTime);
    }
    public void DamageCircuit(float damage, float caliber, float fireCoeff)
    {
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
