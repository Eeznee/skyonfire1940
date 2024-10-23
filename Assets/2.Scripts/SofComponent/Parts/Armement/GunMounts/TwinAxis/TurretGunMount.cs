using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
[AddComponentMenu("Sof Components/Weapons/Gun Mounts/Turret")]
public class TurretGunMount : TwinAxisGunMount
{
    public override bool ForceJoystickControls => true;
}
#if UNITY_EDITOR

[CustomEditor(typeof(TurretGunMount))]
public class TurretGunMountEditor : TwinAxisGunMountEditor
{
    SerializedProperty horizontalRate;
    SerializedProperty verticalRate;

    protected override void OnEnable()
    {
        base.OnEnable();
        horizontalRate = serializedObject.FindProperty("horizontalRate");
        verticalRate = serializedObject.FindProperty("verticalRate");
    }

    protected override void MainSettings()
    {
        base.MainSettings();
        EditorGUILayout.PropertyField(horizontalRate, new GUIContent("Horizontal Rate °/s"));
        EditorGUILayout.PropertyField(verticalRate, new GUIContent("Vertical Rate °/s"));
    }

}
#endif