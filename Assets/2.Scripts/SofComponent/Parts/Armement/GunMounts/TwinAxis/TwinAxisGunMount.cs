using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public abstract class TwinAxisGunMount : GunMount
{
    public Transform traversor;
    public Transform elevator;

    public float horizontalRate = 60f;
    public float verticalRate = 60f;


    [SerializeField] public AxisConstrains constrains = new AxisConstrains(null);
    [SerializeField] public AxisConstrains hydraulicsConstrains = new AxisConstrains(null);

    protected float traverseAngle = 0f;
    protected float elevationAngle = 0f;

    public Transform GunAttachedTr => traversor.IsChildOf(elevator) ? traversor : elevator;

    public override Vector3 FiringDirection => GunAttachedTr.forward;
    public override Vector3 CameraUp => GunAttachedTr.up;
    protected bool LimitedTraverse => constrains.limitedTraverse && Mathf.Abs(constrains.rightTraverseLimit - constrains.leftTraverseLimit) < 359.5f;
    public override float TargetAvailability(Vector3 pos)
    {
        Vector3 localDir = transform.InverseTransformPoint(pos).normalized;
        return Mathf.InverseLerp(-0.5f, 0f, localDir.y);
    }

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        traverseAngle = traversor.localRotation.eulerAngles.y;
        elevationAngle = -elevator.localEulerAngles.x;
    }
    public override void OperateMainTracking(Vector3 direction)
    {
        Vector3 traverseAim = Vector3.ProjectOnPlane(direction, traversor.up);
        float traverseTarget = Vector3.SignedAngle(traversor.forward, traverseAim, traversor.up);

        Vector3 elevatorAim = Vector3.ProjectOnPlane(direction, elevator.right);
        float elevationTarget = Vector3.SignedAngle(elevator.forward, elevatorAim, -elevator.right);

        Vector2 input = new Vector2(traverseTarget / horizontalRate, elevationTarget / verticalRate) / Time.deltaTime;

        if (float.IsNaN(input.x + input.y)) input = Vector2.zero;
        OperateMainManual(input);
    }
    public override void OperateMainManual(Vector2 input)
    {
        Traverse(horizontalRate * Time.deltaTime * input.x);
        Elevate(verticalRate * Time.deltaTime * input.y);

        traversor.localEulerAngles = Vector3.up * traverseAngle;
        elevator.localEulerAngles = -Vector3.right * elevationAngle;
    }
    protected void Traverse(float traverse)
    {
        float forcedElevation = MinimumElevation(traverseAngle + traverse) - elevationAngle;

        float maxElevation = verticalRate * Time.deltaTime;
        if (forcedElevation > maxElevation)
            traverse *= maxElevation / forcedElevation;

        traverseAngle += traverse;
        traverseAngle = (traverseAngle + 180f) % 360f - 180f;
        if (LimitedTraverse)
            traverseAngle = Mathf.Clamp(traverseAngle, LeftLimit(), RightLimit());
    }
    protected void Elevate(float elevate)
    {
        elevationAngle += elevate;
        elevationAngle = Mathf.Clamp(elevationAngle, MinimumElevation(traverseAngle), MaximumElevation(traverseAngle));
    }
    public virtual float MinimumElevation(float traverseAngle)
    {
        traverseAngle = (traverseAngle + 180f) % 360f - 180f;

        if (!linkedHydraulics) return constrains.MinElevation(traverseAngle);
        else
        {
            float min0 = constrains.MinElevation(traverseAngle);
            float min1 = hydraulicsConstrains.MinElevation(traverseAngle);
            return Mathf.Lerp(min0, min1, HydraulicsFactor);
        }
    }
    public virtual float MaximumElevation(float traverseAngle)
    {
        traverseAngle = (traverseAngle + 180f) % 360f - 180f;

        if (!linkedHydraulics) return constrains.MaxElevation(traverseAngle);
        else
        {
            float max0 = constrains.MaxElevation(traverseAngle);
            float max1 = hydraulicsConstrains.MaxElevation(traverseAngle);
            return Mathf.Lerp(max0, max1, HydraulicsFactor);
        }
    }
    public float RightLimit()
    {
        if (!linkedHydraulics)
            return constrains.rightTraverseLimit;
        else
            return Mathf.Lerp(constrains.rightTraverseLimit, hydraulicsConstrains.rightTraverseLimit, HydraulicsFactor);
    }
    public float LeftLimit()
    {
        if (!linkedHydraulics)
            return constrains.leftTraverseLimit;
        else
            return Mathf.Lerp(constrains.leftTraverseLimit, hydraulicsConstrains.leftTraverseLimit, HydraulicsFactor);
    }
}
