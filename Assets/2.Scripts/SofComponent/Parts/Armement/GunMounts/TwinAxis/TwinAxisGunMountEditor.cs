using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


#if UNITY_EDITOR

[CustomEditor(typeof(TwinAxisGunMount))]
public class TwinAxisGunMountEditor : GunMountEditor
{
    SerializedProperty traversor;
    SerializedProperty elevator;

    SerializedProperty constrains;
    SerializedProperty hydraulicsConstrains;

    protected override void OnEnable()
    {
        base.OnEnable();
        traversor = serializedObject.FindProperty("traversor");
        elevator = serializedObject.FindProperty("elevator");

        constrains = serializedObject.FindProperty("constrains");
        hydraulicsConstrains = serializedObject.FindProperty("hydraulicsConstrains");
    }
    protected override void MainSettings()
    {
        base.MainSettings();

        EditorGUILayout.PropertyField(traversor);
        EditorGUILayout.PropertyField(elevator);
    }

    protected override void Movement()
    {
        base.Movement();
        EditorGUILayout.PropertyField(constrains);
    }
    protected override void MovementHydraulics()
    {
        base.MovementHydraulics();

        TwinAxisGunMount turret = (TwinAxisGunMount)target;
        if (!turret.linkedHydraulics) return;

        turret.hydraulicsConstrains.limitedTraverse = turret.constrains.limitedTraverse;
        EditorGUILayout.PropertyField(hydraulicsConstrains);
    }

    //EDITOR DRAWING
    int segmentsFullCircle = 360;
    protected void OnSceneGUI()
    {
        TwinAxisGunMount t = (TwinAxisGunMount)target;

        if (!t.traversor || !t.elevator) return;

        DrawAngleLimits();
    }
    protected void DrawAngleLimits()
    {
        Handles.color = new Color(0.1f, 1f, 0.1f, 0.7f);

        DrawArc(2f, false);
        DrawArc(1.5f, false);
        DrawArc(1f, false);
        DrawArc(2f, true);
        DrawArc(1.5f, true);
        DrawArc(1f, true);

        Handles.color = new Color(1f, 0.1f, 0.1f, 0.7f);

        DrawRadiusLines(2f, false);
        DrawRadiusLines(2f, true);
    }
    protected void DrawArc(float radius, bool upper)
    {
        Vector2 anglesLimits = DrawAnglesRange();

        int segments = Mathf.CeilToInt((anglesLimits.y - anglesLimits.x) * segmentsFullCircle / 360f);

        Vector3[] points = new Vector3[segments + 1];


        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Lerp(anglesLimits.x, anglesLimits.y, (float)i / segments);
            points[i] = PointPosition(angle, radius, upper);
        }

        Handles.DrawPolyLine(points);
    }
    protected void DrawRadiusLines(float radius, bool upper)
    {
        Vector2 anglesLimits = DrawAnglesRange();

        for (float angle = -180f; angle < 180f; angle += 15f)
            if (angle >= anglesLimits.x && angle <= anglesLimits.y) Handles.DrawLine(OriginPos(angle), PointPosition(angle, radius, upper));
    }

    protected virtual Vector3 OriginPos(float traverseAngle)
    {
        TwinAxisGunMount t = (TwinAxisGunMount)target;
        return t.GunAttachedTr.position;
    }
    protected virtual Transform Tr()
    {
        TwinAxisGunMount t = (TwinAxisGunMount)target;
        return t.GunAttachedTr.parent.parent;
    }
    protected Vector3 PointPosition(float traverseAngle, float arcSize, bool upperLimit)
    {
        TwinAxisGunMount t = (TwinAxisGunMount)target;
        Transform tr = Tr() ? Tr() : t.transform;

        Vector3 dir = tr.forward;
        float limit = upperLimit ? t.MaximumElevation(traverseAngle) : t.MinimumElevation(traverseAngle);
        dir = Quaternion.AngleAxis(limit, -tr.right) * dir;
        dir = Quaternion.AngleAxis(traverseAngle, tr.up) * dir;
        return dir * arcSize + OriginPos(traverseAngle);
    }
    protected virtual Vector2 DrawAnglesRange()
    {
        TwinAxisGunMount t = (TwinAxisGunMount)target;
        if (!t.constrains.limitedTraverse) return new Vector2(-180f, 180f);
        return new Vector2(t.LeftLimit(), t.RightLimit());
    }
}
#endif