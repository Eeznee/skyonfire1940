using UnityEngine;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public enum AircraftWorldState
{
    Flying,
    TakingOff
}
public class ObjectData : SofComponent
{
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

    public Action OnValuesReset;
    public Value<float> gsp, vsp, tas, invertTas, ias;
    public Value<float> energy, relativeAltitude, altitude, heading;
    public Value<float> density, relativeDensity, temperature, pressure;
    public Value<float> pitchAngle, bankAngle, angleOfSlip, angleOfAttack, turnRate;
    public Value<float> groundEffect;
    public Value<Vector3> forward, up, right, position;

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
        turnRate = new Value<float>(() => { return -tr.InverseTransformDirection(rb.angularVelocity).x; }, this);
        bankAngle = new Value<float>(() => {
            float angle = tr.eulerAngles.z;
            return (angle > 180f) ? angle - 360f : angle;
        }, this);
        angleOfSlip = new Value<float>(() => { return Vector3.SignedAngle(forward.Get, Vector3.ProjectOnPlane(rb.velocity, up.Get), up.Get); }, this);
        angleOfAttack = new Value<float>(() => { return tas.Get < 2f ? 0f : Vector3.SignedAngle(forward.Get, Vector3.ProjectOnPlane(rb.velocity, right.Get), right.Get); }, this);
        groundEffect = new Value<float>(() => { return aircraft ? Aerodynamics.GetGroundEffect(relativeAltitude.Get, aircraft.stats.wingSpan) : 1f; }, this);

        forward = new Value<Vector3>(() => { return tr.forward; }, this);
        up = new Value<Vector3>(() => { return tr.up; }, this);
        right = new Value<Vector3>(() => { return tr.right; }, this);
        position = new Value<Vector3>(() => { return tr.position; }, this);

        prevVelFirstValue = false;
    }
    public override void SetReferences(SofComplex _complex)
    {
        base.SetReferences(_complex);
        OnValuesReset = null;
        InitializeValues();
    }
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        if (rb) prevVel = rb.velocity;
    }
    void FixedUpdate()
    {
        if (OnValuesReset != null) OnValuesReset();
        if (aircraft)
        {
            ComputeAcceleration();
            ComputeWorldState();
        }
    }
    private bool prevVelFirstValue;
    protected Vector3 prevVel { get; private set; }
    public Vector3 acceleration { get; private set; }
    public float truegForce { get; private set; }
    public float gForce { get; private set; }
    private float totalG = 0f;
    private float gForceCounter = 0f;
    const float gInterval = 0.3f;

    const float invertGravity = 1f / 9.81f;
    protected void ComputeAcceleration()
    {
        if (!prevVelFirstValue)
        {
            prevVel = rb.velocity;
            prevVelFirstValue = true;
        }

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

    public bool grounded { get; private set; }
    protected void ComputeWorldState()
    {
        grounded = data.relativeAltitude.Get < 5f && data.gsp.Get < aircraft.cruiseSpeed * 0.5f;
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
