using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Syrus.Plugins.ChartEditor;

public partial class PistonEnginePreset : EnginePreset
{
    public struct PistonEngineStats
    {
        public float powerHp;
        public float altitude;

        public float boostedPowerHp;
        public float boostedAltitude;
    }

    public Vector2[] continuousPowerPlotsKm;
    public Vector2[] boostedPowerPlotsKm;
    public Vector2[] takeOffBoostPowerPlotsKm;


    public static float maxGraphAltitude = 8000f;

    public const float powerInterval = 200f;
    public const float labelXOffset = 0.2f;
    public const float labelYOffset = 20f;


    const int takeOffPlots = 12;
    public void GraphUpdate(float maxAltitude, int points)
    {
        continuousPowerPlotsKm = new Vector2[points];
        boostedPowerPlotsKm = new Vector2[points];
        takeOffBoostPowerPlotsKm = new Vector2[takeOffPlots];

        float increment = maxAltitude / points;

        for (int i = 0; i < points; i++)
        {
            float altitude = i * maxAltitude / points;
            float power = BestPower(altitude, EngineRunMode.Continuous) * wattsToHp;
            float boostedPower = BestPower(altitude, EngineRunMode.Boost) * wattsToHp;
            float takeOffPower = BestPower(altitude, EngineRunMode.TakeOffBoost) * wattsToHp;

            continuousPowerPlotsKm[i] = new Vector2(altitude * 0.001f, power);
            boostedPowerPlotsKm[i] = new Vector2(altitude * 0.001f, boostedPower);

            if (i < takeOffPlots - 1) takeOffBoostPowerPlotsKm[i] = new Vector2(altitude * 0.001f, takeOffPower);
            if (i == takeOffPlots - 1) takeOffBoostPowerPlotsKm[i] = new Vector2(altitude * 0.001f, Mathf.Max(boostedPower, power));
        }
    }
    public bool CheckPowerSettingsValidity()
    {
        return true;
    }

    public float GraphMaxPower()
    {
        PistonEngineStats[] stats = ImportantStats();

        float highestPower = 0f;

        for (int i = 0; i < stats.Length; i++)
        {
            if (stats[i].powerHp > highestPower) highestPower = stats[i].powerHp;
            if (stats[i].boostedPowerHp > highestPower) highestPower = stats[i].boostedPowerHp;
        }

        int highestPowerRounded = Mathf.CeilToInt(highestPower / powerInterval);

        return highestPowerRounded * powerInterval + powerInterval * 0.05f;
    }
    public PistonEngineStats[] ImportantStats()
    {
        PistonEngineStats[] stats = new PistonEngineStats[powerSettings.Length + 1];

        stats[0].powerHp = BestPower(0f, EngineRunMode.Continuous) * wattsToHp;

        if (HasTakeOffBoost) stats[0].boostedPowerHp = BestPower(0f, EngineRunMode.TakeOffBoost) * wattsToHp;
        else if (HasCombatBoost) stats[0].boostedPowerHp = BestPower(0f, EngineRunMode.Boost) * wattsToHp;

        stats[0].altitude = stats[0].boostedAltitude = 0f;

        for (int i = 1; i <= powerSettings.Length; i++)
        {
            stats[i].powerHp = powerSettings[i - 1].power;
            stats[i].altitude = powerSettings[i - 1].criticalAltitude;


            float boostedAltitude = OptimumAltitude(i -1 , EngineRunMode.Boost);
            float boostedPowerHp = Power(i-1, EngineRunMode.Boost, boostedAltitude, CombatBoostRadPerSec) * wattsToHp;
            stats[i].boostedPowerHp = HasCombatBoost ? boostedPowerHp : 0f;
            stats[i].boostedAltitude = boostedAltitude;
        }

        return stats;
    }
    public float[] GraphLabels()
    {
        float[] poi = new float[6 * powerSettings.Length];

        for (int i = 0; i < powerSettings.Length; i++)
        {
            int valueIndex = i * 6;
            int xCoordIndex = i * 6 + 1;
            int yCoordIndex = i * 6 + 2;
            
            poi[valueIndex] = powerSettings[i].power;
            poi[yCoordIndex] = powerSettings[i].power + labelYOffset;
            poi[xCoordIndex] = powerSettings[i].criticalAltitude * 0.001f + labelXOffset;

            //boosted
            float altitude = OptimumAltitude(i, EngineRunMode.Boost);
            float boostedPowerHp = Power(i, EngineRunMode.Boost, altitude, CombatBoostRadPerSec) * wattsToHp;
            altitude = Mathf.Clamp(altitude, 0f, 20000f);
            boostedPowerHp = Mathf.Clamp(boostedPowerHp, 0f, 1500f);
            if (altitude > 0f && HasCombatBoost)
            {
                poi[valueIndex + 3] = boostedPowerHp;
                poi[yCoordIndex + 3] = boostedPowerHp + labelYOffset;
                poi[xCoordIndex + 3] = altitude * 0.001f + labelXOffset;
            }
            else
            {
                poi[valueIndex + 3] = poi[valueIndex];
                poi[yCoordIndex + 3] = poi[yCoordIndex];
                poi[xCoordIndex + 3] = poi[xCoordIndex];
            }
        }

        return poi;
    }
    public string ErrorCheck()
    {
        if (powerSettings == null || powerSettings.Length == 0) return "You must have at least one power setting";

        //if (superChargingType == SuperChargingType.Turbocharger && powerSettings.Length != 1) 
        //    return "When using a turbocharger, the engine must have only 1 power setting";

        if (superChargingType == SuperchargingMechanism.VariableSupercharger && powerSettings.Length != 2)
            return "When using a variable supercharger, the engine must have 2 power settings";

        if (CombatBoostMP < ContinuousMP && HasCombatBoost)
            return "Boosted Manifold Pressure must be higher than continuous Manifold Pressure";

        if (TakeOffBoostMP < ContinuousMP && HasTakeOffBoost)
            return "Take Off Boost Manifold Pressure must be higher continuous regular Manifold Pressure";

        float trackAltitude = -1f;
        for (int i = 0; i < powerSettings.Length; i++)
        {
            if (powerSettings[i].criticalAltitude < trackAltitude)
                return "Power settings must have ascending altitude";
            trackAltitude = powerSettings[i].criticalAltitude;
        }

        return "";
    }
    public void OnValidate()
    {
        if (powerSettings == null) powerSettings = new SuperchargerPowerSettings[1];
        GraphUpdate(12000f, 200);
    }
}


