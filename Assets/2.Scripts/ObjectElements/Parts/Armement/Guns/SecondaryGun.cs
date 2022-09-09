using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SecondaryGun : Gun
{
    public override void Update()
    {
        base.Update();
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(SecondaryGun)), CanEditMultipleObjects]
public class SecondaryGunEditor : GunEditor
{
}
#endif
