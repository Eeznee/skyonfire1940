using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class DoubleAxisTurret : Turret
{
    //Traverse settings
    public Transform traversor;
    public bool limitedTraverse = false;
    public Vector2 traverseConstrains = new Vector2(-60f, 60f);
    public float traverseRate = 30.0f;
    protected float traverseAngle = 0f;

    //Elevation settings
    public Transform elevator;
    public float elevationRate = 30f;
    public float elevationConstrain = 90f;
    public AnimationCurve depressionConstrain = AnimationCurve.Constant(0f, 360f, 0f);
    public AnimationCurve depressionConstrainAnimated = AnimationCurve.Constant(0f, 360f, 0f);
    protected float elevationAngle = 0f;


    protected const float ArcSize = 10.0f;

    public override float TargetAvailability(Vector3 pos)
    {
        Vector3 localDir = transform.InverseTransformPoint(pos).normalized;
        return Mathf.InverseLerp(-0.5f,0f,localDir.y);
    }

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        traverseAngle = traversor.localRotation.eulerAngles.y;
        elevationAngle = -elevator.localEulerAngles.x;
    }

    protected virtual float MinDepression(float traverseAngle)
    {
        traverseAngle = (traverseAngle + 360f) % 360f;
        if (animatedConstrains) { return Mathf.Lerp(depressionConstrain.Evaluate(traverseAngle), depressionConstrainAnimated.Evaluate(traverseAngle), animationFactor); }
        return depressionConstrain.Evaluate(traverseAngle);
    }

    protected override void BasicAxisInput()
    {
        //Traverse Calculations
        bool forceElevation = false;
        float traverseOffset = traverseRate * Time.deltaTime * basicAxis.x;
        float elevationNeeded = MinDepression(traverseAngle + traverseOffset) - elevationAngle;
        if (elevationNeeded > 0f) //If traverse blocks
        {
            forceElevation = true;
            traverseOffset *= Mathf.Min(elevationRate / 2f * Time.deltaTime / elevationNeeded, 1f);
        }
        traverseAngle = (traverseAngle + traverseOffset) % 360f;
        if (limitedTraverse && Mathf.Abs(traverseConstrains.x - traverseConstrains.y) < 359.5f) traverseAngle = Mathf.Clamp(traverseAngle, traverseConstrains.x, traverseConstrains.y);

        //Elevation Calculations
        float elevationOffset = elevationRate * Time.deltaTime * basicAxis.y;
        if (forceElevation) elevationOffset = elevationRate / 2f * Time.deltaTime;
        elevationAngle += elevationOffset;
        elevationAngle = Mathf.Clamp(elevationAngle, MinDepression(traverseAngle), elevationConstrain);

        //Apply
        traversor.localEulerAngles = Vector3.up * traverseAngle;
        elevator.localEulerAngles = -Vector3.right * elevationAngle;
    }
    protected override void BasicAxisTarget()
    {
        bool forceElevation = false;
        //Traverse
        Vector3 traverseAim = Vector3.ProjectOnPlane(targetDirection, traversor.up);
        float targetAngleOffset = Vector3.SignedAngle(traversor.forward, traverseAim, traversor.up);
        targetAngleOffset = Mathf.Sign(targetAngleOffset) * Mathf.Min(traverseRate * Time.deltaTime, Mathf.Abs(targetAngleOffset));
        float elevationNeeded = MinDepression(traverseAngle + targetAngleOffset) - elevationAngle;
        if (elevationNeeded > 0f) //If traverse blocks
        {
            forceElevation = true;
            targetAngleOffset *= Mathf.Min(elevationRate / 2f * Time.deltaTime / elevationNeeded, 1f);
        }
        traverseAngle += targetAngleOffset;
        if (limitedTraverse && Mathf.Abs(traverseConstrains.x - traverseConstrains.y) < 359.5f) traverseAngle = Mathf.Clamp(traverseAngle, traverseConstrains.x, traverseConstrains.y);

        //Elevation Calculations
        Vector3 elevatorAim = Vector3.ProjectOnPlane(targetDirection, elevator.right);
        targetAngleOffset = Vector3.SignedAngle(elevator.forward, elevatorAim, -elevator.right);
        targetAngleOffset = Mathf.Sign(targetAngleOffset) * Mathf.Min(elevationRate * Time.deltaTime, Mathf.Abs(targetAngleOffset));
        if (forceElevation) targetAngleOffset = elevationRate / 2f * Time.deltaTime;

        elevationAngle = (elevationAngle + targetAngleOffset) % 360f;
        elevationAngle = Mathf.Clamp(elevationAngle, MinDepression(traverseAngle), elevationConstrain);

        traversor.localEulerAngles = Vector3.up * traverseAngle;
        elevator.localEulerAngles = -Vector3.right * elevationAngle;
    }

#if UNITY_EDITOR
    //GIZMOS
    public void OnDrawGizmos()
    {
        if (elevator && traversor)
        {
            for (int i = 0; i < 36; i++)
            {
                Handles.color = new Color(0.1f, 1f, 0.1f, 0.3f);
                Vector3 dir = Quaternion.AngleAxis(i * 10f, elevator.up) * Quaternion.AngleAxis(MinDepression(i * 10f - transform.localRotation.eulerAngles.y), -elevator.right) * elevator.forward;
                Handles.DrawLine(elevator.position + dir * 0.1f, elevator.position + dir * 2f);
            }
        }
    }
#endif
}
#if UNITY_EDITOR

[CustomEditor(typeof(DoubleAxisTurret))]
public class DoubleAxisTurretEditor : TurretEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        DoubleAxisTurret turret = (DoubleAxisTurret)target;

        turret.traversor = EditorGUILayout.ObjectField("Traversor Transform", turret.traversor, typeof(Transform), true) as Transform;
        turret.elevator = EditorGUILayout.ObjectField("Elevator Transform", turret.elevator, typeof(Transform), true) as Transform;

        GUI.color = Color.green;
        EditorGUILayout.HelpBox("Traverse settings", MessageType.None);
        GUI.color = GUI.backgroundColor;
        turret.traverseRate = EditorGUILayout.FloatField("Traverse rate deg/s", turret.traverseRate);
        turret.limitedTraverse = EditorGUILayout.Toggle("Limited Traverse", turret.limitedTraverse);
        if (turret.limitedTraverse)
            turret.traverseConstrains = EditorGUILayout.Vector2Field("Left/Right Constrains Degrees", turret.traverseConstrains);

        GUI.color = Color.red;
        EditorGUILayout.HelpBox("Elevation settings", MessageType.None);
        GUI.color = GUI.backgroundColor;
        turret.elevationRate = EditorGUILayout.FloatField("Elevation rate deg/s", turret.elevationRate);
        turret.elevationConstrain = EditorGUILayout.FloatField("Elevation Constrain", turret.elevationConstrain);
        turret.depressionConstrain = EditorGUILayout.CurveField("Depression by traverse", turret.depressionConstrain);
        if (turret.animatedConstrains)
        {
            turret.depressionConstrainAnimated = EditorGUILayout.CurveField("Depression by traverse Animated", turret.depressionConstrainAnimated);
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