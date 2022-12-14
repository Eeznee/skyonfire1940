using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Radiator : Part
{
    public LiquidTank mainTank;
    public LiquidTank.LiquidCircuit circuit;

    public override void Initialize(ObjectData d,bool firstTime)
    {
        base.Initialize(d,firstTime);
        if (firstTime)
        {
            circuit = new LiquidTank.LiquidCircuit(this, mainTank, mainTank.escapeSpeed / 2f);
        }
    }

    private void FixedUpdate()
    {
        circuit.Leaking();
        Burning();
    }
    public override void Damage(float damage, float caliber, float fireCoeff)
    {
        base.Damage(damage, caliber, fireCoeff);
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

        rad.material = EditorGUILayout.ObjectField("Material", rad.material, typeof(PartMaterial), false) as PartMaterial;
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
