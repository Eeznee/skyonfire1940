using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System;
public class Engine : SofModule
{
    public enum EnginesState
    {
        Destroyed,
        Off,
        On
    }

    public EnginePreset preset;

    public bool igniting = false;
    public float ignitionState;
    public bool pumped;

    public float rps = 0f;

    public bool onInput;
    public float throttleInput;
    public float trueThrottle;
    public bool boosting;

    public Carburetor carburetor;
    public EngineTemp temp;

    public LiquidTank oil;
    public Circuit oilCircuit;
    public LiquidTank water;
    public Circuit waterCircuit;

    public Action OnIgnition;
    public Action OnTurnOff;

    public override float EmptyMass() { return preset.weight; }
    public override float Mass() { return preset.weight; }
    public override void Initialize(SofComplex _complex)
    {
        material = preset.material;
        base.Initialize(_complex);

        oilCircuit = new Circuit(transform, oil);

        carburetor = new Carburetor(this);
        temp = new EngineTemp(this);
    }
    public float Volume()
    {
        if (Working()) return 1f;
        if (igniting) return rps / preset.idleRPS;
        return 0f;
    }
    public bool Functional() { return aircraft && !ripped && !aircraft.fuelSystem.Empty; }
    public bool Working() { return !igniting && onInput && Functional() && rps > preset.idleRPS * 0.5f && carburetor.working; }
    public virtual float ConsumptionRate() { return 0f; } //Unit : kg/h

    public void EngineFixedUpdate()
    {
        boosting = preset.type != EnginePreset.Type.Jet && Working() && aircraft.boost;
        trueThrottle = Working() ? Mathf.Clamp01((throttleInput + 0.005f) / 1.005f) : 0f;

        carburetor.Update(Time.fixedDeltaTime);
        temp.Update(Time.fixedDeltaTime);

        if (!Working()) return;

        aircraft.fuelSystem.Consume(ConsumptionRate(), Time.fixedDeltaTime);
    }
    public override void DamageTick(float dt)
    {
        base.DamageTick(dt);

        if (StructureIntegrity() < 1f)
        {
            oilCircuit.Leaking(dt);
            if (preset.WaterCooled()) waterCircuit.Leaking(dt);
            Burning(dt);
        }
    }
    public void Set(bool on, bool instant)
    {
        if (ripped || igniting || (on == onInput)) return;
        onInput = on;

        if (instant)
        {
            rps = on ? Mathf.Lerp(preset.idleRPS, preset.nominalRPS, throttleInput) : 0f;
            temp.SetTemperature(on ? temp.EquilibrumTemp(throttleInput) : data.temperature.Get);
        }
        else if (on) { pumped = true; TryIgnite(); }
    }
    public void TryIgnite() { if (!igniting && rps < preset.idleRPS * 0.5f && pumped && Functional() && onInput) StartCoroutine(Ignition()); }
    public IEnumerator Ignition()
    {
        igniting = true;
        ignitionState = 0f;

        float timeCount = 0f;
        float randomDelay = UnityEngine.Random.Range(0f, 1.5f);
        while (timeCount < randomDelay)
        {
            timeCount += Time.deltaTime;
            yield return null;
        }

        OnIgnition?.Invoke();
        timeCount = 0f;
        float startRps = rps;

        while (timeCount < preset.ignitionTime)
        {
            ignitionState = timeCount / preset.ignitionTime;
            rps = Mathf.Lerp(startRps, preset.idleRPS, Mathv.SmoothStep(ignitionState, 4));
            timeCount += Time.deltaTime;
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
    SerializedProperty preset;
    SerializedProperty oil;
    SerializedProperty water;
    protected virtual void OnEnable()
    {
        preset = serializedObject.FindProperty("preset");
        oil = serializedObject.FindProperty("oil");
        water = serializedObject.FindProperty("water");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Engine engine = (Engine)target;
        EditorGUILayout.PropertyField(preset);

        if (engine.preset)
        {
            EditorGUILayout.PropertyField(oil, new GUIContent("Oil Tank"));
            bool liquidCooled = engine.preset.type == EnginePreset.Type.V || engine.preset.type == EnginePreset.Type.Inverted;
            if (liquidCooled) EditorGUILayout.PropertyField(water, new GUIContent("Water Tank"));

            EditorGUILayout.LabelField("Dry Weight", engine.preset.weight.ToString("0.0") + " Kg");
        }
        else
        {
            GUI.color = Color.red;
            GUILayout.Space(20f);
            EditorGUILayout.HelpBox("Please assign an engine preset", MessageType.Warning);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif