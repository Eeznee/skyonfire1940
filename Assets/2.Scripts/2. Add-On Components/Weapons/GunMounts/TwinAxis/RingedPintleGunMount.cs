using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
[AddComponentMenu("Sof Components/Weapons/Gun Mounts/Ringed Pintle")]
public class RingedPintleGunMount : PintleGunMount
{
    public Transform turretRing;
    public float ringTraverseRate = 60f;


    protected float ringTraverseAngle;

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        ringTraverseAngle = turretRing.localRotation.eulerAngles.y;
    }

    public override float MinimumElevation(float traverseAngle)
    {
        return base.MinimumElevation(traverseAngle + ringTraverseAngle);
    }
    public override void OperateSpecialManual(float input)
    {
        float traverseOffset = ringTraverseRate * Time.deltaTime * input;
        ringTraverseAngle = (ringTraverseAngle + traverseOffset) % 360f;

        turretRing.localEulerAngles = Vector3.up * ringTraverseAngle;
    }
    public override void OperateSpecialTracking(Vector3 direction)
    {
        Vector3 traverseAim = Vector3.ProjectOnPlane(direction, traversor.up);
        float targetAngleOffset = Vector3.SignedAngle(traversor.forward, traverseAim, traversor.up);
        targetAngleOffset = Mathf.Sign(targetAngleOffset) * Mathf.Min(ringTraverseRate * Time.deltaTime, Mathf.Abs(targetAngleOffset));
        ringTraverseAngle += targetAngleOffset;

        turretRing.localEulerAngles = Vector3.up * ringTraverseAngle;
    }
}
#if UNITY_EDITOR

[CustomEditor(typeof(RingedPintleGunMount))]
public class RingedDoubleAxisEditor : PintleGunMountEditor
{
    SerializedProperty turretRing;
    SerializedProperty ringTraverseRate;
    static bool showRing = true;

    protected override void OnEnable()
    {
        base.OnEnable();
        turretRing = serializedObject.FindProperty("turretRing");
        ringTraverseRate = serializedObject.FindProperty("ringTraverseRate");
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        showRing = EditorGUILayout.Foldout(showRing, "Ring", true, EditorStyles.foldoutHeader);
        if (showRing)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(turretRing, new GUIContent("Ring Transform"));
            EditorGUILayout.PropertyField(ringTraverseRate, new GUIContent("Traverse Rate °/s"));
            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
    protected override Vector3 OriginPos(float traverseAngle)
    {
        RingedPintleGunMount t = (RingedPintleGunMount)target;
        Vector3 direction = t.turretRing.InverseTransformPoint(t.GunAttachedTr.position);
        direction = t.turretRing.parent.TransformDirection(direction);
        direction = Quaternion.AngleAxis(traverseAngle, t.turretRing.up) * direction;

        return t.turretRing.position + direction;
    }
    protected override Vector2 DrawAnglesRange()
    {
        return new Vector2(-180f, 180f);
    }
    protected override Transform Tr()
    {
        RingedPintleGunMount t = (RingedPintleGunMount)target;
        if(t.turretRing) return t.turretRing.parent;
        return t.transform.parent;
    }
}
#endif