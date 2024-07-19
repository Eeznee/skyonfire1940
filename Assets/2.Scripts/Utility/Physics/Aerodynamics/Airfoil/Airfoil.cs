using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Airfoil", menuName = "SOF/Aircraft Modules/Airfoil")]
public class Airfoil : ScriptableObject
{
    public enum AirfoilType
    {
        Standard,
        Symmetrical,
        WithFlaps
    }
    public AirfoilType airfoilType = AirfoilType.Standard;

    public AirfoilSim airfoilSim = new AirfoilSim(0f,1f,15f,-10f,0.01f);
    public AirfoilSim flappedAirfoilSim = new AirfoilSim(0.5f,1.5f,15f,-5f,0.1f);

    public float editorFlaps = 0f;
    public AnimationCurve clPlot;
    public AnimationCurve cdPlot;
    public AnimationCurve testPlot;

    public float Gradient(float flaps)
    {
        if (flaps <= 0f) return airfoilSim.Gradient();
        return Mathf.Lerp(airfoilSim.Gradient(),flappedAirfoilSim.Gradient(),flaps);
    }
    public float Gradient() { return Gradient(0f); }
    public Vector2 Coefficients(float alpha, float flaps)
    {
        if (flaps <= 0f) return airfoilSim.Coefficients(alpha);
        return Vector2.Lerp(airfoilSim.Coefficients(alpha), flappedAirfoilSim.Coefficients(alpha), flaps);
    }
    public Vector2 Coefficients(float alpha) { return Coefficients(alpha, 0f); }
    public void SendToCurve(float from,float to,float step)
    {
        airfoilSim.UpdateAirfoilQuarterTools();
        flappedAirfoilSim.UpdateAirfoilQuarterTools();

        clPlot = AnimationCurve.Constant(-from, to, 0f);
        cdPlot = AnimationCurve.Constant(-from, to, 0f);
        testPlot = AnimationCurve.Constant(-from, to, 0f);
        for (float i = from; i < to; i += step)
        {
            Vector2 coeffs = airfoilType == AirfoilType.WithFlaps ? Coefficients(i,editorFlaps) : Coefficients(i);
            clPlot.AddKey(new Keyframe(i, coeffs.y));
            cdPlot.AddKey(new Keyframe(i, coeffs.x));
            if (coeffs.x != 0f) testPlot.AddKey(new Keyframe(i, coeffs.y / coeffs.x));
        }
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Airfoil))]
public class AirfoilEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Airfoil airfoil = (Airfoil)target;

        GUI.color = GUI.backgroundColor;

        EditorGUI.BeginChangeCheck();

        airfoil.airfoilType = (Airfoil.AirfoilType)EditorGUILayout.EnumPopup("Type", airfoil.airfoilType);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("airfoilSim"), true);
        if (airfoil.airfoilType == Airfoil.AirfoilType.Symmetrical)
        {
            airfoil.airfoilSim.zeroCl = 0f;
            airfoil.airfoilSim.minAlpha = -airfoil.airfoilSim.maxAlpha;
        }

        if (airfoil.airfoilType == Airfoil.AirfoilType.WithFlaps)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("flappedAirfoilSim"), true);

        GUILayout.Space(20f);
        GUI.color = Color.green;
        EditorGUILayout.HelpBox("Preview Curves", MessageType.None);
        GUI.color = GUI.backgroundColor;

        if (airfoil.airfoilType == Airfoil.AirfoilType.WithFlaps)
            airfoil.editorFlaps = EditorGUILayout.Slider("Flaps preview", airfoil.editorFlaps, 0f, 1f);

        airfoil.SendToCurve(-90f, 90f, 1f);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("clPlot"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cdPlot"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("testPlot"), true);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(airfoil);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif