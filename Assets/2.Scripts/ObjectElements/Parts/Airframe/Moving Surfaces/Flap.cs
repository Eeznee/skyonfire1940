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
        return Mathf.Lerp(base.MaxSpd(), extendedRipSpeed, aircraft.flaps.state);
    }
    public override void Initialize(ObjectData d,bool firstTime)
    {
        if (firstTime)
            foil = GetComponentInParent<Wing>().foil;
        base.Initialize(d, firstTime);
    }
#if UNITY_EDITOR
    protected override void Draw() { aero.quad.Draw(new Color(1f, 0f, 1f, 0.06f),Color.yellow,false); }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(Flap))]
public class FlapEditor : ShapedAirframeEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Color backgroundColor = GUI.backgroundColor;
        serializedObject.Update();
        Flap flap = (Flap)target;

        GUILayout.Space(20f);
        GUI.color = Color.magenta;
        EditorGUILayout.HelpBox("Flap", MessageType.None);
        GUI.color = backgroundColor;
        flap.extendedRipSpeed = EditorGUILayout.FloatField("Extended Rip Km/h", Mathf.Round(flap.extendedRipSpeed * 36f) / 10f) / 3.6f;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(flap);
            EditorSceneManager.MarkSceneDirty(flap.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }

}
#endif
