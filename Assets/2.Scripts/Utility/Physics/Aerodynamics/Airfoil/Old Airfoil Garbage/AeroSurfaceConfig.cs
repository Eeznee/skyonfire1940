using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[CreateAssetMenu(fileName = "New Aerodynamic Surface Config", menuName = "SOF/Deprecated/Aero Surface Config")]
public class AeroSurfaceConfig : ScriptableObject
{
    public float liftSlope = 6.28f;
    public float skinFriction = 0.02f;
    public float zeroAoA = 0;
    public float maxAoA = 15 * Mathf.Deg2Rad;
    public float minAoA = -15 * Mathf.Deg2Rad;
    public float chord = 1;
    public float flapFraction = 0;
    public float span = 1;
    public bool autoAspectRatio = true;
    public float aspectRatio = 2;
    public float flappyAngle = 0f;

    public AnimationCurve clPlot;
    public AnimationCurve cdPlot;

    [HideInInspector] public float correctedLiftSlope;
    [HideInInspector] public float flapEffectiveness;
    [HideInInspector] public float clMaxFaction;

    public void OnValidate()
    {
        //if (flapFraction > 0.4f)
            //flapFraction = 0.4f;
        if (flapFraction < 0)
            flapFraction = 0;

        if (maxAoA < 0) maxAoA = 0;
        if (minAoA > 0) minAoA = 0;

        if (chord < 1e-3f)
            chord = 1e-3f;

        chord = 1f;
        span = 5f;
        aspectRatio = span / chord;

        // Accounting for aspect ratio effect on lift coefficient.
        correctedLiftSlope = liftSlope * aspectRatio / (aspectRatio + 2 * (aspectRatio + 4) / (aspectRatio + 2));

        float theta = Mathf.Acos(2 * flapFraction - 1);
        flapEffectiveness = 1 - (theta - Mathf.Sin(theta)) / Mathf.PI;

        clMaxFaction = Mathf.Clamp01(1 - 0.5f * (flapFraction - 0.1f) / 0.3f);

        SendToCurve();
    }

