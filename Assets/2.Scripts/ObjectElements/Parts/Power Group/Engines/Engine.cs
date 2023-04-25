using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Engine : Module
{
    public enum EnginesState
    {
        Destroyed,
        Off,
        On
    }
    public EnginePreset preset;

    //State
    public bool igniting = false;
    public bool pumped;
    public float carburetorState = 1f;
    public bool carburetorOk = true;
    public float rps = 0f;
    public float consumption = 0f;

    public float trueEngineVolume = 0f;

    //Input
    public bool onInput;
    public float throttleInput;
    public float trueThrottle;
    public bool boosting;

    //Temperature
    public float maximumTemperature;
    public float temperature;
    public float waterTemperature;
    public float oilTemperature;

    //Liquid tanks
    public LiquidTank oil;
    protected LiquidTank.LiquidCircuit oilCircuit;
    public bool waterCooled;
    public LiquidTank water;
    protected LiquidTank.LiquidCircuit waterCircuit;
    protected int currentTank = 0;

    //Temperature constants
    const float maxThrottleNoCoolant = 0.5f;
    const float minDpsOverHeat = 1f / 100f;
    const float dpsGrowthPerDegree = 1f / 600f;
    const float fullFrictionDps = 1f / 40f;

    public override float EmptyMass()
    {
        return preset.weight;
    }
    public override float Mass()
    {
        return preset.weight;
    }
    public override void Initialize(ObjectData obj, bool firstTime)
    {
        material = preset.material;
        base.Initialize(obj, firstTime);
        emptyMass = preset.weight;
        if (firstTime)
        {
            maximumTemperature = Mathf.LerpUnclamped(preset.tempIdle, preset.tempFull, (preset.boostValue * 3f + 1f) / 4f); //Max temperature is equilibrum temperature at 3/4 boost=
            oilCircuit = new LiquidTank.LiquidCircuit(this, oil, oil.escapeSpeed / 2f);
            currentTank = aircraft.fuelTanks.Length - 1;
        }
    }
    public bool Operational() { return aircraft && !igniting && !ripped && currentTank >= 0; }
    public virtual bool Working() { return onInput && Operational() && rps > preset.idleRPS * 0.5f && carburetorOk; }
    public virtual float ConsumptionRate() { return 0f; } //Unit : kg/h
    public virtual void EngineFixedUpdate()
    {
        //Input
        boosting = preset.type != EnginePreset.Type.Jet && Working() && aircraft.boost;
        trueThrottle = Mathf.Clamp01((throttleInput + 0.005f) / 1.005f);
        
        if (!igniting) trueEngineVolume = Mathf.MoveTowards(trueEngineVolume, Working() ? 1f : 0f, Time.fixedDeltaTime);
        if (!Working()) trueThrottle = 0f;

        //Carburetor
        if (preset.fuelMixer == EnginePreset.FuelMixerType.Carburettor) carburetorState = Mathf.MoveTowards(carburetorState, Mathf.Sign(data.gForce + 0.5f), Time.fixedDeltaTime);
        if (carburetorOk && carburetorState < 0f) VibrationsManager.SendVibrations(0.6f, 0.5f, aircraft);
        carburetorOk = carburetorState > 0f;

        //Consumption
        if (Working())
        {
            consumption = ConsumptionRate();
            aircraft.fuelTanks[currentTank].ConsumeControlled(consumption / 3600f * Time.fixedDeltaTime);

            if (aircraft.fuelTanks[currentTank].currentAmount <= 0f) currentTank--;
        }

        //Temperature
        float maxThrottle = Mathf.Lerp(maxThrottleNoCoolant, 1f, waterCooled ? waterCircuit.mainTank.fill : 1f);
        maxThrottle *= Mathf.Lerp(maxThrottleNoCoolant, 1f, oilCircuit.mainTank.fill);

        float equiTemperature = Mathf.LerpUnclamped(preset.tempIdle, preset.tempFull, Mathf.Clamp01(trueThrottle) / maxThrottle);
        if (!Working()) equiTemperature = data.temperature.Get;
        temperature = Thermodynamics.LerpTemperature(temperature, equiTemperature);
        if (waterCooled) waterTemperature = Thermodynamics.CoolantTemperature(preset.waterTemperature, water.fill, preset.tempFull, temperature);
        oilTemperature = Thermodynamics.CoolantTemperature(preset.oilTemperature, oil.fill, preset.tempFull, temperature);
        //Damage if temperature too high
        if (temperature > maximumTemperature && Working())
        {
            float damagePerSecond = minDpsOverHeat + dpsGrowthPerDegree * (temperature - maximumTemperature);
            SimpleDamage(damagePerSecond * Time.fixedDeltaTime);
        }
        //Friction damage
        float frictionFactor = 1f - Integrity * oil.fill;
        if (frictionFactor > 0.3f)
            SimpleDamage(frictionFactor * trueThrottle * fullFrictionDps * Time.fixedDeltaTime);

        //Leaks and destruction of engine
        oilCircuit.Leaking();
        if (waterCooled) waterCircuit.Leaking();
        Burning();
        if (temperature > maximumTemperature * 1.12f) Rip();
    }
    public void Set(bool on, bool instant)
    {
        if (ripped || igniting || (on == onInput)) return;
        onInput = on;

        if (instant)
        {
            rps = on ? Mathf.Lerp(preset.idleRPS, preset.nominalRPS, throttleInput) : 0f;
            temperature = on ? Mathf.Lerp(preset.tempIdle, preset.tempFull, throttleInput) : data.temperature.Get;
        }
        else if (on) { pumped = true; TryIgnite(); }
    }
    public void TryIgnite(){ if (rps <= preset.idleRPS / 2f && pumped && Operational() && onInput) StartCoroutine(Ignition()); }
    public IEnumerator Ignition()
    {
        float delay = Random.Range(0f, 1.5f);
        while (delay > 0f)
        {
            delay -= Time.deltaTime;
            yield return null;
        }
        igniting = true;
        delay = 0f;
        float fromRPS = rps;
        float startTemperature = temperature;

        while (delay < preset.ignitionTime)
        {
            temperature = Mathf.Max(temperature, Mathf.Lerp(startTemperature, preset.tempIdle, delay / preset.ignitionTime));
            float rpsFactor = Mathf.Sin(delay / preset.ignitionTime * Mathf.PI / 2f);
            rpsFactor = Mathv.SmoothStart(rpsFactor, 4);
            rps = Mathf.Lerp(fromRPS, preset.idleRPS, rpsFactor);
            trueEngineVolume = rpsFactor;
            delay += Time.deltaTime;
            yield return null;
        }
        igniting = false;
    }
    public override void Rip()
    {
        Set(false, false);
        base.Rip();
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Engine))]
public class EngineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Engine engine = (Engine)target;
        engine.preset = EditorGUILayout.ObjectField("Engine Preset", engine.preset, typeof(EnginePreset), false) as EnginePreset;

        if (engine.preset)
        {
            engine.oil = EditorGUILayout.ObjectField("Oil Tank", engine.oil, typeof(LiquidTank), true) as LiquidTank;
            if (engine.preset.type == EnginePreset.Type.V || engine.preset.type == EnginePreset.Type.Inverted) engine.water = EditorGUILayout.ObjectField("Water Tank", engine.water, typeof(LiquidTank), true) as LiquidTank;
            EditorGUILayout.LabelField("Dry Weight", engine.preset.weight.ToString("0.0") + " Kg");
        }
        else
        {
            GUI.color = Color.red;
            GUILayout.Space(20f);
            EditorGUILayout.HelpBox("Please assign a preset", MessageType.Warning);
        }

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed)
        {
            EditorUtility.SetDirty(engine);
            EditorSceneManager.MarkSceneDirty(engine.gameObject.scene);
        }
    }
}
#endif