using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu(fileName = "New Jet Engine", menuName = "SOF/Aircraft Modules/Jet Engine")]
public class JetEnginePreset : EnginePreset
{
    [SerializeField] private float maxThrust = 10000f;
    [SerializeField] private float fuelConsumption = 0.15f;

    public float MaxThrust => maxThrust;


    public override float FuelConsumption(float throttle)
    {
        return fuelConsumption;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(JetEnginePreset))]
public class JetEnginePresetEditor : EnginePresetEditor
{
    SerializedProperty maxThrust;
    SerializedProperty fuelConsumption;
    protected override void OnEnable()
    {
        base.OnEnable();

        maxThrust = serializedObject.FindProperty("maxThrust");
        fuelConsumption = serializedObject.FindProperty("fuelConsumption");
    }

    public override void Performances()
    {
        base.Performances();

        EditorGUILayout.PropertyField(maxThrust, new GUIContent("Max Thrust in N"));
        EditorGUILayout.PropertyField(fuelConsumption, new GUIContent("Fuel Comsumption kg/(N.h)"));
    }
}
#endif
