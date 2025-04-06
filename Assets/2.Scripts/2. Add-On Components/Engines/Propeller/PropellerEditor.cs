using UnityEngine;
using System.Runtime.CompilerServices;
using System;


#if UNITY_EDITOR
using UnityEditor;
using Syrus.Plugins.ChartEditor;


public partial class Propeller : SofModule, IMassComponent, IAircraftForce
{
    public float minPitchSpeed { get; set; }
    public float maxPitchSpeed { get; set; }

    public Vector2[] efficiencyPlot { get; set; }
    public Vector2[] thrustRatioPlot { get; set; }

    private void OnValidate()
    {
        UpdateStats();
    }
    public void UpdateStats()
    {
        if (engine == null || engine.Preset == null)
        {
            efficiencyPlot = new Vector2[0];
            thrustRatioPlot = new Vector2[0];
            return;
        }
        float propellerRps = engine.Preset.NominalRadPerSec * ReductionGear;

        float minPitchAdvanceRatio = AdvanceRatio(minPitch);
        float maxPitchAdvanceRatio = AdvanceRatio(maxPitch);

        minPitchSpeed = AirSpeed(minPitchAdvanceRatio, propellerRps);
        maxPitchSpeed = AirSpeed(maxPitchAdvanceRatio, propellerRps);

#if UNITY_EDITOR
        int points = 100;
        float maxSpeed = 800f;
        float increment = maxSpeed / points;


        float maxPowerToThrust = TheoricalPowerToThrust(propellerRps, maxSpeed) * efficiency;

        efficiencyPlot = new Vector2[points];
        thrustRatioPlot = new Vector2[points];

        for (int i = 0; i < points; i++)
        {
            float rps = propellerRps;

            float speedKph = i * increment;
            float speedMps = speedKph / 3.6f;

            float powerToThrust = TheoricalPowerToThrust(propellerRps, speedMps) * efficiency;
            float eff = powerToThrust * speedMps;

            efficiencyPlot[i] = new Vector2(speedKph, eff);
            thrustRatioPlot[i] = new Vector2(speedKph, powerToThrust * 20f);
        }
#endif
    }

    private void OnDrawGizmos()
    {
        Handles.color = Color.yellow;
        Handles.DrawWireDisc(transform.position, transform.forward, radius);
    }
}

[CustomEditor(typeof(Propeller))]
public class PropellerEditor : SofComponentEditor
{
    SerializedProperty radius;
    SerializedProperty efficiency;
    SerializedProperty invertRotation;

    SerializedProperty pitchControl;
    SerializedProperty minPitch;
    SerializedProperty maxPitch;

    protected override void OnEnable()
    {
        base.OnEnable();
        radius = serializedObject.FindProperty("radius");
        efficiency = serializedObject.FindProperty("efficiency");
        invertRotation = serializedObject.FindProperty("invertRotation");

        pitchControl = serializedObject.FindProperty("pitchControl");
        minPitch = serializedObject.FindProperty("minPitch");
        maxPitch = serializedObject.FindProperty("maxPitch");

        Propeller propeller = (Propeller)target;
        propeller.UpdateStats();
    }
    static bool showMain = true;
    static bool showBladePitch = true;
    static bool showStats = true;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Propeller propeller = (Propeller)target;

        serializedObject.Update();

        showMain = EditorGUILayout.Foldout(showMain, "Main", true, EditorStyles.foldoutHeader);
        if (showMain)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(radius);
            EditorGUILayout.PropertyField(invertRotation);
            EditorGUILayout.Slider(efficiency, 0.6f, 1f);

            EditorGUI.indentLevel--;
        }

        showBladePitch = EditorGUILayout.Foldout(showBladePitch, "Blade & Pitch", true, EditorStyles.foldoutHeader);
        if (showBladePitch)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(pitchControl);

            switch (propeller.PitchControlMechanism)
            {
                case Propeller.PitchControl.Fixed:
                    EditorGUILayout.Slider(maxPitch, 0f, 80f, new GUIContent("Pitch Angle"));
                    break;
                case Propeller.PitchControl.TwoPitch:

                    EditorGUILayout.Slider(minPitch, 0f, 80f, new GUIContent("Pitch Angle 1"));
                    EditorGUILayout.Slider(maxPitch, 0f, 80f, new GUIContent("Pitch Angle 2"));
                    break;
                case Propeller.PitchControl.ConstantSpeed:
                    EditorGUILayout.Slider(minPitch, 0f, 90f, new GUIContent("Min Pitch Angle"));
                    EditorGUILayout.Slider(maxPitch, 0f, 90f, new GUIContent("Max Pitch Angle"));
                    break;
            }

            if(minPitch.floatValue > maxPitch.floatValue)
            {
                minPitch.floatValue = maxPitch.floatValue;
            }

            EditorGUI.indentLevel--;
        }

        showStats = EditorGUILayout.Foldout(showStats, "Stats", true, EditorStyles.foldoutHeader);
        if (showStats)
        {
            EditorGUI.indentLevel++;

            ShowGraph(propeller);

            switch (propeller.PitchControlMechanism)
            {
                case Propeller.PitchControl.Fixed:
                    EditorGUILayout.LabelField("Effective Speed", (propeller.maxPitchSpeed * 3.6f).ToString("0") + " km/h");
                    break;
                case Propeller.PitchControl.TwoPitch:
                    EditorGUILayout.LabelField("Effective Speed 1", (propeller.minPitchSpeed * 3.6f).ToString("0") + " km/h");
                    EditorGUILayout.LabelField("Effective Speed 2", (propeller.maxPitchSpeed * 3.6f).ToString("0") + " km/h");
                    EditorGUILayout.LabelField("Prop Switch", (propeller.TwoPitchAdvanceRatioTrigger() * 3.6f).ToString("0") + " km/h");
                    break;
                case Propeller.PitchControl.ConstantSpeed:
                    EditorGUILayout.LabelField("Min Effective Speed", (propeller.minPitchSpeed * 3.6f).ToString("0") + " km/h");
                    EditorGUILayout.LabelField("Max Effective Speed", (propeller.maxPitchSpeed * 3.6f).ToString("0") + " km/h");
                    break;
            }

            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }


    void ShowGraph(Propeller propeller)
    {
        Color transparent = Color.white;
        transparent.a = 0f;
        Color lightGray = Color.gray;
        lightGray.a = 0.2f;

        GUIChartEditor.BeginChart(10f, 150f, 100f, 450f, transparent,
            //GUIChartEditorOptions.ShowLabels("0", Color.white, engine.GraphLabels()),
            GUIChartEditorOptions.ShowAltPeriod(),
            GUIChartEditorOptions.ShowAxes(Color.white),
            GUIChartEditorOptions.ShowGrid(50f, 0.1f, lightGray, Color.white, true),
            GUIChartEditorOptions.ChartBounds(0f, 800f, -1.2f, 1.2f),
            GUIChartEditorOptions.SetOrigin(ChartOrigins.BottomLeft));


        GUIChartEditor.PushLineChart(propeller.efficiencyPlot, Color.red);
        GUIChartEditor.PushLineChart(propeller.thrustRatioPlot, Color.green);

        GUIChartEditor.EndChart();
    }
}
#endif
