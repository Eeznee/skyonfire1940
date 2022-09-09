using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class PrimaryGun : Gun
{
    public override void Update()
    {
        base.Update();
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(PrimaryGun)), CanEditMultipleObjects]
public class PrimaryGunEditor : GunEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif