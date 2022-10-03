using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Flap : Airframe
{
    public MiniAirfoil miniFoil;

    //Settings
    public float extendedRipSpeed = 60f;

    public override void Initialize(ObjectData d,bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            miniFoil.Init(transform);
        }
    }
    public override float MaxSpeed()
    {
        float input = aircraft ? aircraft.flapsInput : 0f;
        return Mathf.Lerp(base.MaxSpeed(), extendedRipSpeed, Mathf.Sqrt(input)); 
    }
    void FixedUpdate()
    {
        if (aircraft && data.ias > MaxSpeed() && Random.value < Time.fixedDeltaTime / 2f) Rip();
        else miniFoil.ApplyForces(this);
    }
#if UNITY_EDITOR
    //GIZMOS
    void OnDrawGizmos()
    {
        //CALCULATE AEROFOIL STRUCTURE
        miniFoil.Init(transform);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(miniFoil.rootLiftPos, miniFoil.tipLiftPos);
        Color fill = Color.magenta;
        fill.a = 0.06f;
        Features.DrawControlHandles(miniFoil.mainQuad.leadingBot, miniFoil.mainQuad.leadingTop, miniFoil.mainQuad.trailingTop, miniFoil.mainQuad.trailingBot, fill, Color.yellow);
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(Flap))]
public class FlapEditor : Editor
{
    Color backgroundColor;

    private static GUIContent deleteButton = new GUIContent("Remove", "Delete");
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        serializedObject.Update();
        Flap flap = (Flap)target;

        GUILayout.Space(20f);
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Configure with parent airfoil", MessageType.None);
        GUI.color = backgroundColor;

        //Damage model
        GUILayout.Space(20f);
        GUI.color = Color.green;
        EditorGUILayout.HelpBox("Flap", MessageType.None);
        GUI.color = backgroundColor;
        flap.extendedRipSpeed = EditorGUILayout.FloatField("Extended Rip Km/h", Mathf.Round(flap.extendedRipSpeed * 36f) / 10f) / 3.6f;
        EditorGUILayout.LabelField("Area", flap.miniFoil.mainQuad.area.ToString("0.00") + " m2");
        EditorGUILayout.LabelField("Mass", flap.emptyMass.ToString("0.00") + " kg");

        if (GUI.changed)
        {
            EditorUtility.SetDirty(flap);
            EditorSceneManager.MarkSceneDirty(flap.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }

}
#endif
