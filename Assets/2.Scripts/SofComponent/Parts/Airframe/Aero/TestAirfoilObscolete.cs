using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[CreateAssetMenu(fileName = "New Airfoil Section", menuName = "Airfoil Section")]
public class TestAirfoilObscolete : ScriptableObject
{
    public struct SpecificVariables
    {
        //Airfoil Shape Affected
        public float correctedLiftSlope;
        public float flapEffectiveness;
        public float clMaxFraction;

        //Flap Angle Affected
        public float zeroLiftAoA;
        public float stallAngleHigh;
        public float stallAngleLow;
        public float paddedStallAngleHigh;
        public float paddedStallAngleLow;

        //Used to check changes in shape and flap angle
        private TestAirfoilObscolete section;
        public float ar;
        private float flapFraction;
        public float flapAngle;

        public void Compute(bool computeAll, TestAirfoilObscolete _section, float _ar, float _flapFraction, float _flapAngle)
        {
            //Recomputed at start and when airfoil structure changes
            if (computeAll || section != _section || ar != _ar || flapFraction != _flapFraction)
            {
                section = _section;
                ar = _ar;
                flapFraction = _flapFraction;

                // Accounting for aspect ratio effect on lift coefficient.
                correctedLiftSlope = section.liftSlope * ar / (ar + 2 * (ar + 4) / (ar + 2));

                float theta = Mathf.Acos(2 * flapFraction - 1);
                flapEffectiveness = 1 - (theta - Mathf.Sin(theta)) / Mathf.PI;

                clMaxFraction = Mathf.Clamp01(1 - 0.5f * (flapFraction - 0.1f) / 0.3f);
            }

            //Recomputed when flap angle changes
            if (computeAll || flapAngle != _flapAngle)
            {
                flapAngle = _flapAngle;

                float deltaLift = correctedLiftSlope * flapEffectiveness * flapAngle;
                deltaLift *= Mathf.Lerp(0.8f, 0.4f, (Mathf.Abs(flapAngle) * Mathf.Rad2Deg - 10) / 50);  //correction
                zeroLiftAoA = section.zeroAoA - deltaLift / correctedLiftSlope;

                float clMaxHigh = correctedLiftSlope * (section.maxAoA - zeroLiftAoA) + deltaLift * clMaxFraction;
                float clMaxLow = correctedLiftSlope * (section.minAoA - zeroLiftAoA) + deltaLift * clMaxFraction;

                stallAngleHigh = zeroLiftAoA + clMaxHigh / correctedLiftSlope;
                stallAngleLow = zeroLiftAoA + clMaxLow / correctedLiftSlope;

                // Low angles of attack mode and stall mode curves are stitched together by a line segment. 
                paddedStallAngleHigh = stallAngleHigh + Mathf.Deg2Rad * Mathf.Lerp(15, 5, (Mathf.Rad2Deg * flapAngle + 50) / 100);
                paddedStallAngleLow = stallAngleLow - Mathf.Deg2Rad * Mathf.Lerp(15, 5, (-Mathf.Rad2Deg * flapAngle + 50) / 100);
            }
        }
    }
    public float liftSlope = 6.28f;
    public float skinFriction = 0.02f;
    public float zeroAoA = 0;
    public float maxAoA = 15 * Mathf.Deg2Rad;
    public float minAoA = -15 * Mathf.Deg2Rad;
    public SpecificVariables v;
    public float ar;
    public float flapFraction;
    public float flapAngle;

    public AnimationCurve clPlot;
    public AnimationCurve cdPlot;
    public AnimationCurve testPlot;


