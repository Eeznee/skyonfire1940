﻿using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class ObjectData : MonoBehaviour
{
    //0 is detached part, 1 is simple, 2 is complex, 3 is aircraft
    public int type;

    //References
    public SofObject sofObject;
    public SofSimple simple;
    public SofComplex complex;
    public SofAircraft aircraft;
    public Part[] parts;
    public Rigidbody rb;
    public Weather weather;
    public float mass = 3000f;


    //Speed
    public float gsp;
    public float tas;
    public float ias;
    public float VerticalSpeed { get { return rb.velocity.y; } }
    public float energy;
    public Vector3 acceleration = Vector3.zero;
    public Vector3 angularAcceleration = Vector3.zero;
    public float truegForce = 0f;
    public float gForce = 1f;
    float totalG = 0f;
    float gForceCounter = 0f;
    const float gInterval = 0.3f;

    protected Vector3 prevAng = Vector3.zero;
    protected Vector3 prevVel = Vector3.zero;

    //Air
    public float relativeAltitude = 5f;
    public float altitude = 5f;
    public float airDensity = 1.3f;
    public float seaLevelAirDensity = 1.3f;
    public float ambientTemperature = 20f;
    public float ambientPressure = 101325f;

    //Direction
    public float headingDirection = 0f;

    //Flying object specifics
    public float pitchAngle = 0f;
    public float bankAngle = 0f;
    public float angleOfSlip = 0f;
    public float angleOfAttack = 0f;
    public float groundEffect = 1f;

    public void Initialize(bool firstTime)
    {
        //Type
        sofObject = GetComponent<SofObject>();
        simple = GetComponent<SofSimple>();
        complex = GetComponent<SofComplex>();
        aircraft = GetComponent<SofAircraft>();
        if (!sofObject) type = 0;
        if (simple) type = 1;
        if (complex) type = 2;
        if (aircraft) type = 3;

        if (type >= 2) gameObject.layer = 9;
        else gameObject.layer = 0;

        weather = GameManager.weather;
        rb = (type == 1) ? GameManager.gm.mapRb : (GetComponent<Rigidbody>() ? GetComponent<Rigidbody>() : gameObject.AddComponent<Rigidbody>());
        ObjectElement[] elements = GetComponentsInChildren<ObjectElement>();
        parts = GetComponentsInChildren<Part>();

        //rb settings
        mass = 0f;
        foreach (ObjectElement element in elements)
        {
            element.gameObject.layer = 9;
            element.Initialize(this, firstTime);
            mass += element.Mass();
        }
        if (type == 1) return;
        rb.mass = mass;
        rb.angularDrag = 0f;
        rb.drag = 0f;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void Start()
    {
        prevVel = rb.velocity;
        prevAng = rb.angularVelocity;
    }
    void FixedUpdate()
    {
        if (type == 2 ||type == 0) ComplexData();
        if (type == 3) AircraftData();
    }

    protected void ComplexData()
    {
        rb.mass = mass;
        //Direction
        altitude = Mathf.Max(transform.position.y, 0f);
        relativeAltitude = Mathf.Max(altitude - GameManager.map.HeightAtPoint(transform.position),0.5f);
        headingDirection = rb.transform.eulerAngles.y;

        //Speed
        gsp = rb.velocity.magnitude;
        tas = gsp;
    }
    protected void AircraftData()
    {
        ComplexData();

        //Meteo
        ambientTemperature = Aerodynamics.GetTemperature(altitude, weather.localTemperature);
        ambientPressure = Aerodynamics.GetPressure(altitude, weather.localTemperature);
        airDensity = Aerodynamics.GetAirDensity(ambientTemperature, ambientPressure);
        seaLevelAirDensity = Aerodynamics.GetAirDensity(weather.localTemperature, Aerodynamics.SeaLvlPressure);

        //Directions
        pitchAngle = Vector3.Angle(transform.forward, Vector3.ProjectOnPlane(transform.forward, Vector3.up));
        if (transform.forward.y < 0f) pitchAngle *= -1f;

        bankAngle = transform.root.eulerAngles.z;
        bankAngle = (bankAngle > 180f) ? bankAngle - 360f : bankAngle;
        Vector3 slipVelocity = Vector3.ProjectOnPlane(rb.velocity, transform.up);
        Vector3 attackVelocity = Vector3.ProjectOnPlane(rb.velocity, transform.right);
        angleOfSlip = Vector3.SignedAngle(transform.forward, slipVelocity, transform.up);
        angleOfAttack = Vector3.SignedAngle(transform.forward, attackVelocity, transform.right);
        bool submerged = transform.position.y < 1f;
        rb.angularDrag = submerged ? 0.5f : 0f;
        rb.drag = submerged ? 1f : 0f;

        //Speed
        gsp = rb.velocity.magnitude;
        tas = gsp;
        ias = tas * Mathf.Sqrt(airDensity/seaLevelAirDensity);
        energy = altitude * -Physics.gravity.y + gsp * gsp / 2;

        //Acceleration
        acceleration = (rb.velocity - prevVel) / Time.fixedDeltaTime;
        acceleration = transform.InverseTransformDirection(acceleration);
        truegForce = acceleration.y / 9.81f + transform.up.y;
        prevVel = rb.velocity;

        //Average G forces
        totalG += Time.fixedDeltaTime * truegForce;
        gForceCounter += Time.fixedDeltaTime;
        if (gForceCounter > gInterval) {
            gForce = totalG / gForceCounter;
            gForceCounter = totalG = 0f;
        }

        //Angular acceleration
        angularAcceleration = (rb.angularVelocity - prevAng) / Time.fixedDeltaTime;
        prevAng = rb.angularVelocity;

        groundEffect = relativeAltitude / aircraft.wingSpan * Mathf.Sqrt(relativeAltitude / aircraft.wingSpan) * 33f;
        groundEffect = 1f / groundEffect + 1f;
    }
}
/*
#if UNITY_EDITOR
[CustomEditor(typeof(ObjectData))]
public class ObjectDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        ObjectData data = (ObjectData)target;
        data.Initialize(false);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(data);
            EditorSceneManager.MarkSceneDirty(data.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
*/