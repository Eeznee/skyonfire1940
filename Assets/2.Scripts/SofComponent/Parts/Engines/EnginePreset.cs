using UnityEngine;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Engine", menuName = "SOF/Aircraft Modules/Engine")]
public class EnginePreset : ScriptableObject
{
    public enum Type
    {
        V,
        Radial,
        Inverted,
        Jet
    }
    public enum FuelMixerType
    {
        Carburetor,
        Injection
    }

    //General
    public string designation;
    public string constructor;
    public Type type;
    public FuelMixerType fuelMixer = FuelMixerType.Carburetor;
    public int cylinders = 12;
    public float weight = 500f;

    //Performances
    public float nominalRPS = 314.1592653f;
    public float boostRPS = 314.1592653f;
    public float idleRPS = 31.41592653f;
    public float ignitionTime = 2f;
    public int gears = 1;
    public float boostValue = 1.1f;
    public float boostTime = 300f;
    public float boostBestAltitude = 4500f;
    public float boostMaxAltitude = 5000f;
    public AnimationCurve gear1 = AnimationCurve.Linear(0f, 1000f, 10000f, 1000f);
    public AnimationCurve gear2 = AnimationCurve.Linear(0f, 1000f, 10000f, 1000f);
    public const float minPowEff = 0.3f;
    public const float engineFriction = -300f;

    public float maxThrust;

    //Temperature
    public float waterTemperature = 110f;
    public float oilTemperature = 90f;
    public float tempIdle = 120f;
    public float tempFull = 250f;

    //Fuel
    public float idleConsumption = 0.4f;
    public float halfConsumption = 0.25f;
    public float fullConsumption = 0.3f;

    //Effects
    public ParticleSystem ignitionEffect;
    public ParticleSystem boostEffect;
    public ParticleSystem overHeatEffect;
    public ParticleSystem burningEffect;

    //Audio and effects
    public AudioClip startUpAudio;
    public AudioClip shutDownAudio;
    public AudioClip idleAudioCockpit;
    public AudioClip fullAudioCockpit;
    public AudioClip idleAudioExtSelf;
    public AudioClip fullAudioExtSelf;
    public AudioClip spatialAudio;
    public AudioClip[] enginePops;
    public float fullRps = 1500f;

    public const float burningChance = 0.15f;

    public bool WaterCooled() {return type == Type.V || type == Type.Inverted; }

    public float RpmPowerEffectiveness(float radSec, bool boost)
    {
        float targetRps = boost ? boostRPS : nominalRPS;
        if (radSec < targetRps)
            return Mathf.Lerp(0f, 1f, Mathv.SmoothStart(radSec / targetRps, 2));
        else
            return Mathf.InverseLerp(targetRps * 2f, targetRps, radSec);
    }
    public float TargetRPS(float thr, bool boost)
    {
        if (boost) return boostRPS;
        return Mathf.Lerp(idleRPS,nominalRPS, thr);
    }
    public float ConsumptionRate(float thr, float powerOrThrust)
    {
        if (type != Type.Jet) powerOrThrust /= 745.7f;
        return ConsumptionCoeff(thr) * powerOrThrust;
    }
    public float ConsumptionCoeff(float thr)
    {
        //Lagrange interpolation polynom , idle at 0 , half at 0.5 and full at 1
        float coeff = idleConsumption;
        coeff += thr * (-3f * idleConsumption + 4f * halfConsumption - fullConsumption);
        coeff += 2 * thr * thr * (idleConsumption - 2f * halfConsumption + fullConsumption);
        return coeff;
    }

