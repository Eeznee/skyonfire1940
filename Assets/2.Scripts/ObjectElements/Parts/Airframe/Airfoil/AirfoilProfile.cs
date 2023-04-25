using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


[CreateAssetMenu(fileName = "New Airfoil Profile", menuName = "Aircraft/Airfoil Profile")]
public class AirfoilProfile : ScriptableObject
{
    public AnimationCurve liftCurve = AnimationCurve.Linear(-180.0f, 0.0f, 180, 0);
    public AnimationCurve dragCurve = AnimationCurve.Linear(-180.0f, 0.0f, 180, 0);
    public AnimationCurve liftCurveFlaps = AnimationCurve.Linear(-180.0f, 0.0f, 180, 0);
    public AnimationCurve dragCurveFlaps = AnimationCurve.Linear(-180.0f, 0.0f, 180, 0);
    public TextAsset performancePlot;

    public enum AirfoilType
    {
        Asymetric,
        Symetric
    }
    public AirfoilType type = AirfoilType.Asymetric;
    public float clZero = 0f;
    public float clMax = 1.2f;
    public float clMin = -1.2f;
    public float maxAngle = 18f;
    public float minAngle = -18f;
    public float cdMin = 0.01f;
    public float flapClZero = 0.5f;
    public float flapClMax = 3f;
    public float flapClMin = 0f;
    public float flapMaxAngle = 25f;
    public float flapMinAngle = -10f;
    public float flapCdMin = 0.1f;

    public TrailRenderer tipTrail;

    public float gradient { get { return (clMax - clZero) / (maxAngle * Mathf.Deg2Rad); } }

    public float Cl(float aoa) { return liftCurve.Evaluate(aoa); }
    public float Cl(float aoa, float flapFactor) { return Mathf.Lerp(liftCurve.Evaluate(aoa), liftCurveFlaps.Evaluate(aoa), flapFactor); }
    public float Cd(float aoa) { return dragCurve.Evaluate(aoa); }
    public float Cd(float aoa, float flapFactor) { return Mathf.Lerp(dragCurve.Evaluate(aoa), dragCurveFlaps.Evaluate(aoa), flapFactor); }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AirfoilProfile))]
