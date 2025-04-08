using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[AddComponentMenu("Sof Components/Undercarriage/Steering Axis")]
public class SteeringAxis : SofComponent
{
    public enum Controls
    {
        PedalsSteering,
        FreeWheel
    }
    public Controls steeringControls;

    public Vector3 axis = Vector3.up;
    public float maxSteerAngle = 20f;

    private float steerAngle = 0f;
    private Wheel wheel;

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        wheel = GetComponentInChildren<Wheel>();
    }

    private void FixedUpdate()
    {
        if (steeringControls == Controls.PedalsSteering)
            PedalsSteeringFixedUpdate();
        else
            FreeWheelFixedUpdate();

    }
    private void PedalsSteeringFixedUpdate()
    {
        float invertAngle = Mathf.Sign(-localPos.z);
        steerAngle = aircraft ? aircraft.controls.current.yaw * maxSteerAngle : 0f;
        transform.localRotation = Quaternion.AngleAxis(steerAngle * invertAngle, axis);
    }

    const float freeWheelMaxForwardSpeed = 6f;
    private void FreeWheelFixedUpdate()
    {
        if (!wheel.grounded) return;

        Vector3 pointDir = rb.GetPointVelocity(tr.position);
        float groundSpeed = pointDir.magnitude;
        float forwardSpeed = Vector3.Dot(pointDir, tr.root.forward);

        if (groundSpeed < 0.05f) return;

        if (forwardSpeed < freeWheelMaxForwardSpeed)
        {
            pointDir = Vector3.ProjectOnPlane(pointDir, transform.up);
            float targetSteerAngle = Vector3.SignedAngle(transform.parent.forward, pointDir, transform.parent.TransformDirection(axis));
            steerAngle = Mathf.MoveTowards(steerAngle, targetSteerAngle, Time.fixedDeltaTime * groundSpeed * 160f);
        }
        else
        {
            steerAngle = Mathf.MoveTowards(steerAngle, 0f, Time.fixedDeltaTime * groundSpeed * 500f);
        }
        transform.localRotation = Quaternion.AngleAxis(steerAngle, axis);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(SteeringAxis)), CanEditMultipleObjects]
public class SteeringAxisEditor : SofComponentEditor
{

    private SerializedProperty axis;
    private SerializedProperty steeringControls;
    private SerializedProperty maxSteerAngle;


    protected override string BasicName()
    {
        return "Steering Axis";
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        axis = serializedObject.FindProperty("axis");
        steeringControls = serializedObject.FindProperty("steeringControls");
        maxSteerAngle = serializedObject.FindProperty("maxSteerAngle");
    }

    protected override void BasicFoldout()
    {
        base.BasicFoldout();

        SteeringAxis steeringAxis = (SteeringAxis)target;


        EditorGUILayout.PropertyField(axis);

        EditorGUILayout.PropertyField(steeringControls);

        if (steeringAxis.steeringControls == SteeringAxis.Controls.PedalsSteering)
        {
            EditorGUILayout.PropertyField(maxSteerAngle);
        }
    }
    protected void OnSceneGUI()
    {
        SteeringAxis steeringAxis = (SteeringAxis)target;

        Vector3 lowerPoint = steeringAxis.transform.position;
        Vector3 higherPoint = steeringAxis.transform.TransformPoint(steeringAxis.axis.normalized * 2f);

        Handles.color = Color.green;
        Handles.DrawLine(lowerPoint, higherPoint, 2f);
    }
}
#endif