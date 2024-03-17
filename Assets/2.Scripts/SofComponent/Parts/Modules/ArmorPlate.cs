using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class ArmorPlate : SofModule
{

}
#if UNITY_EDITOR
[CustomEditor(typeof(ArmorPlate)), CanEditMultipleObjects]
public class ArmorPlateEditor : PartEditor
{
    SerializedProperty material;
    protected override void OnEnable()
    {
        base.OnEnable();
        material = serializedObject.FindProperty("material");
    }
    protected override string BasicName()
    {
        return "Armor Plate";
    }
    protected override void BasicFoldout()
    {
        base.BasicFoldout();

        ArmorPlate module = (ArmorPlate)target;

        EditorGUILayout.PropertyField(material);
    }
}
#endif