[CustomEditor(typeof(PistonEnginePreset))]
public class PistonEnginePresetEditor : EnginePresetEditor
{
    SerializedProperty powerSettings;
    SerializedProperty superChargingType;
    SerializedProperty continuousMP;
    SerializedProperty throttlingLossPerKm;
    SerializedProperty rpmEfficiencyDelta;
    SerializedProperty adjustedRpmAltitudeDetla;

    SerializedProperty combatBoost;
    SerializedProperty combatBoostRpm;
    SerializedProperty combatBoostMP;
    SerializedProperty combatBoostMaxTime;

    SerializedProperty takeOffBoost;
    SerializedProperty takeOffBoostRpm;
    SerializedProperty takeOffBoostMP;

    SerializedProperty propellerReductionGear;
    SerializedProperty fuelConsumption;


    protected override void OnEnable()
    {
        base.OnEnable();

        propellerReductionGear = serializedObject.FindProperty("propellerReductionGear");
        fuelConsumption = serializedObject.FindProperty("fuelConsumption");

        powerSettings = serializedObject.FindProperty("powerSettings");
        superChargingType = serializedObject.FindProperty("superChargingType");
        continuousMP = serializedObject.FindProperty("continuousMP");
        throttlingLossPerKm = serializedObject.FindProperty("throttlingLossPerKm");
        rpmEfficiencyDelta = serializedObject.FindProperty("rpmEfficiencyDelta");
        adjustedRpmAltitudeDetla = serializedObject.FindProperty("adjustedRpmAltitudeDetla");

        combatBoost = serializedObject.FindProperty("combatBoost");
        combatBoostRpm = serializedObject.FindProperty("combatBoostRpm");
        combatBoostMP = serializedObject.FindProperty("combatBoostMP");
        combatBoostMaxTime = serializedObject.FindProperty("combatBoostMaxTime");

        takeOffBoost = serializedObject.FindProperty("takeOffBoost");
        takeOffBoostRpm = serializedObject.FindProperty("takeOffBoostRpm");
        takeOffBoostMP = serializedObject.FindProperty("takeOffBoostMP");
    }
    public override void Main()
    {
        base.Main();

        EditorGUILayout.Slider(propellerReductionGear, 0.1f, 1f,new GUIContent("Prop Reduction Gear"));
        EditorGUILayout.Slider(fuelConsumption, 0.1f, 0.5f, new GUIContent("Fuel Consumption kg/(hp.h)"));
    }

    static bool showStats = false;

