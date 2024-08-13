using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


public class ReflectorSight : MonoBehaviour
{
    public enum AdjustmentType
    {
        None,
        SpanIndicators
    }
    

    public Renderer[] reticles;
    private float[] maxEmmissions;

    public AdjustmentType adjustment;

    public Transform rightSpanIndicator;
    public Transform leftSpanIndicator;
    public float minSpanIndicatorOffset = 0f;
    public float maxSpanIndicatorOffset = 5f;

    public void AdjustReticle(float adjustValue)
    {
        switch (adjustment)
        {
            case AdjustmentType.None: return;
            case AdjustmentType.SpanIndicators:
                AdjustSpan(adjustValue);
                return;
        }
    }
    private void AdjustSpan(float value)
    {
        float angle = Mathf.Lerp(minSpanIndicatorOffset, maxSpanIndicatorOffset, value);
        if (rightSpanIndicator)
            rightSpanIndicator.localRotation = Quaternion.Euler(0f, angle, 0f);
        if(leftSpanIndicator)
            leftSpanIndicator.localRotation = Quaternion.Euler(0f, -angle, 0f);
    }
    private void ChangeBrightness(float brightnessLevel)
    {
        for (int i = 0; i < reticles.Length; i++)
        {
            float emissionValue = maxEmmissions[i] * brightnessLevel;
            reticles[i].material.SetFloat("_Emission", emissionValue);
        }
    }

    private void Start()
    {
        maxEmmissions = new float[reticles.Length];

        for (int i = 0; i < reticles.Length; i++)
            maxEmmissions[i] = reticles[i].sharedMaterial.GetFloat("_Emission");
    }

    private void Update()
    {
        //ChangeBrightness(0.5f);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(ReflectorSight)), CanEditMultipleObjects]
public class ReflectorSightEditor : Editor
{
    SerializedProperty reticles;

    SerializedProperty adjustment;

    SerializedProperty rightSpanIndicator;
    SerializedProperty leftSpanIndicator;
    SerializedProperty minSpanIndicatorOffset;
    SerializedProperty maxSpanIndicatorOffset;

    static float adjustmentValue;

    private void OnEnable()
    {
        reticles = serializedObject.FindProperty("reticles");

        adjustment = serializedObject.FindProperty("adjustment");

        rightSpanIndicator = serializedObject.FindProperty("rightSpanIndicator");
        leftSpanIndicator = serializedObject.FindProperty("leftSpanIndicator");
        minSpanIndicatorOffset = serializedObject.FindProperty("minSpanIndicatorOffset");
        maxSpanIndicatorOffset = serializedObject.FindProperty("maxSpanIndicatorOffset");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        ReflectorSight reflector = (ReflectorSight)target;

        EditorGUILayout.PropertyField(reticles);

        EditorGUILayout.PropertyField(adjustment);
        adjustmentValue = EditorGUILayout.Slider("Adjustment Test Value",adjustmentValue, 0f,1f);

        if (reflector.adjustment == ReflectorSight.AdjustmentType.SpanIndicators)
        {
            EditorGUILayout.PropertyField(rightSpanIndicator);
            EditorGUILayout.PropertyField(leftSpanIndicator);
            EditorGUILayout.MinMaxSlider("Angles Limits",ref reflector.minSpanIndicatorOffset, ref reflector.maxSpanIndicatorOffset, -15f, 15f);
            EditorGUILayout.PropertyField(minSpanIndicatorOffset);
            EditorGUILayout.PropertyField(maxSpanIndicatorOffset);

            reflector.AdjustReticle(adjustmentValue);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
