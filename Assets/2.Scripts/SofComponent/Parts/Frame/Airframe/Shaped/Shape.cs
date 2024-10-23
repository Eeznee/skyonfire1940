using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.UIElements;


[Serializable]
public struct Shape
{
    [HideInInspector] public Transform parent;
    [HideInInspector] public bool snapped;

    public Vector3 localPosition;
    public Vector2 scale;

    public float incidence;
    public float dihedral;
    public float sweep;

    public float tipScale;

    public Quaternion localRotation { get { return Quaternion.Euler(incidence, 0f, dihedral); } }

    public Vector3 position { get { return parent.TransformPoint(localPosition); } }
    public Vector3 forward { get { return parent.TransformDirection(localForward); } }
    public Vector3 right { get { return parent.TransformDirection(localRight); } }

    public Vector3 localForward { get { return localRotation * Vector3.forward; } }
    public Vector3 localUp { get { return localRotation * Vector3.up; } }
    public Vector3 localRight { get { return localRotation * Vector3.right; } }

    public Vector3 controlSurfaceAxis { get { return localRotation * Vector3.forward; } }

    public Quad ToQuad()
    {
        Vector3 sweepAngleOffset = localForward * Mathf.Abs(scale.x) / Mathf.Tan((90f - sweep) * Mathf.Deg2Rad);
        Vector3 rootLiftPos = localPosition - (localRight * (scale.x * 0.5f));
        rootLiftPos -= 0.5f * sweepAngleOffset;
        Vector3 tipLiftPos = rootLiftPos + (localRight * scale.x);
        tipLiftPos += sweepAngleOffset;

        Vector3 forward = localForward * scale.y;
        Vector3 lt = tipLiftPos + (forward * (1f - Quad.liftLine) * tipScale / 100f);
        Vector3 lb = rootLiftPos + (forward * (1f - Quad.liftLine));
        Vector3 tt = tipLiftPos - (forward * Quad.liftLine * tipScale / 100f);
        Vector3 tb = rootLiftPos - (forward * Quad.liftLine);

        return new Quad(parent, lt, lb, tt, tb);
    }
    public bool SnapTo(ShapedAirframe snapTo)
    {
        Vector2 previousScale = scale;
        Vector3 previousPos = localPosition;
        Quaternion previousRotation = localRotation;

        scale = SnapScale(snapTo.shape);
        incidence = snapTo.shape.incidence;
        localPosition = SnapPosition(snapTo.shape);

        return previousScale != scale || previousPos != localPosition || previousRotation != localRotation;
    }
    Vector2 SnapScale(Shape snapTo)
    {
        return new Vector2(scale.x, snapTo.scale.y * snapTo.tipScale * 0.01f);
    }
    Vector3 SnapPosition(Shape snapTo)
    {
        Vector3 worldPos = snapTo.position;
        worldPos += snapTo.right * snapTo.scale.x * 0.5f;
        worldPos += scale.x * right * 0.5f;
        worldPos += 0.5f * snapTo.forward * Mathf.Abs(snapTo.scale.x) / Mathf.Tan((90f - snapTo.sweep) * Mathf.Deg2Rad);
        worldPos += 0.5f * forward * Mathf.Abs(scale.x) / Mathf.Tan((90f - sweep) * Mathf.Deg2Rad);
        return parent.InverseTransformPoint(worldPos);
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(Shape))]
public class ShapeDrawer : PropertyDrawer
{
    public static float height = 200f;
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return height;
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        bool snapped = property.FindPropertyRelative("snapped").boolValue;
        float startPos = position.y;
        position.height = 20f;

        Transform parent = (Transform)property.FindPropertyRelative("parent").objectReferenceValue;
        bool wing = parent.GetComponent<Wing>();

        EditorGUI.BeginProperty(position, label, property);

        if (snapped)
        {
            EditorGUI.LabelField(position, "Locked values due to wing snapping");
            position.y += 20f;
        }

        GUI.enabled = !snapped;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("localPosition"), new GUIContent("Position"));
        position.y += 20f;

        GUI.enabled = !snapped;
        EditorGUI.Slider(position, property.FindPropertyRelative("incidence"), -10f, 10f, new GUIContent("Incidence Angle °"));
        position.y += 20f;
        GUI.enabled = true;
        EditorGUI.Slider(position, property.FindPropertyRelative("dihedral"), wing ? -30f : -90f, wing ? 30f : 90f, new GUIContent("Dihedral Angle °"));
        position.y += 20f;
        EditorGUI.Slider(position, property.FindPropertyRelative("sweep"), -60f, 60f, new GUIContent("Sweep Angle °"));
        position.y += 20f;

        Scale(ref position, property, snapped);

        EditorGUI.Slider(position, property.FindPropertyRelative("tipScale"), 5f, 100f, new GUIContent("Tip Scale %"));
        position.y += 20f;

        EditorGUI.EndProperty();

        height = position.y - startPos;

        ResetValueIfDefault(property);
    }
    void Scale(ref Rect position, SerializedProperty property, bool snapped)
    {
        var contentRect = new Rect(position.x, position.y, position.width, 20f);
        position.y += 20f;
        contentRect = EditorGUI.PrefixLabel(contentRect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Scale"));
        var labels = new[] { new GUIContent("Span"), new GUIContent("Chord") };
        var properties = new[] { property.FindPropertyRelative("scale.x"), property.FindPropertyRelative("scale.y") };
        var actives = new[] { true, !snapped };

        DrawMultiplePropertyFields(contentRect, labels, properties, actives);
    }

    private const float SubLabelSpacing = 4;
    private static void DrawMultiplePropertyFields(Rect pos, GUIContent[] subLabels, SerializedProperty[] props, bool[] active)
    {
        var indent = EditorGUI.indentLevel;
        var labelWidth = EditorGUIUtility.labelWidth;

        var propsCount = props.Length;
        var width = (pos.width - (propsCount - 1) * SubLabelSpacing) / propsCount;
        var contentPos = new Rect(pos.x, pos.y, width, pos.height);
        EditorGUI.indentLevel = 0;
        for (var i = 0; i < propsCount; i++)
        {
            EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(subLabels[i]).x;
            GUI.enabled = active[i];
            EditorGUI.PropertyField(contentPos, props[i], subLabels[i]);
            contentPos.x += width + SubLabelSpacing;
        }

        EditorGUIUtility.labelWidth = labelWidth;
        EditorGUI.indentLevel = indent;
        GUI.enabled = true;
    }

    private void ResetValueIfDefault(SerializedProperty property)
    {
        if (property.FindPropertyRelative("scale.x").floatValue != 0f) return;
        if (property.FindPropertyRelative("scale.y").floatValue != 0f) return;
        if (property.FindPropertyRelative("tipScale").floatValue != 0f) return;

        property.FindPropertyRelative("scale.x").floatValue = 1f;
        property.FindPropertyRelative("scale.y").floatValue = 1f;
        property.FindPropertyRelative("tipScale").floatValue = 100f;
    }
}
#endif