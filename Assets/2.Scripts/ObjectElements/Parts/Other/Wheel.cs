using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Wheel : Module
{
    private float brakeInput;
    private float steeringInput;

    public bool brake = false;
    public bool splitBraking = true;
    public float brakeTorque = 600f;

    public bool steering = false;
    public float maxSteerAngle = 30f;
    public float maxSteerSpeed = 60f / 3.6f;
    public ParticleSystem brakeEffect;

    private float radius;

    public WheelCollider wheel;
    Quaternion defaultLocalRot;
    Vector3 defaultLocalPos;
    Vector3 rootPos;
    MeshCollider meshCollider;

    private bool isGrounded;
    private float rpm;
    private bool wheelDisabled = false;
    private bool brakeFxPlaying = false;

    public override bool Detachable()
    {
        return true;
    }
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);

        if (firstTime)
        {
            radius = wheel.radius;
            meshCollider = GetComponent<MeshCollider>();
            if (brake) brakeEffect = Instantiate(brakeEffect, transform.position, Quaternion.identity, transform);
            defaultLocalRot = transform.parent.localRotation;
            defaultLocalPos = transform.localPosition;
            rootPos = transform.root.InverseTransformPoint(transform.position);
        }
        if (meshCollider) meshCollider.isTrigger = !ripped;
    }

    void FixedUpdate()
    {
        if (!wheel || Time.timeScale == 0f) return;

        rpm = wheelDisabled ? Mathf.Max(0f,rpm - Time.fixedDeltaTime * 10f) : wheel.rpm;
        isGrounded = wheel.isGrounded;

        if (steering && aircraft)
        {
            float speedEff = Mathf.InverseLerp(maxSteerSpeed, 0f, data.gsp);
            steeringInput = aircraft.controlInput.y * maxSteerAngle * -Mathf.Sign(rootPos.z) * speedEff;
            wheel.steerAngle = Mathf.MoveTowards(wheel.steerAngle,steeringInput, Time.fixedDeltaTime * 2f);
            transform.parent.localRotation = defaultLocalRot;
            transform.parent.Rotate(Vector3.up * wheel.steerAngle);
        }

        wheel.motorTorque = isGrounded ? 1f : 0f;

        if (isGrounded)
        {
            wheel.GetGroundHit(out WheelHit hit);
            float force = hit.force;
            if (force > wheel.suspensionSpring.spring)
                Rip();

            if (!aircraft || !brake) return;
            //Input
            bool forcedBrake = data.gsp < 2f && ((int)aircraft.enginesState <= 1 || aircraft.throttle < 0.05f);

            brakeInput = splitBraking ? -Mathf.Sign(rootPos.x) * aircraft.controlInput.y : 0f;
            brakeInput = Mathf.Max(aircraft.brake, brakeInput);
            if (forcedBrake) brakeInput = 1f;
            if (wheel.radius == 0f) brakeInput = 0f;

            wheel.brakeTorque = brakeInput * brakeTorque;
            rb.AddTorque(-transform.root.right * wheel.brakeTorque);
        }
    }
    private void Update()
    {
        if (aircraft && aircraft.gear && rootPos.x != 0f)
        {
            bool newWheelDisabled = aircraft.gear.state < 0.8f;
            if (newWheelDisabled != wheelDisabled) {  wheelDisabled = newWheelDisabled; wheel.radius = wheelDisabled? 0f: radius; }
        }
        //Roll
        /*
        Vector3 targetPos = transform.parent.TransformPoint(defaultLocalPos);
        if (wheel.isGrounded)
            wheel.GetWorldPose(out targetPos, out Quaternion quat);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, 0.02f * Time.deltaTime);
        */
        if (rpm > 1f) transform.Rotate(Vector3.right * rpm * 6f * Time.deltaTime * Mathf.Sign(Vector3.Dot(tr.right,tr.root.right)));

        if (!aircraft || !brake) return;
        //Effects
        bool effect = brakeInput > 0.1f && isGrounded && complex.lod.LOD() <= 1;
        if (effect != brakeFxPlaying)
        {
            brakeFxPlaying = effect;
            if (brakeFxPlaying) brakeEffect.Play();
            else brakeEffect.Stop();
        }
        return;
    }
    public override void Rip()
    {
        if (ripped) return;
        base.Rip();
        Detach();
        rb.angularVelocity = tr.right * wheel.rpm / 30f * Mathf.PI;
        Destroy(wheel.gameObject);
        Destroy(this);
    }
}