    public Vector3 Coefficients(float angleOfAttack, SpecificVariables v)
    {
        //Low AoA
        if (angleOfAttack <= v.stallAngleHigh && angleOfAttack >= v.stallAngleLow)
            return CoefficientsLow(angleOfAttack, v);
        //Beyond Stall
        if (angleOfAttack > v.paddedStallAngleHigh || angleOfAttack < v.paddedStallAngleLow)
            return CoefficientsStall(angleOfAttack, v);

        // Linear stitching in-between stall and low angles of attack modes.
        bool high = angleOfAttack > v.stallAngleHigh;
        Vector3 low = CoefficientsLow(high ? v.stallAngleHigh : v.stallAngleLow, v);
        Vector3 stall = CoefficientsStall(high ? v.paddedStallAngleHigh : v.paddedStallAngleLow, v);
        if (high)
            return Vector3.Lerp(low, stall, (angleOfAttack - v.stallAngleHigh) / (v.paddedStallAngleHigh - v.stallAngleHigh));
        else
            return Vector3.Lerp(low, stall, (angleOfAttack - v.stallAngleLow) / (v.paddedStallAngleLow - v.stallAngleLow));
    }
    private Vector3 CoefficientsLow(float aoa, SpecificVariables v)
    {
        float cl = v.correctedLiftSlope * (aoa - v.zeroLiftAoA);
        float inducedAngle = cl / (Mathf.PI * v.ar);
        float effectiveAngle = aoa - v.zeroLiftAoA - inducedAngle;

        float tangentialCoefficient = skinFriction * Mathf.Cos(effectiveAngle);

        float normalCoefficient = (cl + Mathf.Sin(effectiveAngle) * tangentialCoefficient) / Mathf.Cos(effectiveAngle);
        float cd = normalCoefficient * Mathf.Sin(effectiveAngle) + tangentialCoefficient * Mathf.Cos(effectiveAngle);
        float cw = -normalCoefficient * TorqCoefficientProportion(effectiveAngle);

        return new Vector3(cl, cd, cw);
    }
    private Vector3 CoefficientsStall(float aoa, SpecificVariables v)
    {
        float liftCoefficientLowAoA = v.correctedLiftSlope * ((aoa > v.stallAngleHigh ? v.stallAngleHigh : v.stallAngleLow) - v.zeroLiftAoA);
        float inducedAngle = liftCoefficientLowAoA / (Mathf.PI * v.ar);

        float lerpParam;
        if (aoa > v.stallAngleHigh)
            lerpParam = (Mathf.PI / 2 - Mathf.Clamp(aoa, -Mathf.PI / 2, Mathf.PI / 2)) / (Mathf.PI / 2 - v.stallAngleHigh);
        else
            lerpParam = (-Mathf.PI / 2 - Mathf.Clamp(aoa, -Mathf.PI / 2, Mathf.PI / 2)) / (-Mathf.PI / 2 - v.stallAngleLow);
        inducedAngle = Mathf.Lerp(0, inducedAngle, lerpParam);
        float effectiveAngle = aoa - v.zeroLiftAoA - inducedAngle;

        float normalCoefficient = FrictionAt90Degrees(v.flapAngle) * Mathf.Sin(effectiveAngle) *
            (1 / (0.56f + 0.44f * Mathf.Abs(Mathf.Sin(effectiveAngle))) -
            0.41f * (1 - Mathf.Exp(-17 / v.ar)));
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
        testPlot = AnimationCurve.Constant(-180f, 180f, 0f);
        for (float i = -180f; i < 180f; i += 0.5f)
        {
            v.Compute(true,this,ar, flapFraction, flapAngle);
            Vector3 coefficients = Coefficients(i * Mathf.Deg2Rad, v);
            clPlot.AddKey(new Keyframe(i, coefficients.x));
            cdPlot.AddKey(new Keyframe(i, coefficients.y));
            v.Compute(true, this, ar, flapFraction, 0f);
            coefficients = Coefficients(i * Mathf.Deg2Rad, v);
            testPlot.AddKey(new Keyframe(i, clPlot.Evaluate(i) - coefficients.x));
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TestAirfoilObscolete))]
public class AirfoilSectionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        TestAirfoilObscolete foil = (TestAirfoilObscolete)target;
        GUI.color = GUI.backgroundColor;

        foil.liftSlope = EditorGUILayout.FloatField("Lift Slope", foil.liftSlope);
        foil.skinFriction = EditorGUILayout.FloatField("Skin Friction", foil.skinFriction);
        foil.zeroAoA = EditorGUILayout.FloatField("Null Lift AoA", foil.zeroAoA * Mathf.Rad2Deg) * Mathf.Deg2Rad;
        foil.maxAoA = Mathf.Abs(EditorGUILayout.FloatField("Stall AoA High", foil.maxAoA * Mathf.Rad2Deg) * Mathf.Deg2Rad);
        foil.minAoA = -Mathf.Abs(EditorGUILayout.FloatField("Stall AoA Low", foil.minAoA * Mathf.Rad2Deg) * Mathf.Deg2Rad);
        foil.clPlot = EditorGUILayout.CurveField("Lift Curve", foil.clPlot);
        foil.cdPlot = EditorGUILayout.CurveField("Drag Curve", foil.cdPlot);
        foil.testPlot = EditorGUILayout.CurveField("Test Plot", foil.testPlot);
        foil.ar = EditorGUILayout.FloatField("Aspect Ratio", foil.ar);
        foil.flapFraction = EditorGUILayout.FloatField("Flap Fraction", foil.flapFraction);
        foil.flapAngle = EditorGUILayout.FloatField("Flap Angle", foil.flapAngle * Mathf.Rad2Deg) * Mathf.Deg2Rad;

        if (GUILayout.Button("Big Button"))
        {
            foil.SendToCurve();
        }


        if (GUI.changed)
        {
            EditorUtility.SetDirty(foil);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
