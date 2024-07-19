using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class FuelTank : LiquidTank, IDamageTick
{
    public bool selfSealing = true;

    new public void DamageTick(float dt)
    {
        base.DamageTick(dt);
        if (structureDamage > 0.85f && selfSealing) circuit.holesArea = 0f;
    }
}
//
#if UNITY_EDITOR
[CustomEditor(typeof(FuelTank))]
public class FuelTankEditor : LiquidTankEditor
{
    SerializedProperty selfSealing;


    static bool showFuelTank = true;
    protected override void OnEnable()
    {
        base.OnEnable();
        selfSealing = serializedObject.FindProperty("selfSealing");
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        showFuelTank = EditorGUILayout.Foldout(showFuelTank, "Fuel Tank", true, EditorStyles.foldoutHeader);
        if (showFuelTank)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(selfSealing);
            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