    // Calculating flap deflection influence on zero lift angle of attack
    // and angles at which stall happens.
    public Vector3 FlapEffect(float flapAngle)
    {
        float deltaLift = correctedLiftSlope * flapEffectiveness * flapAngle;
        deltaLift *= Mathf.Lerp(0.8f, 0.4f, (Mathf.Abs(flapAngle) * Mathf.Rad2Deg - 10) / 50);  //correction
        float trueZeroLiftAoA = zeroAoA - deltaLift / correctedLiftSlope;

        float clMaxHigh = correctedLiftSlope * (maxAoA - trueZeroLiftAoA) + deltaLift * clMaxFaction;
        float clMaxLow = correctedLiftSlope * (minAoA - trueZeroLiftAoA) + deltaLift * clMaxFaction;

        float trueStallAngleHigh = trueZeroLiftAoA + clMaxHigh / correctedLiftSlope;
        float trueStallAngleLow = trueZeroLiftAoA + clMaxLow / correctedLiftSlope;

        return new Vector3(trueZeroLiftAoA, trueStallAngleHigh, trueStallAngleLow);
    }
    private Vector3 CalculateCoefficients(float angleOfAttack, float flapAngle, float zeroLiftAoA, float stallAngleHigh, float stallAngleLow)
    {
        Vector3 aerodynamicCoefficients;

        // Low angles of attack mode and stall mode curves are stitched together by a line segment. 
        float paddingAngleHigh = Mathf.Deg2Rad * Mathf.Lerp(15, 5, (Mathf.Rad2Deg * flapAngle + 50) / 100);
        float paddingAngleLow = Mathf.Deg2Rad * Mathf.Lerp(15, 5, (-Mathf.Rad2Deg * flapAngle + 50) / 100);
        float paddedStallAngleHigh = stallAngleHigh + paddingAngleHigh;
        float paddedStallAngleLow = stallAngleLow - paddingAngleLow;

        //Low AoA
        if (angleOfAttack <= stallAngleHigh && angleOfAttack > stallAngleLow)
            aerodynamicCoefficients = CalculateCoefficientsAtLowAoA(angleOfAttack, flapAngle, zeroLiftAoA);

        //Beyond Stall
        else if (angleOfAttack > paddedStallAngleHigh || angleOfAttack < paddedStallAngleLow)
            aerodynamicCoefficients = CalculateCoefficientsAtStall(angleOfAttack, flapAngle, zeroLiftAoA, stallAngleHigh, stallAngleLow);

        // Linear stitching in-between stall and low angles of attack modes.
        else
        {
            if (angleOfAttack > stallAngleHigh) //High
            {
                Vector3 aerodynamicCoefficientsLow = CalculateCoefficientsAtLowAoA(stallAngleHigh, flapAngle, zeroLiftAoA);
                Vector3 aerodynamicCoefficientsStall = CalculateCoefficientsAtStall(paddedStallAngleHigh, flapAngle, zeroLiftAoA, stallAngleHigh, stallAngleLow);
                float lerpParam = (angleOfAttack - stallAngleHigh) / (paddedStallAngleHigh - stallAngleHigh);
                aerodynamicCoefficients = Vector3.Lerp(aerodynamicCoefficientsLow, aerodynamicCoefficientsStall, lerpParam);
            }
            else //Low
            {
                Vector3 aerodynamicCoefficientsLow = CalculateCoefficientsAtLowAoA(stallAngleLow, flapAngle, zeroLiftAoA);
                Vector3 aerodynamicCoefficientsStall = CalculateCoefficientsAtStall(paddedStallAngleLow, flapAngle, zeroLiftAoA, stallAngleHigh, stallAngleLow);
                float lerpParam = (angleOfAttack - stallAngleLow) / (paddedStallAngleLow - stallAngleLow);
                aerodynamicCoefficients = Vector3.Lerp(aerodynamicCoefficientsLow, aerodynamicCoefficientsStall, lerpParam);
            }

        }
        return aerodynamicCoefficients;
    }
    private Vector3 CalculateCoefficientsAtLowAoA(float aoa, float flapAngle, float zeroLiftAoA)
    {
        float cl = correctedLiftSlope * (aoa - zeroLiftAoA);
        float inducedAngle = cl / (Mathf.PI * aspectRatio);
        float effectiveAngle = aoa - zeroLiftAoA - inducedAngle;

        float tangentialCoefficient = skinFriction * Mathf.Cos(effectiveAngle);

        float normalCoefficient = (cl + Mathf.Sin(effectiveAngle) * tangentialCoefficient) / Mathf.Cos(effectiveAngle);
        float cd = normalCoefficient * Mathf.Sin(effectiveAngle) + tangentialCoefficient * Mathf.Cos(effectiveAngle);
        float cw = -normalCoefficient * TorqCoefficientProportion(effectiveAngle);

        return new Vector3(cl, cd, cw);
    }
    private Vector3 CalculateCoefficientsAtStall(float aoa, float flapAngle, float zeroLiftAoA, float stallAngleHigh, float stallAngleLow)
    {
        float liftCoefficientLowAoA = correctedLiftSlope * ((aoa > stallAngleHigh ? stallAngleHigh : stallAngleLow) - zeroLiftAoA);
        float inducedAngle = liftCoefficientLowAoA / (Mathf.PI * aspectRatio);

        float lerpParam;
        if (aoa > stallAngleHigh)
            lerpParam = (Mathf.PI / 2 - Mathf.Clamp(aoa, -Mathf.PI / 2, Mathf.PI / 2)) / (Mathf.PI / 2 - stallAngleHigh);
        else
            lerpParam = (-Mathf.PI / 2 - Mathf.Clamp(aoa, -Mathf.PI / 2, Mathf.PI / 2)) / (-Mathf.PI / 2 - stallAngleLow);
        inducedAngle = Mathf.Lerp(0, inducedAngle, lerpParam);
        float effectiveAngle = aoa - zeroLiftAoA - inducedAngle;

        float normalCoefficient = FrictionAt90Degrees(flapAngle) * Mathf.Sin(effectiveAngle) *
            (1 / (0.56f + 0.44f * Mathf.Abs(Mathf.Sin(effectiveAngle))) -
            0.41f * (1 - Mathf.Exp(-17 / aspectRatio)));
        float tangentialCoefficient = 0.5f * skinFriction * Mathf.Cos(effectiveAngle);

        float cl = normalCoefficient * Mathf.Cos(effectiveAngle) - tangentialCoefficient * Mathf.Sin(effectiveAngle);
        float cd = normalCoefficient * Mathf.Sin(effectiveAngle) + tangentialCoefficient * Mathf.Cos(effectiveAngle);
        float cw = -normalCoefficient * TorqCoefficientProportion(effectiveAngle);

        return new Vector3(cl, cd, cw);
    }
    private float TorqCoefficientProportion(float effectiveAngle)
    {
        return 0.25f - 0.175f * (1 - 2 * Mathf.Abs(effectiveAngle) / Mathf.PI);
    }
    private float FrictionAt90Degrees(float flapAngle)
    {
        return 1.98f - 4.26e-2f * flapAngle * flapAngle + 2.1e-1f * flapAngle;
    }
    public void SendToCurve()
    {
        clPlot = AnimationCurve.Constant(-180f, 180f, 0f);
        cdPlot = AnimationCurve.Constant(-180f, 180f, 0f);
        for (float i = -180f; i < 180f; i+= 0.5f)
        {
            Vector3 flapEffect = FlapEffect(flappyAngle);
            Vector3 coefficients = CalculateCoefficients(i * Mathf.Deg2Rad, flappyAngle, flapEffect.x, flapEffect.y, flapEffect.z);
            clPlot.AddKey(new Keyframe(i,coefficients.x));
            cdPlot.AddKey(new Keyframe(i, coefficients.y));
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AeroSurfaceConfig))]
public class AeroSurfaceConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        AeroSurfaceConfig foil = (AeroSurfaceConfig)target;
        GUI.color = GUI.backgroundColor;

        foil.liftSlope = EditorGUILayout.FloatField("Lift Slope", foil.liftSlope);
        foil.skinFriction = EditorGUILayout.FloatField("Skin Friction", foil.skinFriction);
        foil.zeroAoA = EditorGUILayout.FloatField("Null Lift AoA", foil.zeroAoA * Mathf.Rad2Deg) * Mathf.Deg2Rad;
        foil.maxAoA = EditorGUILayout.FloatField("Stall AoA High", foil.maxAoA * Mathf.Rad2Deg) * Mathf.Deg2Rad;
        foil.minAoA = EditorGUILayout.FloatField("Stall AoA Low", foil.minAoA * Mathf.Rad2Deg) * Mathf.Deg2Rad;
        foil.flapFraction = EditorGUILayout.Slider("Flap Fraction", foil.flapFraction, 0f, 1f);
        foil.flappyAngle = EditorGUILayout.Slider("Flap Angle", foil.flappyAngle * Mathf.Rad2Deg, -90f, 90f) * Mathf.Deg2Rad;
        foil.clPlot = EditorGUILayout.CurveField("Lift Curve", foil.clPlot);
        foil.cdPlot = EditorGUILayout.CurveField("Drag Curve", foil.cdPlot);

        if (GUILayout.Button("Send To Curves"))
        {
            foil.OnValidate();
        }


        if (GUI.changed)
        {
            EditorUtility.SetDirty(foil);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif

public class AirfoilSimulator
{
    AeroSurfaceConfig config;

}
