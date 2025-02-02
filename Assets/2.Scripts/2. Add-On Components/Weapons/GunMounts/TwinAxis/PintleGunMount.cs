using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
[AddComponentMenu("Sof Components/Weapons/Gun Mounts/Pintle")]
public class PintleGunMount : TwinAxisGunMount
{
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        verticalRate = horizontalRate;
    }
}
#if UNITY_EDITOR

[CustomEditor(typeof(PintleGunMount))]
public class PintleGunMountEditor : TwinAxisGunMountEditor
{
    SerializedProperty horizontalRate;

    protected override void OnEnable()
    {
        base.OnEnable();
        horizontalRate = serializedObject.FindProperty("horizontalRate");
    }

    protected override void MainSettings()
    {
        base.MainSettings();
        EditorGUILayout.PropertyField(horizontalRate, new GUIContent("Rotate Rate °/s"));
    }

}
#endif