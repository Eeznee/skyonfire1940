using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[CreateAssetMenu(fileName = "New Tire Friction", menuName = "SOF/Materials/Tire Friction")]
public class TireFriction : ScriptableObject
{
    public float peakFriction = 1.2f;
    public float peakSlip = 0.4f;
    public float extremumFriction = 0.8f;
    public float hardness = 1.5f;

    public AnimationCurve frictionPlot;

    public ParticleSystem frictionEffect;

    public float Friction(float slipFactor)
    {
        slipFactor = Mathf.Abs(slipFactor);
        return slipFactor < peakSlip ? ZeroToPeak(slipFactor) : PeakToLimit(slipFactor);
    }
    private float ZeroToPeak(float slipFactor)
    {
        float x = slipFactor / peakSlip;
        return peakFriction * x;
    }
    private float PeakToLimit(float slipFactor)
    {
        float x = Mathf.Clamp01((slipFactor - peakSlip) * hardness / (1f - peakSlip));

        float t = Mathf.Lerp(Mathv.SmoothStart(x, 2), Mathv.SmoothStop(x, 6), Mathv.SmoothStop(x, 4));
        return Mathf.Lerp(peakFriction, extremumFriction, t);
    }

    public void SendToCurve(float step)
    {
        frictionPlot = AnimationCurve.Linear(0f, 0f,1f, extremumFriction);
        for (float i = 0f; i <= 1f + step; i += step)
            frictionPlot.AddKey(new Keyframe(i, Friction(i)));
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(TireFriction))]
public class TireFrictionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        TireFriction tire = (TireFriction)target;

        GUI.color = GUI.backgroundColor;

        EditorGUILayout.Slider(serializedObject.FindProperty("peakFriction"), 0.1f, 2f);
        EditorGUILayout.Slider(serializedObject.FindProperty("peakSlip"), 0f, 0.5f);
        EditorGUILayout.Slider(serializedObject.FindProperty("extremumFriction"), 0.1f, 1f);
        EditorGUILayout.Slider(serializedObject.FindProperty("hardness"), 0f, 5f);

        tire.SendToCurve(0.01f);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("frictionPlot"), new GUIContent("FRICTION v SLIP"),true);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("frictionEffect"));

        serializedObject.ApplyModifiedProperties();
    }
}
#endif