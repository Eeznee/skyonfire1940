using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class LensGimbal : GimbalTurret
{
    public Transform lens;
    public float lensRotationSpeed = 60f;
    private Vector3 defaultForward;
    private float lensAngle = 0f;

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        defaultForward = lens.parent.InverseTransformDirection(lens.forward);
    }

    protected override void SpecialAxisTarget()
    {
        Vector3 up = Vector3.ProjectOnPlane(targetDirection, lens.forward);
        if (up.sqrMagnitude < 0.01f) return;
        up = lens.parent.InverseTransformDirection(up);
        Quaternion target = Quaternion.LookRotation(defaultForward, -up);
        lens.localRotation = Quaternion.RotateTowards(lens.localRotation, target, Time.deltaTime * lensRotationSpeed);
    }
    protected override void SpecialAxisInput()
    {
        lensAngle += Time.deltaTime * lensRotationSpeed * specialAxis;
        lensAngle = lensAngle % 360f;
        Vector3 euler = lens.localEulerAngles;
        euler.z = lensAngle;
        lens.localEulerAngles = euler;
    }

#if UNITY_EDITOR
    //GIZMOS
    public override void OnDrawGizmos()
    {
        if (rotator)
        {
            for (int i = 0; i < 72; i++)
            {
                Handles.color = new Color(0.1f, 1f, 0.1f, 0.3f);

                Vector3 dir = Quaternion.AngleAxis(i * 5f, rotator.forward) * Quaternion.AngleAxis(alphaConstrains.Evaluate(i * 5f), rotator.right) * rotator.forward;
                Handles.DrawLine(rotator.position + dir * 0.1f, rotator.position + dir * 3f);
            }
        }
    }
#endif
}
#if UNITY_EDITOR

[CustomEditor(typeof(LensGimbal))]
public class LensGimbalEditor : GimbalTurretEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        LensGimbal turret = (LensGimbal)target;
        turret.lens = EditorGUILayout.ObjectField("Lens Transform", turret.lens, typeof(Transform), true) as Transform;
        turret.lensRotationSpeed = EditorGUILayout.FloatField("Rotation speed deg/s", turret.lensRotationSpeed);
        if (GUI.changed)
        {
            EditorUtility.SetDirty(turret);
            EditorSceneManager.MarkSceneDirty(turret.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif