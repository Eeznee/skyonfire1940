using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR

[CustomEditor(typeof(GimbalGunMount))]
public class GimbalGunMountEditor : GunMountEditor
{
    SerializedProperty localUp;
    SerializedProperty rotator;
    SerializedProperty rotationSpeed;

    SerializedProperty alphaConstrains;
    SerializedProperty animatedAlphaConstrains;
    protected override void OnEnable()
    {
        base.OnEnable();
        alphaConstrains = serializedObject.FindProperty("alphaConstrains");
        animatedAlphaConstrains = serializedObject.FindProperty("animatedAlphaConstrains");


        localUp = serializedObject.FindProperty("localUp");
        rotator = serializedObject.FindProperty("rotator");
        rotationSpeed = serializedObject.FindProperty("rotationSpeed");
    }
    protected override void MainSettings()
    {
        base.MainSettings();

        EditorGUILayout.PropertyField(localUp,new GUIContent("Tilt With Mount"));
        EditorGUILayout.PropertyField(rotator);
        EditorGUILayout.PropertyField(rotationSpeed, new GUIContent("Rotate Rate °/s"));
    }
    protected override void Movement()
    {
        base.Movement();

        EditorGUILayout.PropertyField(alphaConstrains);
    }
    protected override void MovementHydraulics()
    {
        base.MovementHydraulics();

        GimbalGunMount turret = (GimbalGunMount)target;
        if (!turret.linkedHydraulics) return;

        EditorGUILayout.PropertyField(animatedAlphaConstrains);
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.ApplyModifiedProperties();
    }

    //EDITOR DRAWING
    int segments = 360;
    protected void OnSceneGUI()
    {
        GimbalGunMount t = (GimbalGunMount)target;

        if (!t.rotator) return;

        DrawAngleLimits();
    }
    protected void DrawAngleLimits()
    {
        Handles.color = new Color(0.1f, 1f, 0.1f, 0.7f);

        DrawArc(2f);
        DrawArc(1.5f);
        DrawArc(1f);

        Handles.color = new Color(1f, 0.1f, 0.1f, 0.7f);

        DrawRadiusLines(2f);
    }
    protected void DrawArc(float radius)
    {
        Vector3[] points = new Vector3[segments + 1];

        for (int i = 0; i <= segments; i++)
            points[i] = PointPosition(360f * i / segments, radius);

        Handles.DrawPolyLine(points);
    }
    protected Vector3 PointPosition(float traverseAngle, float arcSize)
    {
        GimbalGunMount t = (GimbalGunMount)target;

        Vector3 dir = Quaternion.AngleAxis(traverseAngle, t.rotator.forward) * Quaternion.AngleAxis(t.alphaConstrains.Evaluate(traverseAngle), t.rotator.right) * t.rotator.forward;

        return dir * arcSize + t.rotator.position;
    }
    protected void DrawRadiusLines(float radius)
    {
        GimbalGunMount t = (GimbalGunMount)target;

        for (float angle = -180f; angle < 179f; angle += 15f)
            if (angle > -180f && angle < 180f) Handles.DrawLine(t.rotator.position, PointPosition(angle, radius));
    }
}
#endif