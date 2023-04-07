using UnityEngine;
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
    public Module[] modules;
    public AirframeBase[] airframes;
    public Rigidbody rb;
    public Weather weather;
    public Mass mass;


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
    public Transform tr;
    public Vector3 forward;
    public Vector3 up;
    public Vector3 right;
    public float relativeAltitude = 5f;
    public float altitude = 5f;
    public Vector3 position = Vector3.zero;
    public float airDensity = 1.3f;
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
        tr = transform;
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
        parts = GetComponentsInChildren<Part>();
        modules = GetComponentsInChildren<Module>();
        airframes = GetComponentsInChildren<AirframeBase>();

        foreach (ObjectElement element in GetComponentsInChildren<ObjectElement>())
            element.Initialize(this, firstTime);

        if (type == 1) return;
        rb.angularDrag = 0f;
        rb.drag = 0f;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = aircraft ? RigidbodyInterpolation.Extrapolate : RigidbodyInterpolation.Extrapolate;
        /*
        foreach (ObjectElement element in GetComponentsInChildren<ObjectElement>())
        {
            element.gameObject.layer = 9;
            element.Initialize(this, false);
        }
        */
        //New parts might have been created
        modules = GetComponentsInChildren<Module>();
        parts = GetComponentsInChildren<Part>();
        mass = new Mass(parts, false);

        if (type == 3) rb.inertiaTensor = Mass.InertiaMoment(parts, true);
    }

    private void Start()
    {
        prevVel = rb.velocity;
        prevAng = rb.angularVelocity;
    }
    void FixedUpdate()
    {
        if (type == 2 || type == 0) ComplexData();
        if (type == 3) AircraftData();


        if (type == 3)
        {
            if (altitude < 5f) foreach (AirframeBase airframe in airframes) airframe.Floating();
            foreach (AirframeBase airframe in airframes)
                airframe.ForcesStress();
        }
    }

    protected void ComplexData()
    {
        rb.mass = mass.mass;
        rb.centerOfMass = mass.center;
        //Direction
        position = tr.position;
        altitude = Mathf.Max(position.y, 0f);
        relativeAltitude = Mathf.Max(altitude - GameManager.map.HeightAtPoint(position), 0.5f);

        gsp = rb.velocity.magnitude;
        tas = gsp;
    }
    protected void AircraftData()
    {
        ComplexData();

        ambientTemperature = Aerodynamics.GetTemperature(altitude);
        ambientPressure = Aerodynamics.GetPressure(altitude);
        airDensity = Aerodynamics.GetAirDensity(ambientTemperature, ambientPressure);

        //Directions
        forward = tr.forward;
        up = tr.up;
        right = tr.right;
        pitchAngle = Vector3.Angle(forward, Vector3.ProjectOnPlane(forward, Vector3.up));
        if (forward.y < 0f) pitchAngle *= -1f;

        headingDirection = tr.eulerAngles.y;
        bankAngle = tr.eulerAngles.z;
        bankAngle = (bankAngle > 180f) ? bankAngle - 360f : bankAngle;
        Vector3 slipVelocity = Vector3.ProjectOnPlane(rb.velocity, up);
        Vector3 attackVelocity = Vector3.ProjectOnPlane(rb.velocity, right);
        angleOfSlip = Vector3.SignedAngle(forward, slipVelocity, up);
        angleOfAttack = Vector3.SignedAngle(forward, attackVelocity, right);
        bool submerged = altitude < 1f;
        rb.angularDrag = submerged ? 0.5f : 0f;
        rb.drag = submerged ? 1f : 0f;

        //Speed
        ias = tas * Mathf.Sqrt(airDensity / Aerodynamics.seaLvlDensity);
        energy = altitude * -Physics.gravity.y + gsp * gsp * 0.5f;

        //Acceleration
        acceleration = (rb.velocity - prevVel) * TimeManager.invertFixedDelta;
        acceleration = tr.InverseTransformDirection(acceleration);
        truegForce = acceleration.y / 9.81f + up.y;
        prevVel = rb.velocity;

        //Average G forces
        totalG += Time.fixedDeltaTime * truegForce;
        gForceCounter += Time.fixedDeltaTime;
        if (gForceCounter > gInterval)
        {
            gForce = totalG / gForceCounter;
            gForceCounter = totalG = 0f;
        }

        //Angular acceleration
        angularAcceleration = (rb.angularVelocity - prevAng) * TimeManager.invertFixedDelta;
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