public class Steering : Module
{
    private float steeringInput;

    public bool steering = false;
    public float maxSteerAngle = 30f;
    public float maxSteerSpeed = 60f / 3.6f;

    public WheelCollider wheel;
    Quaternion defaultLocalRot;
    Vector3 rootPos;
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);

        if (firstTime)
        {
            defaultLocalRot = transform.parent.localRotation;
            rootPos = transform.root.InverseTransformPoint(transform.position);
        }
    }

    void FixedUpdate()
    {
        if (!wheel || Time.timeScale == 0f) return;

        if (steering && aircraft)
        {
            float speedEff = Mathf.InverseLerp(maxSteerSpeed, 0f, data.gsp);
            steeringInput = aircraft.controlInput.y * maxSteerAngle * -Mathf.Sign(rootPos.z) * speedEff;
            wheel.steerAngle = Mathf.MoveTowards(wheel.steerAngle, steeringInput, Time.fixedDeltaTime * 2f);
            transform.parent.localRotation = defaultLocalRot;
            transform.parent.Rotate(Vector3.up * wheel.steerAngle);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Wheel))]
public class WheelEditor : Editor
{
    Color backgroundColor;

    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        serializedObject.Update();
        //
        Wheel wheel = (Wheel)target;

        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Part Properties", MessageType.None);
        GUI.color = backgroundColor;

        //General
        wheel.emptyMass = EditorGUILayout.FloatField("Wheel Mass", wheel.emptyMass);
        wheel.material = EditorGUILayout.ObjectField("Part Material", wheel.material, typeof(ModuleMaterial), false) as ModuleMaterial;

        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Wheel Properties", MessageType.None);
        GUI.color = backgroundColor;
        wheel.wheel = EditorGUILayout.ObjectField("Wheel Collider", wheel.wheel, typeof(WheelCollider), true) as WheelCollider;
        if (!wheel.wheel)
            wheel.wheel = wheel.transform.parent.GetComponentInChildren<WheelCollider>();

        if (wheel.wheel)
        {
            wheel.wheel.mass = wheel.emptyMass;

            //Brakes
            wheel.brake = EditorGUILayout.Toggle("Can brake", wheel.brake);
            if (wheel.brake)
            {
                wheel.splitBraking = EditorGUILayout.Toggle("Can split brake", wheel.splitBraking);
                wheel.brakeTorque = EditorGUILayout.FloatField("Brake torque N.m", wheel.brakeTorque);
                wheel.brakeEffect = EditorGUILayout.ObjectField("Brake effect", wheel.brakeEffect, typeof(ParticleSystem), false) as ParticleSystem;
            }
            //Steer
            wheel.steering = EditorGUILayout.Toggle("Can steer", wheel.steering);
            if (wheel.steering)
            {
                wheel.maxSteerAngle = EditorGUILayout.FloatField("Max steering angle", wheel.maxSteerAngle);
                wheel.maxSteerSpeed = EditorGUILayout.FloatField("Max steer speed Km/h", wheel.maxSteerSpeed * 3.6f) / 3.6f;
            }
        }
        else
        {
            EditorGUILayout.HelpBox("You must attach a wheel collider", MessageType.Warning);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(wheel);
            EditorSceneManager.MarkSceneDirty(wheel.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
