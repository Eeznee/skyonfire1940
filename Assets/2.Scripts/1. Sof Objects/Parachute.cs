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
    public CrewSeat seat;

    private float rotationSpeed;
    private float trueRadius;
    private float dragCoeff = 1.75f;
    private bool landed;

    const float terminalVelocity = 7f;

    private Vector3 startVelocity;
    private string startTag;

    public override void SetReferences()
    {
        base.SetReferences();
        
    }
    private void Start()
    {
        //Do not delete, or initialize will be called twice
    }
    public void TriggerParachute(SofAircraft aircraft, CrewMember _crew)
    {
        startVelocity = aircraft.rb.velocity + aircraft.transform.up * 5f;
        startTag = aircraft.tag;

        _crew.transform.parent = seat.transform.parent;

        GameInitialization();

        rb.velocity = startVelocity;

        _crew.seats = new List<CrewSeat>(new CrewSeat[] { seat });
        _crew.SwitchSeat(0);

        if (_crew == Player.crew)
        {
            if (GameManager.gm.vr) SofVrRig.instance.ResetView();
            //else PlayerCamera.ResetRotation(false);
            Player.Set(this);
        }
        aircraft.RemoveComponentRoot(_crew);


    }
    protected override void GameInitialization()
    {
        base.GameInitialization();

        rotationSpeed = Random.Range(-2f, 2f);
        dragCoeff = Random.Range(0.9f, 1.1f) * 1600f / (terminalVelocity * terminalVelocity * Aerodynamics.seaLvlDensity * radius * radius * Mathf.PI);
        trueRadius = 0f;
        landed = false;
        tag = startTag;
    }
    public override void ResetInertiaTensor()
    {
        rb.inertiaTensor = new Vector3(100f, 400f * Random.Range(0.8f, 1.2f), 100f);
    }
    protected override void SetRigidbody()
    {
        rb = this.GetCreateComponent<Rigidbody>();

        rb.centerOfMass = Vector3.zero;
        rb.angularDrag = 1f;
        rb.drag = 0f;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Extrapolate;
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        Vector3 vel = rb.GetPointVelocity(transform.TransformPoint(dragPoint));
        trueRadius = Mathf.MoveTowards(trueRadius, radius, Time.fixedDeltaTime * 0.5f);
        float area = trueRadius * trueRadius * Mathf.PI;
        rb.AddForceAtPosition(Aerodynamics.Drag(vel, data.density.Get, area, dragCoeff, 1f),transform.TransformPoint(dragPoint));
        if (data.relativeAltitude.Get < -feetGroundOffset && !landed) Land();

        transform.Rotate(0f, rotationSpeed * Time.fixedDeltaTime, 0f, Space.Self);
    }
    private void Land()
    {
        landed = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.isKinematic = true;

        Vector3 pos = transform.position;
        pos.y = data.altitude.Get - data.relativeAltitude.Get - feetGroundOffset;
        transform.position = pos;
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        if (data.altitude.Get - data.relativeAltitude.Get == 0f) GetComponent<Animator>().SetBool("Swimming", true);
        else GetComponent<Animator>().SetBool("Grounded", true);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Parachute))]
public class ParachuteEditor : SofModularEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Parachute para = (Parachute)target;
        para.seat = EditorGUILayout.ObjectField("Seat", para.seat, typeof(CrewSeat), true) as CrewSeat;
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
