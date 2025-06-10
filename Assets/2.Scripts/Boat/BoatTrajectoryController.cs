using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[RequireComponent(typeof(BoatMovement))]
public class BoatTrajectoryController : MonoBehaviour
{
    public Vector2[] targetPoints;


    private BoatMovement boatMovement;

    private PID turnPID;
    private float pointsRadius;
    private float pointSqrDistanceTrigger;

    private int currentPointId;

    void Start()
    {
        boatMovement = GetComponent<BoatMovement>();

        pointsRadius = boatMovement.TurnRadius();
        pointSqrDistanceTrigger = pointsRadius * pointsRadius;

        float x = 0.5f * M.Pow(boatMovement.maxAngularSpeed, 2) / boatMovement.angularAcceleration;

        turnPID = new PID(new Vector3(1f / x, 0f, 0f));

        currentPointId = 0;
    }
    void Update()
    {
        Vector2 currentPos = new Vector2(transform.position.x - GameManager.refPos.x, transform.position.z - GameManager.refPos.z);
        Vector2 targetPos = targetPoints[currentPointId];

        float sqrDistance = (targetPos - currentPos).sqrMagnitude;

        if (sqrDistance < pointSqrDistanceTrigger) currentPointId = (currentPointId + 1) % targetPoints.Length;


        Vector2 currentDir = new Vector2(transform.forward.x, transform.forward.z);
        Vector2 targetDir = targetPos - currentPos;

        float angle = Vector2.SignedAngle(currentDir, targetDir);

        boatMovement.throttleInput = 1f;
        boatMovement.turnInput = turnPID.Update(-angle, Time.deltaTime);
    }

    public Vector3 PointToWorldPos(int id)
    {
        return new Vector3(targetPoints[id].x, 0f, targetPoints[id].y);
    }
    public Vector2 CurrentTarget()
    {
        return targetPoints[currentPointId];
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.yellow;
        if (targetPoints == null) return;
        for (int i = 0; i < targetPoints.Length; i++)
        {
            Vector3 p1 = PointToWorldPos(i);
            Vector3 p2 = PointToWorldPos((i + 1) % targetPoints.Length);

            if (Application.isPlaying)
            {
                p1 += GameManager.refPos;
                p2 += GameManager.refPos;
            }
            Handles.DrawLine(p1, p2);
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(BoatTrajectoryController))]
public class BoatTrajectoryControllerEditor : Editor
{
    SerializedProperty targetPoints;

    void OnEnable()
    {
        targetPoints = serializedObject.FindProperty("targetPoints");
    }
    public override void OnInspectorGUI()
    {
        BoatTrajectoryController gun = (BoatTrajectoryController)target;

        serializedObject.Update();

        EditorGUILayout.PropertyField(targetPoints);

        GUI.color = editPosition ? new Color(0.5f, 0.5f, 0.5f, 5f) : GUI.backgroundColor;
        if (GUILayout.Button(editPosition ? "Editing Positions" : "Edit Positions"))
        {
            editPosition = !editPosition;

            if (!editPosition) Tools.current = previousTool;
        }

        serializedObject.ApplyModifiedProperties();
    }
    static Tool previousTool;
    static bool editPosition;

    protected virtual void OnSceneGUI()
    {
        BoatTrajectoryController traj = (BoatTrajectoryController)target;
        Transform tr = traj.transform;

        if (traj.targetPoints == null) return;
        if (!editPosition) return;
        if (Tools.current != Tool.None) previousTool = Tools.current;
        Tools.current = Tool.None;

        Vector3[] pos = new Vector3[traj.targetPoints.Length];// traj.targetPoints;

        for(int i = 0; i < traj.targetPoints.Length; i++)
        {
            Vector2 targetPoint = traj.targetPoints[i];
            pos[i] = new Vector3(targetPoint.x,0f, targetPoint.y);

            Handles.Label(pos[i] + SceneView.lastActiveSceneView.rotation * Vector3.down * 0.3f, "Point " + i);
        }

        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < traj.targetPoints.Length; i++)
        {
            pos[i] = Handles.PositionHandle(pos[i], tr.rotation);
        }


        if (EditorGUI.EndChangeCheck())
        {
            for (int i = 0; i < traj.targetPoints.Length; i++)
            {
                traj.targetPoints[i] = new Vector2(pos[i].x, pos[i].z); 
            }
        }

    }
    protected void OnDisable()
    {
        if (editPosition) Tools.current = previousTool;
    }
}
#endif