    public override void Performances()
    {
        PistonEnginePreset engine = (PistonEnginePreset)target;

        string error = engine.ErrorCheck();
        if (error != "") EditorGUILayout.HelpBox(error, MessageType.Error);
        else EditorGUILayout.HelpBox("Errors will be shown here. No error detected, the engine should be OK", MessageType.Info);

        base.Performances();

        EditorGUILayout.PropertyField(continuousMP, new GUIContent("Manifold Pressure"));

        EditorGUILayout.PropertyField(combatBoost);
        if (engine.HasCombatBoost)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(combatBoostRpm, new GUIContent("RPM"));
            EditorGUILayout.PropertyField(combatBoostMP, new GUIContent("Manifold Pressure"));
            EditorGUILayout.PropertyField(combatBoostMaxTime, new GUIContent("Max Time"));

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.PropertyField(takeOffBoost);
        if (engine.HasTakeOffBoost)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(takeOffBoostRpm, new GUIContent("RPM"));
            EditorGUILayout.PropertyField(takeOffBoostMP, new GUIContent("Manifold Pressure"));

            EditorGUI.indentLevel--;
        }

        GUILayout.Space(20);

        EditorGUILayout.PropertyField(superChargingType);
        EditorGUILayout.PropertyField(powerSettings, new GUIContent("Power Settings Continuous"));

        GUILayout.Space(20);

        if (engine.SuperChargingType != SuperchargingMechanism.Turbocharger)
            EditorGUILayout.Slider(throttlingLossPerKm, 0f, 5f, new GUIContent("Throttling loss %/km"));
        if (engine.HasCombatBoost || engine.HasTakeOffBoost)
        {
            EditorGUILayout.Slider(rpmEfficiencyDelta, -0.2f, 1f, new GUIContent("Rpm Efficiency Delta"));
            EditorGUILayout.Slider(adjustedRpmAltitudeDetla, 0f, 1000f, new GUIContent("Adjusted Rpm Altitude Delta"));
        }

        GUILayout.Space(20);

        showStats = EditorGUILayout.Foldout(showStats, "Stats & Graph", true, EditorStyles.foldoutHeader);
        if (showStats)
        {
            EditorGUI.indentLevel++;

            PistonEnginePreset.PistonEngineStats[] stats = engine.ImportantStats();

            EditorGUILayout.LabelField("At sea level");
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Continuous", stats[0].powerHp.ToString("0") + " hp");
            if (engine.HasTakeOffBoost) EditorGUILayout.LabelField("Take Off Boost", stats[0].boostedPowerHp.ToString("0") + " hp");
            else if (engine.HasCombatBoost) EditorGUILayout.LabelField("Boosted", stats[0].boostedPowerHp.ToString("0") + " hp");

            EditorGUI.indentLevel--;

            for (int i = 1; i < stats.Length; i++)
            {
                EditorGUILayout.LabelField("Supercharger Setting " + i);
                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField("Continuous", stats[i].powerHp.ToString("0") + " hp  @  " + stats[i].altitude.ToString("0") + " m");
                if (stats[i].boostedAltitude > 0f && engine.HasCombatBoost)
                    EditorGUILayout.LabelField("Boosted", stats[i].boostedPowerHp.ToString("0") + " hp  @  " + stats[i].boostedAltitude.ToString("0") + " m");

                EditorGUI.indentLevel--;

            }
            EditorGUI.indentLevel--;

            GUILayout.Space(20f);

            PistonEnginePreset.maxGraphAltitude = EditorGUILayout.Slider(PistonEnginePreset.maxGraphAltitude, 5000f, 15000f);
            EditorGUILayout.LabelField(new GUIContent("HORSEPOWER VS ALTITUDE (km)"));

            GUILayout.Space(10f);

            ShowGraph(engine);
        }
    }
    void ShowGraph(PistonEnginePreset engine)
    {
        Color transparent = Color.white;
        transparent.a = 0f;
        Color lightGray = Color.gray;
        lightGray.a = 0.2f;

        GUIChartEditor.BeginChart(10f, 150f, 100f, 450f, transparent,
            GUIChartEditorOptions.ShowLabels("0", Color.white, engine.GraphLabels()),
            GUIChartEditorOptions.ShowAltPeriod(),
            GUIChartEditorOptions.ShowAxes(Color.white),
            GUIChartEditorOptions.ShowGrid(1f, PistonEnginePreset.powerInterval, lightGray, Color.white, true),
            GUIChartEditorOptions.ChartBounds(-0.6f, PistonEnginePreset.maxGraphAltitude * 0.001f + 0.1f, -50f, engine.GraphMaxPower()),
            GUIChartEditorOptions.SetOrigin(ChartOrigins.BottomLeft));


        if (engine.HasCombatBoost)
            GUIChartEditor.PushLineChart(engine.boostedPowerPlotsKm, Color.red);
        if (engine.HasTakeOffBoost)
            GUIChartEditor.PushLineChart(engine.takeOffBoostPowerPlotsKm, Color.magenta);
        GUIChartEditor.PushLineChart(engine.continuousPowerPlotsKm, Color.green);

        GUIChartEditor.EndChart();
    }
}
#endif
