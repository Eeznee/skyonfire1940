using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[Serializable]
public struct AxisConstrains
{
    public bool limitedTraverse;
    public float leftTraverseLimit;
    public float rightTraverseLimit;


    public bool hasMinElevationCurve;
    public float minElevation;
    public AnimationCurve minElevationByTraverse;

    public bool hasMaxElevationCurve;
    public float maxElevation;
    public AnimationCurve maxElevationByTraverse;

    public AxisConstrains(TwinAxisGunMount turret)
    {
        limitedTraverse = false;
        leftTraverseLimit = -60f;
        rightTraverseLimit = 60f;


        hasMaxElevationCurve = false;
        maxElevation = 90f;
        maxElevationByTraverse = AnimationCurve.Linear(-180f, 70f, 180f, 70f);

        hasMinElevationCurve = false;
        minElevation = -10f;
        minElevationByTraverse = AnimationCurve.Linear(-180f, -10f, 180f, -10f);
    }

    public float MinElevation(float traverse)
    {
        if (hasMinElevationCurve) return minElevationByTraverse.Evaluate(traverse);
        return minElevation;
    }
    public float MaxElevation(float traverse)
    {
        if (hasMaxElevationCurve) return maxElevationByTraverse.Evaluate(traverse);
        return maxElevation;
    }
}
#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(AxisConstrains))]
public class DoubleAxisConstrainsDrawer : PropertyDrawer
{
    public static float height = 200f;

    public static bool maxElevation = true;
    public static bool minElevation = true;
    public static bool traverse = true;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return height;
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float startPos = position.y;
        position.height = 20f;

        EditorGUI.BeginProperty(position, label, property);

        TraverseGUI(ref position, property);
        MaxElevationGUI(ref position, property);
        MinElevationGUI(ref position, property);

        EditorGUI.EndProperty();

        height = position.y - startPos;
    }
    private void TraverseGUI(ref Rect position, SerializedProperty property)
    {
        traverse = EditorGUI.Foldout(position, traverse, "Traverse", true, EditorStyles.foldoutHeader);
        position.y += 20f;

        if (traverse)
        {
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("limitedTraverse"), new GUIContent("Limited Traverse"));
            position.y += 20f;

            if (property.FindPropertyRelative("limitedTraverse").boolValue)
            {
                EditorGUI.indentLevel++;
                Vector2 minMax = new Vector2(property.FindPropertyRelative("leftTraverseLimit").floatValue, property.FindPropertyRelative("rightTraverseLimit").floatValue);

                minMax = EditorGUI.Vector2Field(position, new GUIContent("Left/Right"), minMax);
                position.y += 20f;

                float min = minMax.x;
                float max = minMax.y;
                EditorGUI.MinMaxSlider(position, new GUIContent(""), ref min, ref max, -180f, 180f);
                position.y += 20f;

                min = Mathf.Round(min * 10f) * 0.1f;
                max = Mathf.Round(max * 10f) * 0.1f;

                property.FindPropertyRelative("leftTraverseLimit").floatValue = min;
                property.FindPropertyRelative("rightTraverseLimit").floatValue = max;
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }
    }
    private void MaxElevationGUI(ref Rect position, SerializedProperty property)
    {
        maxElevation = EditorGUI.Foldout(position, maxElevation, "Maximum Elevation", true, EditorStyles.foldoutHeader);
        position.y += 20f;

        if (maxElevation)
        {
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("hasMaxElevationCurve"), new GUIContent("Curve"));
            position.y += 20f;
            if (property.FindPropertyRelative("hasMaxElevationCurve").boolValue)
                EditorGUI.PropertyField(position, property.FindPropertyRelative("maxElevationByTraverse"), new GUIContent("Max Elevation"));
            else
                EditorGUI.PropertyField(position, property.FindPropertyRelative("maxElevation"), new GUIContent("Max Elevation"));
            position.y += 20f;
            EditorGUI.indentLevel--;
        }
    }
    private void MinElevationGUI(ref Rect position, SerializedProperty property)
    {
        minElevation = EditorGUI.Foldout(position, minElevation, "Minimum Elevation", true, EditorStyles.foldoutHeader);
        position.y += 20f;

        if (minElevation)
        {
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("hasMinElevationCurve"), new GUIContent("Curve"));
            position.y += 20f;
            if (property.FindPropertyRelative("hasMinElevationCurve").boolValue)
                EditorGUI.PropertyField(position, property.FindPropertyRelative("minElevationByTraverse"), new GUIContent("Min Elevation"));
            else
                EditorGUI.PropertyField(position, property.FindPropertyRelative("minElevation"), new GUIContent("Min Elevation"));
            position.y += 20f;
            EditorGUI.indentLevel--;
        }
    }
}
#endif