using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class ArmorPlate : SofModule, IMassComponent
{
    public float LoadedMass => mass;
    public float EmptyMass => mass;
    public float RealMass => mass;

    public float thickness = 6f;
    public float mass = 0f;
    public override float MaxHp => 1000f;

    public override ModuleArmorValues Armor => new ModuleArmorValues(thickness,0f);

    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);
        if (thickness <= 0f) Debug.LogError("This armor plate (" + name + ") has no thickness value", this);
    }

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
