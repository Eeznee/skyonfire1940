using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class GimbalGunMount : GunMount
{
    public Transform rotator;
    public float rotationSpeed = 70f;
    public AnimationCurve alphaConstrains = AnimationCurve.Constant(0f, 360f, 30f);
    public AnimationCurve animatedAlphaConstrains = AnimationCurve.Constant(0f, 360f, 30f);
    public bool localUp = false;

    private float averageAngle;

    public override Vector3 CameraUp => rotator.up;
    public override Vector3 FiringDirection => rotator.forward;
    protected virtual float MaxAlpha(float theta)
    {
        theta = (theta + 180f) % 360f - 180f;
        if (linkedHydraulics) { return Mathf.Lerp(alphaConstrains.Evaluate(theta), animatedAlphaConstrains.Evaluate(theta), HydraulicsFactor); }
        return alphaConstrains.Evaluate(theta);
    }
    public override float TargetAvailability(Vector3 pos)
    {
        float angle = Vector3.Angle(transform.forward, pos - transform.position);
        return Mathf.InverseLerp(averageAngle * 2f, averageAngle * 0.7f, angle);
    }

    private void Start()
    {
        averageAngle = 0f;
        for (int i = 0; i < 360; i += 30)
            averageAngle += alphaConstrains.Evaluate(i) / 12f;
    }

    public override void OperateMainManual(Vector2 input)
    {
        if (input.sqrMagnitude > 1f) input /= input.magnitude;

        Vector3 targetDirection = rotator.forward;
        targetDirection = Quaternion.AngleAxis(input.x * rotationSpeed * Time.deltaTime, SofCamera.tr.up) * targetDirection;
        targetDirection = Quaternion.AngleAxis(-input.y * rotationSpeed * Time.deltaTime, SofCamera.tr.right) * targetDirection;

        OperateMainTracking(targetDirection);
    }
    public override void OperateMainTracking(Vector3 direction)
    {
        float targetTheta = Theta(direction);
        float targetAlpha = AlphaClamped(direction, targetTheta);

        rotator.rotation = Quaternion.RotateTowards(rotator.rotation, TargetRotation(targetTheta, targetAlpha), rotationSpeed * Time.deltaTime);
    }
    private float Theta(Vector3 direction)
    {
        Transform parent = rotator.parent;
        Vector3 flattenedDirection = Vector3.ProjectOnPlane(direction, parent.forward);
        return Vector3.SignedAngle(parent.up, flattenedDirection, parent.forward) + 180f;
    }
    private float AlphaClamped(Vector3 direction, float theta)
    {
        float alpha = Vector3.Angle(direction, rotator.parent.forward);
        float maxAlpha = MaxAlpha(theta);
        return Mathf.Min(alpha, maxAlpha);
    }

    private Quaternion TargetRotation(float targetTheta, float targetAlpha)
    {
        Transform parent = rotator.parent;

        Vector3 targetDir = Quaternion.AngleAxis(targetTheta, parent.forward) * Quaternion.AngleAxis(targetAlpha, parent.right) * parent.forward;
        Vector3 targetUp = localUp ? parent.up : transform.root.up;
        return Quaternion.LookRotation(targetDir, targetUp);
    }
}