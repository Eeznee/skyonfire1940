using UnityEngine;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class Value<T>
{
    private Func<T> Compute;
    private bool updated = false;
    private T value;

    public Value(Func<T> _Compute, ObjectData d)
    {
        Compute = _Compute;
        d.OnValuesReset += () => updated = false;
    }
    public T Get
    {
        get
        {
            if (!updated) { value = Compute(); updated = true; }
            return value;
        }
    }
}
public class ObjectData : MonoBehaviour
{
    //0 is detached part, 1 is simple, 2 is complex, 3 is aircraft
    public int type;

    //References
    public Transform tr;
    public Rigidbody rb;
    public SofObject sofObject;
    public SofSimple simple;
    public SofComplex complex;
    public SofAircraft aircraft;
    public Part[] parts;
    public Module[] modules;
    public AirframeBase[] airframes;

    public Action OnValuesReset;
    public Value<float> gsp,vsp,tas,invertTas,ias;
    public Value<float> energy,relativeAltitude,altitude,heading;
    public Value<float> density,relativeDensity,temperature,pressure;
    public Value<float> pitchAngle,bankAngle,angleOfSlip,angleOfAttack;
    public Value<float> groundEffect;
    public Value<Vector3> forward,up,right,position;

    private void InitializeValues()
    {
        gsp = new Value<float>(() => { return rb.velocity.magnitude; }, this);
        vsp = new Value<float>(() => { return rb.velocity.y; }, this);
        tas = new Value<float>(() => { return gsp.Get; }, this);
        invertTas = new Value<float>(() => { return 1f / tas.Get; }, this);
        ias = new Value<float>(() => { return tas.Get * Mathf.Sqrt(relativeDensity.Get); }, this);

        energy = new Value<float>(() => { return altitude.Get * -Physics.gravity.y + gsp.Get * gsp.Get * 0.5f; }, this);
        altitude = new Value<float>(() => { return tr.position.y; }, this);
        relativeAltitude = new Value<float>(() => { return Mathf.Max(altitude.Get - GameManager.map.HeightAtPoint(position.Get), 0.5f); }, this);
        heading = new Value<float>(() => { return tr.eulerAngles.y; }, this);

        density = new Value<float>(() => { return Aerodynamics.GetAirDensity(temperature.Get, pressure.Get); }, this);
        relativeDensity = new Value<float>(() => { return density.Get * Aerodynamics.invertSeaLvlDensity; }, this);
        temperature = new Value<float>(() => { return Aerodynamics.GetTemperature(altitude.Get); }, this);
        pressure = new Value<float>(() => { return Aerodynamics.GetPressure(altitude.Get); }, this);

        pitchAngle = new Value<float>(() => { return Vector3.Angle(forward.Get, Vector3.ProjectOnPlane(forward.Get, Vector3.up)) * Mathf.Sign(forward.Get.y); }, this);
        bankAngle = new Value<float>(() => {
            float angle = tr.eulerAngles.z;
            return (angle > 180f) ? angle - 360f : angle;
        }, this);
        angleOfSlip = new Value<float>(() => { return Vector3.SignedAngle(forward.Get, Vector3.ProjectOnPlane(rb.velocity, up.Get), up.Get); }, this);
        angleOfAttack = new Value<float>(() => { return Vector3.SignedAngle(forward.Get, Vector3.ProjectOnPlane(rb.velocity, right.Get), right.Get); }, this);
        groundEffect = new Value<float>(() => { return aircraft ? Aerodynamics.GetGroundEffect(relativeAltitude.Get, aircraft.wingSpan) : 1f; }, this);

        forward = new Value<Vector3>(() => { return tr.forward; }, this);
        up = new Value<Vector3>(() => { return tr.up; }, this);
        right = new Value<Vector3>(() => { return tr.right; }, this);
        position = new Value<Vector3>(() => { return tr.position; }, this);
    }

    private Mass mass;
    public float GetMass() { return mass.mass; }
    public Vector3 GetCenterOfMass() { return mass.center; }
    public void ShiftMass(float shift) { mass.mass += shift; UpdateRbMass(false); }
    public void ShiftMass(Mass shift) { mass += shift; UpdateRbMass(true); }
    public void UpdateRbMass(bool centerOfMass) { rb.mass = mass.mass; if (centerOfMass) rb.centerOfMass = mass.center; }
    public void Initialize(bool firstTime)
    {
        OnValuesReset = null;
        InitializeValues();

        tr = transform;
        sofObject = GetComponent<SofObject>();
        simple = GetComponent<SofSimple>();
        complex = GetComponent<SofComplex>();
        aircraft = GetComponent<SofAircraft>();
        type = aircraft ? 3 : complex ? 2 : simple ? 1 : 0;

        if (type >= 2) gameObject.layer = 9;
        else gameObject.layer = 0;
        
        rb = (type == 1) ? GameManager.gm.mapRb : this.GetCreateComponent<Rigidbody>();

        foreach (ObjectElement element in GetComponentsInChildren<ObjectElement>())
        {
            element.gameObject.layer = 9;
            element.Initialize(this, firstTime);
        }

        if (type == 1) return;
        rb.angularDrag = 0f;
        rb.drag = 0f;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = aircraft ? RigidbodyInterpolation.Extrapolate : RigidbodyInterpolation.Extrapolate;

        parts = GetComponentsInChildren<Part>();
        modules = GetComponentsInChildren<Module>();
        airframes = GetComponentsInChildren<AirframeBase>();

        mass = new Mass(parts, false);
        UpdateRbMass(true);
        Vector3 inertiaTensor = Mass.InertiaMoment(parts, true);
        if (type == 3 || type == 0) rb.inertiaTensor = inertiaTensor;
    }

    private void Start()
    {
        prevVel = rb.velocity;
    }
    void FixedUpdate()
    {
        OnValuesReset();
        if (type == 0 || type == 3)
            WaterPhysics();
        if (type == 3)
        {
            ComputeAcceleration();
            foreach (AirframeBase airframe in airframes)
                airframe.ForcesStress();
        }
    }
    protected Vector3 prevVel = Vector3.zero;
    public Vector3 acceleration = Vector3.zero;
    public float truegForce = 0f;
    public float gForce = 1f;
    private float totalG = 0f;
    private float gForceCounter = 0f;
    const float gInterval = 0.3f;

    const float invertGravity = 1f / 9.81f;
    protected void ComputeAcceleration()
    {
        //Acceleration
        acceleration = (rb.velocity - prevVel) * TimeManager.invertFixedDelta;
        acceleration = tr.InverseTransformDirection(acceleration);
        truegForce = acceleration.y * invertGravity + up.Get.y;
        prevVel = rb.velocity;

        //Average G forces
        totalG += Time.fixedDeltaTime * truegForce;
        gForceCounter += Time.fixedDeltaTime;
        if (gForceCounter > gInterval)
        {
            gForce = totalG / gForceCounter;
            gForceCounter = totalG = 0f;
        }
    }

    private bool submerged = false;
    protected void WaterPhysics()
    {
        if (altitude.Get < 5f) foreach (AirframeBase airframe in airframes) if (airframe) airframe.Floating();

        bool newSubmerged = altitude.Get < 1f;
        if (newSubmerged != submerged)
        {
            submerged = newSubmerged;
            rb.angularDrag = submerged ? 0.5f : 0f;
            rb.drag = submerged ? 1f : 0f;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ObjectData))]
public class ObjectDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        base.OnInspectorGUI();

        ObjectData data = (ObjectData)target;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(data);
            EditorSceneManager.MarkSceneDirty(data.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
