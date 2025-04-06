using UnityEngine;
using System.Runtime.CompilerServices;
using System;
using UnityEditor;

[AddComponentMenu("Sof Components/Power Group/Propeller")]
public partial class Propeller : SofModule, IMassComponent, IAircraftForce
{
    public override ModuleArmorValues Armor => ModulesHPData.NoArmor;
    public override float MaxHp => 10f;
    public override int DefaultLayer()
    {
        return 2;
    }


    public enum PitchControl
    {
        Fixed,
        TwoPitch,
        ConstantSpeed
    }

    [SerializeField] private float mass = 150f;
    [SerializeField] private float radius = 1.5f;
    [SerializeField] private bool invertRotation = false;
    public float EmptyMass => Mathf.Max(mass,25f);
    public float LoadedMass => EmptyMass;
    public float RealMass => EmptyMass;
    public float Radius => radius;
    public float Diameter => radius * 2f;
    public float Area => radius * radius * Mathf.PI;
    public float MomentOfInertia => RealMass * M.Pow(radius, 2) * 0.2f;

    [SerializeField] private PitchControl pitchControl;
    [SerializeField] private float minPitch = 30f;
    [SerializeField] private float maxPitch = 60f;
    [SerializeField] private float efficiency = 0.85f;

    public PitchControl PitchControlMechanism => pitchControl;
    public bool CanBeFeathered => pitchControl == PitchControl.ConstantSpeed && maxPitch >= 80f;
    public float Efficiency => efficiency;



    public float InertiaWithGear => MomentOfInertia * ReductionGear;
    public float ReductionGear => engine.PistonPreset.PropellerReductionGear;
    public float OptimalRps => engine.Preset.NominalRadPerSec * ReductionGear;
    public float RadPerSec => engine.RadPerSec * ReductionGear;

    public float Torque { get; private set; }
    public float Thrust { get; private set; }
    public float BladeAngle { get; private set; }
    public PistonEngine engine { get; private set; }
    public float DragCoefficient { get; private set; }
    public float TwoPitchTrigger { get; private set; }
    public bool TwoPitchMode { get; private set; }

    public override void SetReferences(SofComplex _complex)
    {
        base.SetReferences(_complex);

        engine = GetComponentInParent<PistonEngine>();
    }
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        ComputeTorqueCoefficent();
        TwoPitchTrigger = TwoPitchAdvanceRatioTrigger();

        gameObject.layer = 2;
        tr.Rotate(Vector3.forward * UnityEngine.Random.value * 360f);
        SetBladeAngle(maxPitch);
    }
    void Update()
    {
        float rotation = Time.deltaTime * RadPerSec * 57.3f;
        if(invertRotation) rotation *= -1f;
        transform.Rotate(-Vector3.forward * rotation);
    }
    void FixedUpdate()
    {
        if (Time.timeScale == 0f || !aircraft) return;

        if (pitchControl == PitchControl.ConstantSpeed) ConstantSpeedPitchAdjust(Time.fixedDeltaTime);
        if (pitchControl == PitchControl.TwoPitch) TwoPitchAdjust(Time.fixedDeltaTime);

        Torque = CurrentMotionTorque();
        Thrust = EngineDrivenThrust();

        if (tr.position.y < radius) engine.Rip();
    }


    public ForceAtPoint SimulatePointForce(FlightConditions flightConditions)
    {
        if (float.IsNaN(Thrust)) return new ForceAtPoint(Vector3.zero, flightConditions.position);

        Vector3 direction = flightConditions.TransformWorldDir(tr.root.forward);
        Vector3 point = flightConditions.TransformWorldPos(tr.position);

        if (engine.Igniting) direction = Quaternion.AngleAxis(-30f, aircraft.tr.up) * direction;

        return new ForceAtPoint(direction * Thrust, point);
    }

    const float bladeAngleShiftSpeed = 8f;
    const float maxShiftSpeedAtRps = 30f;

    private void TwoPitchAdjust(float dt)
    {
        float advanceRatio = AdvanceRatio();

        if (TwoPitchMode && advanceRatio < TwoPitchTrigger * 0.98f)
        {
            TwoPitchMode = false;
            if(aircraft == Player.aircraft)
                Log.Print("Propeller Pitch : Low", "TwoPitchPropeller");
        }

        else if (!TwoPitchMode && advanceRatio > TwoPitchTrigger * 1.02f)
        {
            TwoPitchMode = true;
            if (aircraft == Player.aircraft)                
                Log.Print("Propeller Pitch : High", "TwoPitchPropeller");
        }


        float target = TwoPitchMode ? maxPitch : minPitch;
        SetBladeAngle(Mathf.MoveTowards(BladeAngle, target, bladeAngleShiftSpeed * dt));
    }

    private void ConstantSpeedPitchAdjust(float dt)
    {
        float targetRps = engine.PistonPreset.TargetRadPerSec(engine.Throttle, engine.RunMode, engine.SuperchargerSetting, data.altitude.Get);
        float rpsDelta = engine.RadPerSec - targetRps;
        float shiftSpeed = Mathf.Clamp01(Mathf.Abs(rpsDelta) / maxShiftSpeedAtRps) * bladeAngleShiftSpeed;

        float targetBladeAngle = rpsDelta > 0f ? maxPitch : minPitch;

        SetBladeAngle(Mathf.MoveTowards(BladeAngle, targetBladeAngle, shiftSpeed * dt));
    }
    public void EngineSetInstantBladesAngle(float tas, float engineRps)
    {
        float radPerSec = engineRps * ReductionGear;
        SetBladeAngle(Mathf.Clamp(OptimalBladeAngle(radPerSec, tas), minPitch, maxPitch));
    }
    private void SetBladeAngle(float bladeAngle)
    {
        if (PitchControlMechanism == PitchControl.Fixed) BladeAngle = maxPitch;
        else BladeAngle = Mathf.Clamp(bladeAngle, minPitch, maxPitch);
    }


    public override void Rip()
    {
        base.Rip();

        engine.Rip();

        if (complex.lod) complex.lod.UpdateMergedModel();
    }
    public override void DirectStructuralDamage(float integrityDamage)
    {
        //Propellers cannot take damage from explosions or bullets
    }
    public override void ExplosionDamage(Vector3 center, float tnt)
    {
        //Propellers cannot take damage from explosions or bullets
    }
    public override void ProjectileDamage(float hpDamage, float caliber, float fireCoeff)
    {
        //Propellers cannot take damage from explosions or bullets
    }

    private void OnTriggerEnter(Collider other) { Rip(); }
}