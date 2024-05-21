using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Flap : ShapedAirframe
{
    //Settings
    public float extendedRipSpeed = 60f;

    public override float MaxSpd()
    {
        return Mathf.Lerp(base.MaxSpd(), extendedRipSpeed, aircraft.hydraulics.flaps.state);
    }
    public override void Initialize(SofComplex _complex)
    {
        foil = GetComponentInParent<Wing>().foil;
        base.Initialize(_complex);
    }
#if UNITY_EDITOR
    protected override Color FillColor()
    {
        return new Color(1f, 0f, 0.85f, 0.2f);
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(Flap))]
public class FlapEditor : ShapedAirframeEditor
{
    static bool showFlap = true;

    protected override void OnEnable()
    {
        base.OnEnable();
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        Flap flap = (Flap)target;

        showFlap = EditorGUILayout.Foldout(showFlap, "Flap", true, EditorStyles.foldoutHeader);
        if (showFlap)
        {
            EditorGUI.indentLevel++;

            flap.extendedRipSpeed = EditorGUILayout.FloatField("Extended Rip Km/h", Mathf.Round(flap.extendedRipSpeed * 36f) / 10f) / 3.6f;

            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

}
#endif
