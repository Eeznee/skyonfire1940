using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


#if UNITY_EDITOR
[CustomEditor(typeof(SofAirframe)), CanEditMultipleObjects]
public class SofAirframeEditor : FrameEditor
{
    protected override string BasicName()
    {
        return "Airframe";
    }

    protected override void BasicFoldout()
    {
        base.BasicFoldout();

        SofAirframe frame = (SofAirframe)target;
        EditorGUILayout.LabelField("Area", frame.area.ToString("0.0") + " m²");
    }
}
#endif
