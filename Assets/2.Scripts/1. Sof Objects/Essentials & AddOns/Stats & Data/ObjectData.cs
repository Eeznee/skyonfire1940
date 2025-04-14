using UnityEngine;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class ObjectData
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
    public Value<float> gsp, vsp, tas, signedTas, invertTas, ias;
    public Value<float> energy, relativeAltitude, altitude, heading;
    public Value<float> density, relativeDensity, temperature, pressure;
    public Value<float> pitchAngle, bankAngle, angleOfSlip, angleOfAttack, turnRate;
    public Value<float> groundEffect;
    public Value<bool> grounded;

    private void InitializeValues()
    {
        gsp = new Value<float>(() => { return rb.velocity.magnitude; }, this);
        vsp = new Value<float>(() => { return rb.velocity.y; }, this);
        tas = new Value<float>(() => { return gsp.Get; }, this);
        signedTas = new Value<float>(() => { return tas.Get * Mathf.Sign(Vector3.Dot(rb.transform.forward, rb.velocity)); }, this);
        invertTas = new Value<float>(() => { return 1f / tas.Get; }, this);
        ias = new Value<float>(() => { return tas.Get * Mathf.Sqrt(relativeDensity.Get); }, this);

        energy = new Value<float>(() => { return altitude.Get * -Physics.gravity.y + gsp.Get * gsp.Get * 0.5f; }, this);
        altitude = new Value<float>(() => { return tr.position.y; }, this);
        relativeAltitude = new Value<float>(() => { return Mathf.Max(altitude.Get - GameManager.map.HeightAtPoint(tr.position), 0.5f); }, this);
        heading = new Value<float>(() => { return tr.eulerAngles.y; }, this);

        density = new Value<float>(() => { return Aerodynamics.GetAirDensity(temperature.Get, pressure.Get); }, this);
        relativeDensity = new Value<float>(() => { return density.Get * Aerodynamics.invertSeaLvlDensity; }, this);
        temperature = new Value<float>(() => { return Aerodynamics.GetTemperature(altitude.Get); }, this);
        pressure = new Value<float>(() => { return Aerodynamics.GetPressure(altitude.Get); }, this);

        pitchAngle = new Value<float>(() => { return Vector3.Angle(tr.forward, Vector3.ProjectOnPlane(tr.forward, Vector3.up)) * Mathf.Sign(tr.forward.y); }, this);
        turnRate = new Value<float>(() => { return -tr.InverseTransformDirection(rb.angularVelocity).x * Mathf.Rad2Deg; }, this);
        bankAngle = new Value<float>(() =>
        {
            float angle = tr.eulerAngles.z;
            return (angle > 180f) ? angle - 360f : angle;
        }, this);
        angleOfSlip = new Value<float>(() => { return Vector3.SignedAngle(tr.forward, Vector3.ProjectOnPlane(rb.velocity, tr.up), tr.up); }, this);
        angleOfAttack = new Value<float>(() => { return tas.Get < 2f ? 0f : Vector3.SignedAngle(tr.forward, Vector3.ProjectOnPlane(rb.velocity, tr.right), tr.right); }, this);
        groundEffect = new Value<float>(() => { return aircraft ? Aerodynamics.GetGroundEffect(relativeAltitude.Get, aircraft.stats.wingSpan) : 1f; }, this);

        grounded = new Value<bool>(() => 
        {
            if (!aircraft) return true;

            if (ias.Get > aircraft.stats.MinTakeOffSpeedHalfFlaps) return false;
            return relativeAltitude.Get < 10f;

        }, this);

        prevVelFirstValue = false;
    }

    [SerializeField] private Transform tr;
    [SerializeField] private SofComplex complex;
    [SerializeField] private SofAircraft aircraft;
    [SerializeField] private Rigidbody rb;
    public ObjectData(SofComplex _complex)
    {
        tr = _complex.transform;
        complex = _complex;
        aircraft = _complex.aircraft;
        rb = _complex.rb;

        OnValuesReset = null;
        InitializeValues();

        complex.OnFixedUpdate += FixedUpdateData;
        complex.OnInitialize += OnInitialize;
    }
    private void OnInitialize()
    {
        if (rb) prevVel = rb.velocity;
    }
    void FixedUpdateData()
    {
        if (OnValuesReset != null) OnValuesReset();
        if (aircraft)
        {
            ComputeAcceleration();
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
        truegForce = acceleration.y * invertGravity + tr.up.y;
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
}
