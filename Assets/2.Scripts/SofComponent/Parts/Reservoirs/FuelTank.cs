using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class FuelTank : LiquidTank
{
    public bool selfSealing = true;

    public override void DamageTick(float dt)
    {
        base.DamageTick(dt);
        if (Integrity > 0.85f && selfSealing) circuit.holesArea = 0f;
    }
}
//
#if UNITY_EDITOR
[CustomEditor(typeof(FuelTank))]
public class FuelTankEditor : LiquidTankEditor
{
    SerializedProperty selfSealing;
    protected override void OnEnable()
    {
        base.OnEnable();
        selfSealing = serializedObject.FindProperty("selfSealing");
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.PropertyField(selfSealing);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
