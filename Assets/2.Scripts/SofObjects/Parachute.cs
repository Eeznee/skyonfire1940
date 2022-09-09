using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Parachute : SofComplex
{
    public Vector3 dragPoint = new Vector3 (0f,4f,0f);
    public float feetGroundOffset = -1.6f;
    public float radius = 3f;

    private float rotationSpeed;
    private float trueRadius;
    private float dragCoeff = 1.75f;
    private bool grounded;

    const float terminalVelocity = 7f;

    private void Start()
    {
        rotationSpeed = Random.Range(-2f, 2f);
        dragCoeff = Random.Range(0.9f,1.1f) * 1600f / (terminalVelocity * terminalVelocity * data.seaLevelAirDensity * radius * radius * Mathf.PI);
        data.rb.centerOfMass = Vector3.zero;
        data.rb.angularDrag = 1f;
        data.rb.inertiaTensor = new Vector3(data.rb.mass, data.rb.mass * 4f * Random.Range(0.8f,1.2f), data.rb.mass);
        trueRadius = 0f;
        grounded = false;
    }
    private void FixedUpdate()
    {
        Vector3 vel = data.rb.GetPointVelocity(transform.TransformPoint(dragPoint));
        trueRadius = Mathf.MoveTowards(trueRadius, radius, Time.fixedDeltaTime / 2f);
        float area = trueRadius * trueRadius * Mathf.PI;
        data.rb.AddForceAtPosition(Aerodynamics.ComputeDrag(vel,data.tas, data.airDensity, area, dragCoeff, 1f),transform.TransformPoint(dragPoint));
        if (data.relativeAltitude < -feetGroundOffset && !grounded) Ground();
    }
    private void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);
    }
    private void Ground()
    {
        if (grounded) return;
        grounded = true;
        data.rb.isKinematic = true;
        GetComponent<Animator>().SetBool("Grounded",true);
        Vector3 pos = transform.position;
        pos.y = data.altitude - data.relativeAltitude - feetGroundOffset;
        transform.position = pos;
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Parachute))]
public class ParachuteEditor : SofComplexEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Parachute para = (Parachute)target;
        para.radius = EditorGUILayout.FloatField("Parachute Radius",para.radius);
        para.dragPoint = EditorGUILayout.Vector3Field("Parachute Drag Point", para.dragPoint);
        para.feetGroundOffset = EditorGUILayout.FloatField("Feet ground offset", para.feetGroundOffset);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(para);
            EditorSceneManager.MarkSceneDirty(para.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
