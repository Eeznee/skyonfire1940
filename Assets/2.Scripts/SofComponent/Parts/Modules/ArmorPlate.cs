using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class ArmorPlate : SofModule
{
    public float thickness;
    public override float MaxHp => 1000f;

    public override ModuleArmorValues Armor => new ModuleArmorValues(thickness,0f);

}
#if UNITY_EDITOR
[CustomEditor(typeof(ArmorPlate)), CanEditMultipleObjects]
public class ArmorPlateEditor : ModuleEditor
{
    SerializedProperty thickness;
    protected override void OnEnable()
    {
        base.OnEnable();
        thickness = serializedObject.FindProperty("thickness");
    }
    protected override string BasicName()
    {
        return "Armor Plate";
    }

    protected override void BasicFoldout()
    {
        base.BasicFoldout();

        EditorGUILayout.PropertyField(thickness);
    }
}
#endif