public class AirfoilPresetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        AirfoilProfile foil = (AirfoilProfile)target;
        //
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Settings", MessageType.None);
        GUI.color = GUI.backgroundColor;
        foil.type = (AirfoilProfile.AirfoilType)EditorGUILayout.EnumPopup("Airfoil Type", foil.type);

        foil.liftCurve = EditorGUILayout.CurveField("Lift Curve", foil.liftCurve);
        foil.dragCurve = EditorGUILayout.CurveField("Drag Curve", foil.dragCurve);

        if (foil.type == AirfoilProfile.AirfoilType.Asymetric)
        {
            foil.clZero = EditorGUILayout.FloatField("Cl at AoA 0", foil.clZero);
            foil.clMax = EditorGUILayout.FloatField("Maximum Cl", foil.clMax);
            foil.maxAngle = EditorGUILayout.FloatField("At angle", foil.maxAngle);
            foil.clMin = EditorGUILayout.FloatField("Minimum Cl", foil.clMin);
            foil.minAngle = EditorGUILayout.FloatField("At angle", foil.minAngle);
            foil.cdMin = EditorGUILayout.FloatField("Cd Minimum", foil.cdMin);

            foil.liftCurveFlaps = EditorGUILayout.CurveField("Lift Curve Flaps", foil.liftCurveFlaps);
            foil.dragCurveFlaps = EditorGUILayout.CurveField("Drag Curve Flaps", foil.dragCurveFlaps);
            foil.flapClZero = EditorGUILayout.FloatField("Cl at AoA 0 , Flaps", foil.flapClZero);
            foil.flapClMax = EditorGUILayout.FloatField("Maximum Cl , Flaps", foil.flapClMax);
            foil.flapMaxAngle = EditorGUILayout.FloatField("At angle , Flaps", foil.flapMaxAngle);
            foil.flapClMin = EditorGUILayout.FloatField("Minimum Cl , Flaps", foil.flapClMin);
            foil.flapMinAngle = EditorGUILayout.FloatField("At angle , Flaps", foil.flapMinAngle);
            foil.flapCdMin = EditorGUILayout.FloatField("Cd Minimum , Flaps", foil.flapCdMin);
            foil.tipTrail = EditorGUILayout.ObjectField("Wingtip Trail", foil.tipTrail, typeof(TrailRenderer), false) as TrailRenderer;
        }
        else
        {
            foil.clZero = 0f;
            foil.clMax = EditorGUILayout.FloatField("Maximum/Minimum Cl", foil.clMax);
            foil.maxAngle = EditorGUILayout.FloatField("At angle", foil.maxAngle);
            foil.minAngle = -foil.maxAngle;
            foil.clMin = -foil.clMax;
            foil.cdMin = EditorGUILayout.FloatField("Cd Minimum", foil.cdMin);
        }



        if (GUILayout.Button("Apply Simplified Physics"))
        {
            foil.liftCurve = AnimationCurve.Linear(-180.0f, 0f, 180, 0f);
            foil.liftCurve.AddKey(Mathf.Lerp(foil.minAngle, -180f, 0.25f), foil.clMin / 2f);
            foil.liftCurve.AddKey(foil.minAngle, foil.clMin);
            foil.liftCurve.AddKey(0f, foil.clZero);
            foil.liftCurve.AddKey(foil.maxAngle, foil.clMax);
            foil.liftCurve.AddKey(Mathf.Lerp(foil.maxAngle, 180f, 0.25f), foil.clMax / 2f);
            foil.liftCurve.SmoothTangents(2, 0.9f);
            foil.liftCurve.SmoothTangents(4, -0.9f);

            foil.liftCurveFlaps = AnimationCurve.Linear(-180.0f, 0f, 180, 0f);
            foil.liftCurveFlaps.AddKey(Mathf.Lerp(foil.flapMinAngle, -180f, 0.25f), foil.flapClMin / 2f);
            foil.liftCurveFlaps.AddKey(foil.flapMinAngle, foil.flapClMin);
            foil.liftCurveFlaps.AddKey(0f, foil.flapClZero);
            foil.liftCurveFlaps.AddKey(foil.flapMaxAngle, foil.flapClMax);
            foil.liftCurveFlaps.AddKey(Mathf.Lerp(foil.flapMaxAngle, 180f, 0.25f), foil.flapClMax / 2f);
            foil.liftCurveFlaps.SmoothTangents(2, 0.9f);
            foil.liftCurveFlaps.SmoothTangents(4, -0.9f);

            foil.dragCurve = AnimationCurve.Linear(-180.0f, 0.02f, 180, 0.02f);
            foil.dragCurve.AddKey(-90f, 1f);
            foil.dragCurve.AddKey(90f, 1f);
            float zeroLiftPoint = foil.minAngle * foil.clZero / (foil.clZero - foil.clMin);
            foil.dragCurve.AddKey(zeroLiftPoint, foil.cdMin);
            foil.dragCurve.AddKey(foil.minAngle, foil.cdMin * (1f + 0.2f * (Mathf.Abs(zeroLiftPoint) - foil.minAngle)));
            foil.dragCurve.AddKey(foil.maxAngle, foil.cdMin * (1f + 0.2f * (Mathf.Abs(zeroLiftPoint) + foil.maxAngle)));
            //foil.dragCurve.SmoothTangents(2, -0.5f);
            foil.dragCurve.SmoothTangents(3, 0f);
            //foil.dragCurve.SmoothTangents(4, 0.5f);



            foil.dragCurveFlaps = AnimationCurve.Linear(-180.0f, 0.02f, 180, 0.02f);
            foil.dragCurveFlaps.AddKey(-90f, 1f);
            foil.dragCurveFlaps.AddKey(foil.flapMinAngle, foil.flapCdMin * 1.5f);
            foil.dragCurveFlaps.AddKey((foil.flapMaxAngle + foil.flapMinAngle) / 2f, foil.flapCdMin);
            foil.dragCurveFlaps.AddKey(foil.flapMaxAngle, foil.flapCdMin * 1.5f);
            foil.dragCurveFlaps.AddKey(90f, 1f);
            foil.dragCurveFlaps.SmoothTangents(2, -0.5f);
            foil.dragCurveFlaps.SmoothTangents(4, 0.5f);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(foil);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
