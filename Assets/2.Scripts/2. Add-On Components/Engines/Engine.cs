using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System;


public enum EngineClass
{
    PistonEngine,
    JetEngine
}

public abstract class Engine : SofModule, IMassComponent, IDamageTick, IIgnitable
{
    [SerializeField] protected LiquidTank oil;
    [SerializeField] protected LiquidTank water;
    public LiquidTank OilTank => oil;
    public LiquidTank WaterTank => water;


    public bool pumped;

    //Engine state
    public abstract EngineClass Class { get; }
    public abstract EnginePreset Preset { get; }

    public bool OnInput { get; private set; }
    public CompleteThrottle Throttle { get; private set; }
    public bool Igniting { get; protected set; }
    public float RadPerSec { get; protected set; }
    public bool Working { get; private set; }
    public bool CanBeIgnited => RadPerSec < MinimumRps && Functional && !Igniting;
    public virtual bool BoostIsEffective => false;

    //Essential references
    private EngineTemperature temp;
    private Carburetor carburetor;
    private Circuit oilCircuit;
    private Circuit waterCircuit;


    public EngineTemperature Temp => temp;

    public float BurningChance =>  0.15f;
    public float MaxStructureDamageToBurn => 0.8f;
    public ParticleSystem BurningEffect => StaticReferences.Instance.engineFireEffect;
    public float EmptyMass => Preset ? Preset.Weight : 0f;
    public float LoadedMass => EmptyMass;
    public float RealMass => EmptyMass;
    public override ModuleArmorValues Armor => ModulesHPData.EngineArmor;
    public bool Ignitable => true;
    public float TrueThrottle => MinTrueThrottle + Throttle * (1f - MinTrueThrottle);

    public abstract float MinTrueThrottle { get; }
    public abstract float ConsumptionRate { get; }


    [HideInInspector] public Action<Engine> OnIgnition;


    public virtual void SetThrottle(float thr)
    {
        Throttle = new CompleteThrottle(thr);
    }
    public override void Initialize(SofComplex _complex)
    {
        pumped = false;
        Igniting = false;

        base.Initialize(_complex);

        oilCircuit = new Circuit(transform, oil);
        if (Preset.LiquidCooled) waterCircuit = new Circuit(transform, water);

        if (Preset.UsesCarburetor) carburetor = new Carburetor(this);
        temp = new EngineTemperature(this);

        OnProjectileDamage += OnDamageOilLeakChance;
    }

    public bool Functional => HasAircraft && !ripped;
    private bool FuelAvailable => !aircraft.fuel.Empty && (carburetor == null || carburetor.carburetorFlowing);

    public virtual float MinimumRps => Preset.NominalRadPerSec * 0.1f;
    protected abstract void UpdatePowerAndRPS(float dt);

    protected void FixedUpdate()
    {
        Working = Functional && FuelAvailable && !Igniting && OnInput && RadPerSec > MinimumRps;

        carburetor?.Update(Time.fixedDeltaTime);
        Temp.Update(Time.fixedDeltaTime);

        if(!Igniting) UpdatePowerAndRPS(Time.fixedDeltaTime);

        if (Working)
        {
            aircraft.fuel.Consume(ConsumptionRate, Time.fixedDeltaTime);
            OilFrictionDamage(Time.fixedDeltaTime);
        }
    }

    const float fullFrictionDps = 0.001f;
    private void OilFrictionDamage(float dt)
    {
        float frictionFactor = 1f - structureDamage * OilTank.FillRatio;
        if (frictionFactor > 0.3f)
            DirectStructuralDamage(frictionFactor * TrueThrottle * fullFrictionDps * dt);
    }
    public void SetOnInput(bool on)
    {
        OnInput = on;
    }

    public virtual bool SetAutomated(bool on, bool instant)
    {
        if (!Functional || Igniting || (on == OnInput && !instant)) return false;
        OnInput = on;

        if (instant)
        {
            RadPerSec = on ? Mathf.Lerp(Preset.IdleRadPerSec, Preset.NominalRadPerSec, Throttle) : 0f;
            Temp.SetTemperature(on ? Temp.EquilibrumTempMaxContinuous() : data.temperature.Get);
        }
        else
        {
            if (on)
            {
                pumped = true;
                TryIgnite();
            }
        }
        return true;
    }
    public void TryIgnite() 
    {
        if (Igniting) return;
        if (!pumped) return;
        if (!Functional) return;
        if (!OnInput) return;

        if (RadPerSec < MinimumRps) 
            StartCoroutine(Ignition());
    }
    public abstract IEnumerator Ignition();

    public override void Rip()
    {
        SetAutomated(false, false);
        RadPerSec /= 5f;
        base.Rip();
    }

    const float leakChance = 0.12f;
    public void OnDamageOilLeakChance(float damage, float caliber, float fireCoeff)
    {
        if (UnityEngine.Random.value < leakChance) oilCircuit.Damage(caliber);
    }
    public void DamageTick(float dt)
    {
        if (structureDamage >= 1f) return;

        oilCircuit.Leaking(dt);
        if (Preset.LiquidCooled) waterCircuit.Leaking(dt);
    }
}