    public float Friction(bool on, bool ripped)
    {
        if (on) return 0f;
        if (!on && !ripped) return engineFriction;
        return engineFriction * 20f;
    }
    public float Boost(float altitude)
    {
        float altitudeFactor = Mathf.InverseLerp(boostBestAltitude, boostMaxAltitude, altitude);
        return Mathf.Lerp(boostValue, 1f, altitudeFactor );
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(EnginePreset))]
public class EnginePresetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EnginePreset preset = (EnginePreset)target;

        //General settings
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("General Settings", MessageType.None);
        GUI.color = GUI.backgroundColor;
        preset.designation = EditorGUILayout.TextField("Designation", preset.designation);
        preset.constructor = EditorGUILayout.TextField("Constructor", preset.constructor);
        preset.type = (EnginePreset.Type)EditorGUILayout.EnumPopup("Type", preset.type);
        preset.fuelMixer = (EnginePreset.FuelMixerType)EditorGUILayout.EnumPopup("Fuel Mixer", preset.fuelMixer);
        preset.cylinders = EditorGUILayout.IntField("No of cylinders", preset.cylinders);
        preset.weight = EditorGUILayout.FloatField("Dry weight (NO Propeller)", preset.weight);

        //Performances settings
        GUILayout.Space(15f);
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Performances Settings", MessageType.None);
        GUI.color = GUI.backgroundColor;
        preset.ignitionTime = EditorGUILayout.FloatField("Ignition time", preset.ignitionTime);
        preset.idleRPS = EditorGUILayout.FloatField("Idle RPM", preset.idleRPS / (Mathf.PI / 30f)) * (Mathf.PI / 30f);
        preset.nominalRPS = EditorGUILayout.FloatField("Nominal RPM", preset.nominalRPS / (Mathf.PI / 30f)) * (Mathf.PI / 30f);
        if (preset.type == EnginePreset.Type.Jet)
        {
            preset.maxThrust = EditorGUILayout.FloatField("Max Thrust Newtons", preset.maxThrust);
        } else
        {
            preset.boostRPS = EditorGUILayout.FloatField("Boosted RPM", preset.boostRPS / (Mathf.PI / 30f)) * (Mathf.PI / 30f);
            preset.boostValue = EditorGUILayout.FloatField("Boost Throttle Value", preset.boostValue);
            preset.boostTime = EditorGUILayout.FloatField("Boost time sec", preset.boostTime);
            preset.boostBestAltitude = EditorGUILayout.FloatField("Boost Best Altitude", preset.boostBestAltitude);
            preset.boostMaxAltitude = EditorGUILayout.FloatField("Boost Max Altitude", preset.boostMaxAltitude);
            preset.gears = Mathf.Clamp(EditorGUILayout.IntField("Gears", preset.gears), 1, 2);
            preset.gear1 = EditorGUILayout.CurveField("Gear 1 Pwr/Alt Hp", preset.gear1);
            if (preset.gears >= 2) preset.gear2 = EditorGUILayout.CurveField("Gear 2 Pwr/Alt Hp", preset.gear2);
        }

        //Temperature Settings
        GUILayout.Space(15f);
        GUI.color = Color.red;
        EditorGUILayout.HelpBox("Temperature Settings", MessageType.None);
        GUI.color = GUI.backgroundColor;
        preset.waterTemperature = EditorGUILayout.FloatField("Water Temperature Full", preset.waterTemperature);
        preset.oilTemperature = EditorGUILayout.FloatField("Oil Temperature Full", preset.oilTemperature);

        //Fuel Settings
        GUILayout.Space(15f);
        GUI.color = Color.black;
        EditorGUILayout.HelpBox("Fuel Settings", MessageType.None);
        GUI.color = GUI.backgroundColor;
        preset.idleConsumption = EditorGUILayout.FloatField("Consumption Coeff Idle", preset.idleConsumption);
        preset.halfConsumption = EditorGUILayout.FloatField("Consumption Coeff Half", preset.halfConsumption);
        preset.fullConsumption = EditorGUILayout.FloatField("Consumption Coeff Full", preset.fullConsumption);

        //Effects Settings
        GUILayout.Space(15f);
        GUI.color = Color.green;
        EditorGUILayout.HelpBox("Effects Settings", MessageType.None);
        GUI.color = GUI.backgroundColor;
        preset.ignitionEffect = EditorGUILayout.ObjectField("Ignition effect", preset.ignitionEffect, typeof(ParticleSystem), false) as ParticleSystem;
        preset.boostEffect = EditorGUILayout.ObjectField("Boost effect", preset.boostEffect, typeof(ParticleSystem), false) as ParticleSystem;
        preset.overHeatEffect = EditorGUILayout.ObjectField("Over Heat effect", preset.overHeatEffect, typeof(ParticleSystem), false) as ParticleSystem;
        preset.burningEffect = EditorGUILayout.ObjectField("Burning effect", preset.burningEffect, typeof(ParticleSystem), false) as ParticleSystem;

        //Audio Settings
        GUILayout.Space(15f);
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Audio Settings", MessageType.None);
        GUI.color = GUI.backgroundColor;
        preset.startUpAudio = EditorGUILayout.ObjectField("Engine Startup Audio", preset.startUpAudio, typeof(AudioClip),false) as AudioClip;
        preset.shutDownAudio = EditorGUILayout.ObjectField("Engine Shutdown Audio", preset.shutDownAudio, typeof(AudioClip), false) as AudioClip;
        SerializedProperty pops = serializedObject.FindProperty("enginePops");
        EditorGUILayout.PropertyField(pops, true);

        preset.idleAudioCockpit = EditorGUILayout.ObjectField("Engine Idle Audio Cockpit", preset.idleAudioCockpit, typeof(AudioClip), false) as AudioClip;
        preset.fullAudioCockpit = EditorGUILayout.ObjectField("Engine Full Audio Cockpt", preset.fullAudioCockpit, typeof(AudioClip), false) as AudioClip;

        preset.idleAudioExtSelf = EditorGUILayout.ObjectField("Engine Idle Audio Self External", preset.idleAudioExtSelf, typeof(AudioClip), false) as AudioClip;
        preset.fullAudioExtSelf = EditorGUILayout.ObjectField("Engine Full Audio Self External", preset.fullAudioExtSelf, typeof(AudioClip), false) as AudioClip;

        preset.spatialAudio = EditorGUILayout.ObjectField("Spatial Ambiant", preset.spatialAudio, typeof(AudioClip), false) as AudioClip;

        preset.fullRps = EditorGUILayout.FloatField("Full At RPM", preset.fullRps / (Mathf.PI / 30f)) * (Mathf.PI / 30f);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(preset);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
