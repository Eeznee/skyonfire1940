using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class He111Turret : DoubleAxisTurret
{
    protected override float MinDepression(float traverseAngle)
    {
        return base.MinDepression(traverseAngle+secondTraverseAngle);
    }
    public Transform secondTraversor;
    public Vector2 secondTraverseConstrains = new Vector2(-60f, 60f);
    public float secondTraverseRate = 60f;
    protected float secondTraverseAngle = 0f;
    protected override void BasicAxisInput()
    {
        //Traverse Calculations
        bool forceElevation = false;
        float traverseOffset = secondTraverseRate * Time.deltaTime * basicAxis.x;
        float elevationNeeded = MinDepression(traverseAngle + traverseOffset) - elevationAngle;
        if (elevationNeeded > 0f) //If traverse blocks
        {
            forceElevation = true;
            traverseOffset *= Mathf.Min(elevationRate / 2f * Time.deltaTime / elevationNeeded, 1f);
        }
        secondTraverseAngle = (secondTraverseAngle + traverseOffset) % 360f;
        secondTraverseAngle = Mathf.Clamp(secondTraverseAngle, secondTraverseConstrains.x, secondTraverseConstrains.y);

        //Elevation Calculations
        float elevationOffset = elevationRate * Time.deltaTime * basicAxis.y;
        if (forceElevation) elevationOffset = elevationRate / 2f * Time.deltaTime;
        elevationAngle += elevationOffset;
        elevationAngle = Mathf.Clamp(elevationAngle, MinDepression(secondTraverseAngle), elevationConstrain);

        //Apply
        secondTraversor.localEulerAngles = Vector3.up * secondTraverseAngle;
        elevator.localEulerAngles = -Vector3.right * elevationAngle;
    }
    protected override void BasicAxisTarget()
    {
        bool forceElevation = false;
        //Traverse
        Vector3 traverseAim = Vector3.ProjectOnPlane(targetDirection, secondTraversor.up);
        float targetAngleOffset = Vector3.SignedAngle(secondTraversor.forward, traverseAim, secondTraversor.up);
        targetAngleOffset = Mathf.Sign(targetAngleOffset) * Mathf.Min(secondTraverseRate * Time.deltaTime, Mathf.Abs(targetAngleOffset));
        float elevationNeeded = MinDepression(traverseAngle + targetAngleOffset) - elevationAngle;
        if (elevationNeeded > 0f) //If traverse blocks
        {
            forceElevation = true;
            targetAngleOffset *= Mathf.Min(elevationRate / 2f * Time.deltaTime / elevationNeeded, 1f);
        }
        secondTraverseAngle += targetAngleOffset;
        secondTraverseAngle = Mathf.Clamp(secondTraverseAngle, secondTraverseConstrains.x, secondTraverseConstrains.y);

        //Elevation Calculations
        Vector3 elevatorAim = Vector3.ProjectOnPlane(targetDirection, elevator.right);
        targetAngleOffset = Vector3.SignedAngle(elevator.forward, elevatorAim, -elevator.right);
        targetAngleOffset = Mathf.Sign(targetAngleOffset) * Mathf.Min(elevationRate * Time.deltaTime, Mathf.Abs(targetAngleOffset));
        if (forceElevation) targetAngleOffset = elevationRate / 2f * Time.deltaTime;

        elevationAngle = (elevationAngle + targetAngleOffset) % 360f;
        elevationAngle = Mathf.Clamp(elevationAngle, MinDepression(traverseAngle), elevationConstrain);

        secondTraversor.localEulerAngles = Vector3.up * secondTraverseAngle;
        elevator.localEulerAngles = -Vector3.right * elevationAngle;
    }

    protected override void SpecialAxisInput()
    {
        //Traverse Calculations
        float traverseOffset = traverseRate * Time.deltaTime * specialAxis;
        traverseAngle = (traverseAngle + traverseOffset) % 360f;
        if (limitedTraverse && Mathf.Abs(traverseConstrains.x - traverseConstrains.y) < 359.5f) traverseAngle = Mathf.Clamp(traverseAngle, traverseConstrains.x, traverseConstrains.y);

        traversor.localEulerAngles = Vector3.up * traverseAngle;
    }

    protected override void SpecialAxisTarget()
    {
        Vector3 traverseAim = Vector3.ProjectOnPlane(targetDirection, traversor.up);
        float targetAngleOffset = Vector3.SignedAngle(traversor.forward, traverseAim, traversor.up);
        targetAngleOffset = Mathf.Sign(targetAngleOffset) * Mathf.Min(traverseRate * Time.deltaTime, Mathf.Abs(targetAngleOffset));
        traverseAngle += targetAngleOffset;
        if (limitedTraverse && Mathf.Abs(traverseConstrains.x - traverseConstrains.y) < 359.5f) traverseAngle = Mathf.Clamp(traverseAngle, traverseConstrains.x, traverseConstrains.y);

        traversor.localEulerAngles = Vector3.up * traverseAngle;
    }
}
#if UNITY_EDITOR

[CustomEditor(typeof(He111Turret))]
public class He111TurretEditor : DoubleAxisTurretEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        He111Turret turret = (He111Turret)target;

        GUI.color = Color.blue;
        EditorGUILayout.HelpBox("Second settings", MessageType.None);
        GUI.color = GUI.backgroundColor;
        turret.secondTraversor = EditorGUILayout.ObjectField("Second Traversor", turret.secondTraversor, typeof(Transform), true) as Transform;
        turret.secondTraverseRate = EditorGUILayout.FloatField("Traverse rate deg/s", turret.secondTraverseRate);
        turret.secondTraverseConstrains = EditorGUILayout.Vector2Field("Left/Right Constrains Degrees", turret.secondTraverseConstrains);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(turret);
            EditorSceneManager.MarkSceneDirty(turret.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif