using UnityEngine;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class EnginePreset : ScriptableObject
{
    protected const float rpmToRadPerSec = (Mathf.PI / 30f);
    protected const float hpToWatts = 745.7f;
    protected const float wattsToHp = 1f / 745.7f;

    public enum FuelMixerType
    {
        Carburetor,
        Injection
    }
    [SerializeField] private string designation;
    [SerializeField] private string constructor;

    public string Designation => designation;

    [SerializeField] private float weight = 500f;
    [SerializeField] private bool liquidCooled = true;
    [SerializeField] private FuelMixerType fuelMixer = FuelMixerType.Carburetor;

    public float Weight => weight;
    public bool LiquidCooled => liquidCooled;
    public bool UsesCarburetor => fuelMixer == FuelMixerType.Carburetor;


    [SerializeField] private float nominalRPM = 3000f;
    [SerializeField] private float idleRPM = 1000f;
    public float NominalRadPerSec => Mathf.Max(nominalRPM,300f) * rpmToRadPerSec;
    public float IdleRadPerSec => idleRPM * rpmToRadPerSec;


    [SerializeField] private float preIgnitionTime = 1f;
    [SerializeField] private float ignitionTime = 4f;
    [SerializeField] private AudioClip preIgnitionClip;
    [SerializeField] private AudioClip startUpAudio;
    [SerializeField] private ParticleSystem ignitionEffect;
    public float PreIgnitionTime => preIgnitionTime;
    public float IgnitionTime => ignitionTime;
    public AudioClip PreIgnitionClip => preIgnitionClip;
    public AudioClip IgnitionClip => startUpAudio;
    public ParticleSystem IgnitionEffect => ignitionEffect;


    [SerializeField] private AudioClip idleAudioCockpit;
    [SerializeField] private AudioClip fullAudioCockpit;
    [SerializeField] private AudioClip idleAudioExtSelf;
    [SerializeField] private AudioClip fullAudioExtSelf;
    [SerializeField] private AudioClip spatialAudio;

    public AudioClip IdleAudioCockpit => idleAudioCockpit;
    public AudioClip FullAudioCockpit => fullAudioCockpit;
    public AudioClip IdleAudioExtSelf => idleAudioExtSelf;
    public AudioClip FullAudioExtSelf => fullAudioExtSelf;
    public AudioClip SpatialAudio => spatialAudio;




    public abstract float FuelConsumption(float throttle);
}
#if UNITY_EDITOR
[CustomEditor(typeof(EnginePreset))]
public class EnginePresetEditor : Editor
{
    SerializedProperty designation;
    SerializedProperty constructor;
    SerializedProperty weight;
    SerializedProperty liquidCooled;
    SerializedProperty fuelMixer;

    SerializedProperty nominalRPM;
    SerializedProperty idleRPM;

    SerializedProperty preIgnitionTime;
    SerializedProperty ignitionTime;
    SerializedProperty preIgnitionClip;
    SerializedProperty startUpAudio;
    SerializedProperty ignitionEffect;

    SerializedProperty idleAudioCockpit;
    SerializedProperty fullAudioCockpit;
    SerializedProperty idleAudioExtSelf;
    SerializedProperty fullAudioExtSelf;
    SerializedProperty spatialAudio;

    protected virtual void OnEnable()
    {
        designation = serializedObject.FindProperty("designation");
        constructor = serializedObject.FindProperty("constructor");
        weight = serializedObject.FindProperty("weight");
        liquidCooled = serializedObject.FindProperty("liquidCooled");
        fuelMixer = serializedObject.FindProperty("fuelMixer");

        nominalRPM = serializedObject.FindProperty("nominalRPM");
        idleRPM = serializedObject.FindProperty("idleRPM");

        preIgnitionTime = serializedObject.FindProperty("preIgnitionTime");
        ignitionTime = serializedObject.FindProperty("ignitionTime");
        preIgnitionClip = serializedObject.FindProperty("preIgnitionClip");
        startUpAudio = serializedObject.FindProperty("startUpAudio");
        ignitionEffect = serializedObject.FindProperty("ignitionEffect");

        idleAudioCockpit = serializedObject.FindProperty("idleAudioCockpit");
        fullAudioCockpit = serializedObject.FindProperty("fullAudioCockpit");
        idleAudioExtSelf = serializedObject.FindProperty("idleAudioExtSelf");
        fullAudioExtSelf = serializedObject.FindProperty("fullAudioExtSelf");
        spatialAudio = serializedObject.FindProperty("spatialAudio");
    }

    static bool showMain = true;
    static bool showPerformances = true;
    static bool showIgnition = true;
    static bool showAudioFX = true;
    static bool showRunningAudio = true;

    public virtual void Main()
    {
        EditorGUILayout.PropertyField(designation);
        EditorGUILayout.PropertyField(constructor);
        EditorGUILayout.PropertyField(weight);
        EditorGUILayout.PropertyField(liquidCooled);
        EditorGUILayout.PropertyField(fuelMixer);
    }
    public virtual void Performances()
    {
        EnginePreset preset = (EnginePreset)target;

        EditorGUILayout.PropertyField(nominalRPM);
        EditorGUILayout.PropertyField(idleRPM);
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EnginePreset preset = (EnginePreset)target;

        showMain = EditorGUILayout.Foldout(showMain, "Main", true, EditorStyles.foldoutHeader);
        if (showMain)
        {
            EditorGUI.indentLevel++;
            Main();
            EditorGUI.indentLevel--;
        }

        showPerformances = EditorGUILayout.Foldout(showPerformances, "Performances", true, EditorStyles.foldoutHeader);
        if (showPerformances)
        {
            EditorGUI.indentLevel++;
            Performances();
            EditorGUI.indentLevel--;
        }

        showAudioFX = EditorGUILayout.Foldout(showAudioFX, "Audio & FX", true, EditorStyles.foldoutHeader);
        if (showAudioFX)
        {
            EditorGUI.indentLevel++;

            showIgnition = EditorGUILayout.Foldout(showIgnition, "Ignition", true, EditorStyles.foldoutHeader);
            if (showIgnition)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(preIgnitionTime);
                EditorGUILayout.PropertyField(ignitionTime);
                EditorGUILayout.PropertyField(preIgnitionClip);
                EditorGUILayout.PropertyField(startUpAudio);
                EditorGUILayout.PropertyField(ignitionEffect);

                EditorGUI.indentLevel--;
            }

            showRunningAudio = EditorGUILayout.Foldout(showRunningAudio, "Running Audio", true, EditorStyles.foldoutHeader);
            if (showRunningAudio)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(idleAudioCockpit, new GUIContent("Idle Cockpit"));
                EditorGUILayout.PropertyField(fullAudioCockpit, new GUIContent("Full Cockpit"));
                EditorGUILayout.PropertyField(idleAudioExtSelf, new GUIContent("Idle External"));
                EditorGUILayout.PropertyField(fullAudioExtSelf, new GUIContent("Full External"));
                EditorGUILayout.PropertyField(spatialAudio, new GUIContent("Spatial / Far"));

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
