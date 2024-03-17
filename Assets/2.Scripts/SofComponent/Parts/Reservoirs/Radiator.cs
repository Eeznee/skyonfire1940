using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Radiator : SofModule
{
    public LiquidTank mainTank;
    public Circuit circuit;

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        circuit = new Circuit(transform, mainTank);
    }
    public override void DamageTick(float dt)
    {
        base.DamageTick(dt);
        if (StructureIntegrity() < 1f) circuit.Leaking(Time.fixedDeltaTime);
    }
    public override void KineticDamage(float damage, float caliber, float fireCoeff)
    {
        base.KineticDamage(damage, caliber, fireCoeff);
        circuit.Damage(caliber);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Radiator))]
public class RadiatorEditor : Editor
{
    Color backgroundColor;
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        //
        Radiator rad = (Radiator)target;
        //
        serializedObject.Update();

        rad.material = EditorGUILayout.ObjectField("Material", rad.material, typeof(ModuleMaterial), false) as ModuleMaterial;
        rad.mainTank = EditorGUILayout.ObjectField("Attached Tank", rad.mainTank, typeof(LiquidTank), true) as LiquidTank;


        if (GUI.changed)
        {
            EditorUtility.SetDirty(rad);
            EditorSceneManager.MarkSceneDirty(rad.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
