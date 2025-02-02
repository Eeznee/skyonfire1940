using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System;


public abstract class Engine : SofModule, IMassComponent, IDamageTick, IIgnitable
{

    [SerializeField] protected EnginePreset preset;
    [SerializeField] protected LiquidTank oil;
    [SerializeField] protected LiquidTank water;

    public EnginePreset Preset => preset;
    public LiquidTank OilTank => oil;
    public LiquidTank WaterTank => water;

    //public input
    public CompleteThrottle Throttle;
    public bool onInput;
    public bool pumped;

    //Engine state
    public bool igniting { get; private set; }
    public float radiansPerSeconds { get; protected set; }
    public bool workingAndRunning { get; private set; }

    //Essential references
    private EngineTemp temp;
    private Carburetor carburetor;
    private Circuit oilCircuit;
    private Circuit waterCircuit;


    public EngineTemp Temp => temp;

    public float BurningChance => EnginePreset.burningChance;
    public float MaxStructureDamageToBurn => 0.8f;
    public ParticleSystem BurningEffect => preset.burningEffect;
    public float EmptyMass => preset ? preset.weight : 0f;
    public float LoadedMass => EmptyMass;
    public float RealMass => EmptyMass;
    public override ModuleArmorValues Armor => ModulesHPData.EngineArmor;
    public bool Ignitable => true;
    public override float MaxHp => Preset ? Preset.MaxHP : 100f;


    public abstract float ConsumptionRate { get; }


    [HideInInspector] public Action OnIgnition;

    public override void Initialize(SofComplex _complex)
    {
        pumped = false;
        igniting = false;

        base.Initialize(_complex);

        oilCircuit = new Circuit(transform, oil);
        if (Preset.WaterCooled) waterCircuit = new Circuit(transform, water);

        if (Preset.UsesCarburetor) carburetor = new Carburetor(this);
        temp = new EngineTemp(this);

        OnProjectileDamage += OnDamageOilLeakChance;
    }

    public bool Functional => HasAircraft && !ripped;
    private bool FuelAvailable => !aircraft.fuel.Empty && (carburetor == null || carburetor.carburetorFlowing);

    public virtual float MinimumRps => preset.fullRps * 0.1f;
    protected abstract void UpdatePowerAndRPS(float dt);

    protected void FixedUpdate()
    {
        workingAndRunning = Functional && FuelAvailable && !igniting && onInput && radiansPerSeconds > MinimumRps;

        carburetor?.Update(Time.fixedDeltaTime);
        Temp.Update(Time.fixedDeltaTime);

        if(!igniting) UpdatePowerAndRPS(Time.fixedDeltaTime);

        if (workingAndRunning) aircraft.fuel.Consume(ConsumptionRate, Time.fixedDeltaTime);
    }
    public void Set(bool on, bool instant)
    {
        if (!Functional || igniting || (on == onInput)) return;
        onInput = on;

        if (instant)
        {
            radiansPerSeconds = on ? Mathf.Lerp(preset.idleRPS, preset.nominalRPS, Throttle) : 0f;
            Temp.SetTemperature(on ? Temp.EquilibrumTemp(Throttle) : data.temperature.Get);
        }
        else if (on) 
        {
            pumped = true; TryIgnite(); 
        }
    }
    public void TryIgnite() 
    {
        if (igniting) return;
        if (!pumped) return;
        if (!Functional) return;
        if (!onInput) return;

        if (radiansPerSeconds < MinimumRps) 
            StartCoroutine(Ignition());
    }
    public IEnumerator Ignition()
    {
        igniting = true;
        float ignitionState = 0f;

        float timeCount = 0f;
        float randomDelay = UnityEngine.Random.Range(0f, 1.5f);
        while (timeCount < randomDelay)
        {
            timeCount += Time.deltaTime;
            yield return null;
        }

        OnIgnition?.Invoke();
        timeCount = 0f;
        float startRps = radiansPerSeconds;

        while (timeCount < preset.ignitionTime)
        {
            ignitionState = timeCount / preset.ignitionTime;
            radiansPerSeconds = Mathf.Lerp(startRps, MinimumRps * 1.5f, Mathv.SmoothStep(ignitionState, 4));
            timeCount += Time.deltaTime;
            yield return null;
        }
        igniting = false;
    }
    public override void Rip()
    {
        Set(false, false);
        radiansPerSeconds /= 5f;
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
        if (preset.WaterCooled) waterCircuit.Leaking(dt);
    }
}