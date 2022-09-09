using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class GimbalTurret : Turret
{
    public Transform rotator;
    public float rotationSpeed = 70f;
    public AnimationCurve alphaConstrains = AnimationCurve.Constant(0f, 360f, 30f);
    public AnimationCurve animatedAlphaConstrains = AnimationCurve.Constant(0f, 360f, 30f);
    public bool localUp = false;

    private float averageAngle;

    private void Start()
    {
        averageAngle = 0f;
        for (int i = 0; i < 360; i += 30)
            averageAngle += alphaConstrains.Evaluate(i) / 12f;
    }

    public virtual float MaxAlpha(float theta)
    {
        theta %= 360f;
        if (animatedConstrains) { return Mathf.Lerp(alphaConstrains.Evaluate(theta), animatedAlphaConstrains.Evaluate(theta), animationFactor); }
        return alphaConstrains.Evaluate(theta);
    }

    public override float TargetAvailability(Vector3 pos)
    {
        float angle = Vector3.Angle(transform.forward, pos - transform.position);
        return Mathf.InverseLerp(averageAngle*2f,averageAngle*0.7f,angle);
    }
    protected override void BasicAxisTarget()
    {
        base.BasicAxisTarget();
        Transform parent = rotator.parent;
        Vector3 upDirection = Vector3.ProjectOnPlane(targetDirection, parent.forward);
        float targetTheta = Vector3.SignedAngle(upDirection, parent.up, -parent.forward) + 180f;
        float targetAlpha = Mathf.Min(Vector3.Angle(targetDirection, parent.forward), MaxAlpha(targetTheta));
        Vector3 lookDirection = Quaternion.AngleAxis(targetTheta, parent.forward) * Quaternion.AngleAxis(targetAlpha, parent.right) * parent.forward;
        Quaternion desiredRotation = Quaternion.LookRotation(lookDirection, localUp ? parent.up : transform.root.up);
        rotator.rotation = Quaternion.RotateTowards(rotator.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
    }

#if UNITY_EDITOR
    //GIZMOS
    public virtual void OnDrawGizmos()
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

[CustomEditor(typeof(GimbalTurret))]
public class GimbalTurretEditor : TurretEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GimbalTurret turret = (GimbalTurret)target;
        turret.rotator = EditorGUILayout.ObjectField("Rotator Transform", turret.rotator, typeof(Transform), true) as Transform;
        turret.localUp = EditorGUILayout.Toggle("Local Up", turret.localUp);
        turret.rotationSpeed = EditorGUILayout.FloatField("Rotation speed deg/s", turret.rotationSpeed);
        turret.alphaConstrains = EditorGUILayout.CurveField("Max Alpha By Theta", turret.alphaConstrains);

        if (turret.animatedConstrains)
        {
            turret.animatedAlphaConstrains = EditorGUILayout.CurveField("Max Alpha By Theta Animated", turret.animatedAlphaConstrains);
        }
        if (GUI.changed)
        {
            EditorUtility.SetDirty(turret);
            EditorSceneManager.MarkSceneDirty(turret.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif