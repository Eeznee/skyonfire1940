using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif


[Serializable]
public class PropellerChart
{
    public float minPitchAdvanceRatio;
    public float minPitchSpeed;

    public float maxPitchAdvanceRatio;
    public float maxPitchSpeed;

    public AnimationCurve EfficiencyCurve;
    public AnimationCurve ThrustRatioCurve;


    public PropellerChart(P2 propPreset, EnginePreset engine)
    {
        float propellerRps = engine.fullRps * propPreset.reductionGear;

        minPitchAdvanceRatio = propPreset.AdvanceRatio(propPreset.minPitch);
        maxPitchAdvanceRatio = propPreset.AdvanceRatio(propPreset.maxPitch);

        minPitchSpeed = minPitchAdvanceRatio * propPreset.Radius * propellerRps;
        maxPitchSpeed = maxPitchAdvanceRatio * propPreset.Radius * propellerRps;

#if UNITY_EDITOR

        float MaxSpeed = 250f;
        float increment = 2f;

        EfficiencyCurve = AnimationCurve.Linear(0f, propPreset.Efficiency(propellerRps, 0f), MaxSpeed * 3.6f, propPreset.Efficiency(propellerRps, MaxSpeed));
        ThrustRatioCurve = AnimationCurve.Linear(0f, propPreset.PowerToThrust(propellerRps, 0f), MaxSpeed * 3.6f, propPreset.PowerToThrust(propellerRps, MaxSpeed));

        for (float speed = 0f; speed < MaxSpeed; speed += increment)
        {
            float efficiency = propPreset.Efficiency(propellerRps, speed);
            float powerToThrust = propPreset.PowerToThrust(propellerRps, speed);

            EfficiencyCurve.AddKey(new Keyframe(speed * 3.6f, efficiency));
            ThrustRatioCurve.AddKey(new Keyframe(speed * 3.6f, powerToThrust));
        }
        for (int i = 0; i < EfficiencyCurve.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(EfficiencyCurve, i, AnimationUtility.TangentMode.Auto);
            AnimationUtility.SetKeyLeftTangentMode(ThrustRatioCurve, i, AnimationUtility.TangentMode.Auto);
            AnimationUtility.SetKeyRightTangentMode(EfficiencyCurve, i, AnimationUtility.TangentMode.Auto);
            AnimationUtility.SetKeyRightTangentMode(ThrustRatioCurve, i, AnimationUtility.TangentMode.Auto);
        }
#endif
    }
}
[CreateAssetMenu(fileName = "New PPropeller", menuName = "SOF/Aircraft Modules/PPropeller")]
public class P2 : ScriptableObject
{
    public enum PitchControl
    {
        Fixed,
        TwoPitch,
        ConstantSpeed
    }
    public PitchControl pitchControl;

    public float mass = 150f;
    public int bladesAmount = 3;
    public float diameter = 3f;
    public float reductionGear = 0.5f;

    public float minPitch = 30f;
    public float maxPitch = 60f;


    public EnginePreset attachedTestEngine;

    [SerializeField]public PropellerChart propellerChart;


    const float lowestEfficientAdvanceRatio = 0.1f;
    const float minimumAllowedPitchAngle = 20f;
    const float cubicRoot = 1f / 3f;

    public float Radius => diameter * 0.5f;
    public bool CanBeFeathered => pitchControl == PitchControl.ConstantSpeed && maxPitch >= 80f;


    private Vector2 minPitchParams;
    private Vector2 maxPitchParams;


    public void UpdateValues()
    {
        minPitch = Mathf.Clamp(minPitch, minimumAllowedPitchAngle, 90f);
        maxPitch = Mathf.Clamp(maxPitch, minimumAllowedPitchAngle, 90f);
        if (pitchControl == PitchControl.Fixed) minPitch = maxPitch;

        minPitchParams = FindEfficiencyParameters(minPitch);
        maxPitchParams = FindEfficiencyParameters(maxPitch);

        propellerChart = new PropellerChart(this, attachedTestEngine);
    }
    public Vector2 FindEfficiencyParameters(float bladePitchAngle)
    {
        float advancedRatio = AdvanceRatio(bladePitchAngle);
        float adjustedRatio = advancedRatio / lowestEfficientAdvanceRatio;

        float x = (M.Pow(adjustedRatio, 4) - M.Pow(adjustedRatio, 2)) / 3f;
        x = Mathf.Max(x, 0f);
        float y = 2f / (3f * adjustedRatio) + 4f * adjustedRatio / 3f;

        return new Vector2(x, y);
    }
    public float AdvanceRatio(float radPerSec, float tas)
{
        if (radPerSec == 0f) return 0f;

        return tas / (radPerSec * Radius);
    }
    public float AdvanceRatio(float bladeAngleDeg) { return Mathf.Tan(bladeAngleDeg * Mathf.Deg2Rad); }
    private float BladeAngle(float advanceRatio) { return Mathf.Atan(advanceRatio) * Mathf.Rad2Deg; }
    private float BladeAngle(float radPerSec, float tas) { return BladeAngle(AdvanceRatio(radPerSec, tas)); }


    public float Efficiency(float radPerSec, float tas)
    {
        return PowerToThrust(radPerSec, tas) * tas;
    }
    public float PowerToThrust(float radPerSec, float tas)
    {
        if (radPerSec == 0f) return 0f;
        float advanceRatio = AdvanceRatio(radPerSec, tas);
        float bladeAngle = BladeAngle(advanceRatio);
        switch (pitchControl)
        {
            case PitchControl.Fixed: return PowerToThrust(radPerSec, tas, maxPitchParams);

            case PitchControl.TwoPitch:
                if (bladeAngle < minPitch) return PowerToThrust(radPerSec, tas, minPitchParams);
                if (bladeAngle > maxPitch) return PowerToThrust(radPerSec, tas, maxPitchParams);
                return Mathf.Max(PowerToThrust(radPerSec, tas, minPitchParams), PowerToThrust(radPerSec, tas, maxPitchParams));

            case PitchControl.ConstantSpeed:
                if (bladeAngle < minPitch) return PowerToThrust(radPerSec, tas, minPitchParams);
                if(bladeAngle > maxPitch) return PowerToThrust(radPerSec, tas, maxPitchParams);
                return 1f/tas;


            default: return 1f/tas;
        }
    }
    public float Efficiency(float radPerSec, float tas, Vector2 efficiencyParameters)
    {
        return PowerToThrust(radPerSec, tas, efficiencyParameters) * tas;
    }
    public float PowerToThrust(float radPerSec, float tas, Vector2 efficiencyParameters)
    {
        if (radPerSec == 0f) return 0f;
        float advanceRatio = AdvanceRatio(radPerSec, tas);
        float AR = advanceRatio / lowestEfficientAdvanceRatio;

        float value = (efficiencyParameters.y - AR) / (AR * AR + efficiencyParameters.x);
        value = Mathf.Pow(Mathf.Abs(value), cubicRoot) * Mathf.Sign(value);

        return value / (radPerSec * Radius) / lowestEfficientAdvanceRatio;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(P2))]
public class P2Editor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //
        P2 propeller = (P2)target;

        base.OnInspectorGUI();

        if (GUILayout.Button("Compute"))
        {
            propeller.UpdateValues();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(propeller);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif