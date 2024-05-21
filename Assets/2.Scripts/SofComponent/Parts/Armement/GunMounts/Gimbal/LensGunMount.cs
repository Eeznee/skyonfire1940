using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class LensGunMount : GimbalGunMount
{
    public Transform lens;
    public float lensRotationSpeed = 60f;

    private float lensAngle = 0f;


    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        lensAngle = lens.localEulerAngles.z;
    }
    public override void OperateSpecialManual(float input)
    {
        lensAngle += Time.deltaTime * lensRotationSpeed * input;
        lensAngle = lensAngle % 360f;

        Vector3 euler = lens.localEulerAngles;
        euler.z = lensAngle;
        lens.localEulerAngles = euler;
    }
    public override void OperateSpecialTracking(Vector3 direction)
    {
        Vector3 flattenedDirection = Vector3.ProjectOnPlane(direction, lens.forward);
        if (flattenedDirection.sqrMagnitude < 0.2f*0.2f) return;
        Quaternion target = Quaternion.LookRotation(lens.forward, -flattenedDirection);
        lens.rotation = Quaternion.RotateTowards(lens.rotation, target, Time.deltaTime * lensRotationSpeed);
    }
}
#if UNITY_EDITOR

[CustomEditor(typeof(LensGunMount))]
public class LensGunMountEditor : GimbalGunMountEditor
{

    SerializedProperty lens;
    SerializedProperty lensRotationSpeed;
    static bool showLens = true;

    protected override void OnEnable()
    {
        base.OnEnable();
        lens = serializedObject.FindProperty("lens");
        lensRotationSpeed = serializedObject.FindProperty("lensRotationSpeed");
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        showLens = EditorGUILayout.Foldout(showLens, "Lens", true, EditorStyles.foldoutHeader);
        if (showLens)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(lens, new GUIContent("Lens Transform"));
            EditorGUILayout.PropertyField(lensRotationSpeed, new GUIContent("Rotate Rate °/s"));
            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif