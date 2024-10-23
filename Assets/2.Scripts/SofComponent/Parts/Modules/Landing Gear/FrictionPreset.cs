using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
[CreateAssetMenu(fileName = "New Tire Friction", menuName = "SOF/Materials/Tire Friction")]
public class FrictionPreset : ScriptableObject
{
    public const int LUT_RESOLUTION = 1000;

    /// <summary>
    ///     B, C, D and E parameters of short version of Pacejka's magic formula.
    /// </summary>
    [Tooltip("    B, C, D and E parameters of short version of Pacejka's magic formula.")]
    public Vector4 BCDE;

    /// <summary>
    /// Slip at which the friction preset has highest friction.
    /// </summary>
    [UnityEngine.Tooltip("Slip at which the friction preset has highest friction.")]
    public float peakSlip = 0.12f;

    [SerializeField]
    private AnimationCurve _curve;

    public AnimationCurve Curve
    {
        get { return _curve; }
    }

    /// <summary>
    /// Gets the slip at which the friction is the highest for this friction curve.
    /// </summary>
    /// <returns></returns>
    public float GetPeakSlip()
    {
        float peakSlip = -1;
        float yMax = 0;

        for (float i = 0; i < 1f; i += 0.01f)
        {
            float y = _curve.Evaluate(i);
            if (y > yMax)
            {
                yMax = y;
                peakSlip = i;
            }
        }

        return peakSlip;
    }


    /// <summary>
    ///     Generate Curve from B,C,D and E parameters of Pacejka's simplified magic formula
    /// </summary>
    public void UpdateFrictionCurve()
    {
        _curve = new AnimationCurve();
        Keyframe[] frames = new Keyframe[20];
        int n = frames.Length;
        float t = 0;

        for (int i = 0; i < n; i++)
        {
            float v = GetFrictionValue(t, BCDE);
            _curve.AddKey(t, v);

            if (i <= 10)
                t += 0.02f;
            else
                t += 0.1f;
        }

        for (int i = 0; i < n; i++)
            _curve.SmoothTangents(i, 0f);

        peakSlip = GetPeakSlip();
    }

    const float sideFrictionLoadMultiplier = 1.9f;

    public float SideFriction(RaycastHit hit, Wheel wheel)
    {
        float sideLoadFactor = wheel.load * sideFrictionLoadMultiplier;
        float camberFactor = Vector3.Dot(wheel.up, hit.normal);

        float clampedAbsForwardSpeed = Mathf.Max(Mathf.Abs(wheel.forwardSpeed), 5f);

        float sideSlip = Mathf.Atan2(wheel.sideSpeed, clampedAbsForwardSpeed) * Mathf.Rad2Deg * 0.01111f;
        float frictionCoeff = Curve.Evaluate(Mathf.Abs(sideSlip));

        return -Mathf.Sign(sideSlip) * frictionCoeff * sideLoadFactor * wheel.frictionMultiplier * camberFactor;
    }

    private static float GetFrictionValue(float slip, Vector4 p)
    {
        float B = p.x;
        float C = p.y;
        float D = p.z;
        float E = p.w;
        float t = Mathf.Abs(slip);
        return D * Mathf.Sin(C * Mathf.Atan(B * t - E * (B * t - Mathf.Atan(B * t))));
    }
}


#if UNITY_EDITOR

/// <summary>
///     Editor for FrictionPreset.
/// </summary>
[CustomEditor(typeof(FrictionPreset))]
[CanEditMultipleObjects]
public class FrictionPresetEditor : Editor
{
    private FrictionPreset preset;


    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        preset = (FrictionPreset)target;
        Vector4 startBCDE = preset.BCDE;
        float B = preset.BCDE.x;
        float C = preset.BCDE.y;
        float D = preset.BCDE.z;
        float E = preset.BCDE.w;

        B = EditorGUILayout.Slider(new GUIContent("Stifness"),B, 0f, 30f);
        C = EditorGUILayout.Slider(new GUIContent("Shape"), C, 0f, 5f);
        D = EditorGUILayout.Slider(new GUIContent("Peak Friction"), D, 0f, 2f);
        E = EditorGUILayout.Slider(new GUIContent("Curvature"), E, 0f, 2f);


        EditorGUILayout.HelpBox("Resulting curve", MessageType.Info);
        EditorGUILayout.CurveField(preset.Curve,GUILayout.Height(100f));

        preset.BCDE = new Vector4(B, C, D, E);

        if (preset.BCDE != startBCDE)
        {
            preset.UpdateFrictionCurve();
            Undo.RecordObject(target, "Modified FrictionPreset");
            EditorUtility.SetDirty(target);
        }

        serializedObject.ApplyModifiedProperties();
    }
}

#endif