using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class PistonEngine : Engine
{
    //References
    public Propeller propeller;

    //Data
    public float brakePower = 0f;
    public float boostTime;
    private float randomFrictionModifier;

    const float leakChance = 0.12f;

    public override float Mass() { return preset.weight; }
    public override float EmptyMass() { return preset.weight; }

    public float Power(float thr, bool boost, float radSec)
    {
        float basePower = preset.gear1.Evaluate(data.altitude) * 745.7f;
        float efficiency = preset.RpmPowerEffectiveness(radSec, boost) * Mathv.SmoothStart(structureDamage, 2);
        return basePower * (boost ? preset.Boost(data.altitude) : thr) * efficiency;
    }
    public override void Initialize(ObjectData obj, bool firstTime)
    {
        base.Initialize(obj, firstTime);
        if (firstTime)
        {
            waterCooled = !(preset.type == EnginePreset.Type.Radial);
            if (waterCooled) waterCircuit = new LiquidTank.LiquidCircuit(this, water, water.escapeSpeed / 2f);
            boostTime = preset.boostTime;
            propeller = GetComponentInChildren<Propeller>();
            randomFrictionModifier = Random.Range(0.5f, 2f);
        }
    }
    public override float ConsumptionRate()
    {
        return preset.ConsumptionCoeff(trueThrottle) * brakePower / 745.7f;
    }
    void FixedUpdate()
    {
        EngineFixedUpdate();
        if (!igniting)
        {
            float targetPower = Working() ? Power(trueThrottle, boosting, rps) : 0f;
            brakePower = Mathf.MoveTowards(brakePower, targetPower, Time.fixedDeltaTime * 1000000f);
            float torque = (rps != 0f) ? structureDamage * brakePower / rps : 0f;
            torque += propeller.torque + preset.Friction(Working(), ripped) * randomFrictionModifier;

            float rpsSpeedUp = torque / (propeller.reductionGear * propeller.MomentOfInertia);
            rps = Mathf.Clamp(rps + rpsSpeedUp * Time.fixedDeltaTime, 0f, preset.boostRPS);
        }
    }
    public override void Damage(float damage, float caliber, float fireCoeff)
    {
        base.Damage(damage, caliber, fireCoeff);
        if (Random.value < leakChance) oilCircuit.Damage(caliber);
        //if (Random.value < leakChance && waterCooled) waterCircuit.Damage(caliber);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PistonEngine))]
public class PistonEngineEditor : EngineEditor
{
    Color backgroundColor;
    //
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        serializedObject.Update();
        //
        PistonEngine engine = (PistonEngine)target;

        base.OnInspectorGUI();

        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Piston Engine Properties", MessageType.None);
        GUI.color = backgroundColor;


        engine.propeller = engine.GetComponentInChildren<Propeller>();
        if (engine.propeller && engine.preset)
        {
            GUILayout.Space(20f);
            EditorGUILayout.LabelField("Brake Power Sea", engine.preset.gear1.Evaluate(0f).ToString("0.0") + " Hp");
            EditorGUILayout.LabelField("At", (engine.preset.nominalRPS / (Mathf.PI / 30f)).ToString("0.0") + " Rpm");
        }
        else if (!engine.propeller)
        {
            GUI.color = Color.red;
            GUILayout.Space(20f);
            EditorGUILayout.HelpBox("Please create a propeller as child", MessageType.Warning);
        }

        if (engine.complex)
        {
            GUILayout.Space(20f);
            GUI.color = Color.cyan;
            EditorGUILayout.HelpBox("Engine Display", MessageType.None);
            GUI.color = backgroundColor;
            EditorGUILayout.LabelField("Brake Power", (engine.brakePower / 745.7).ToString("0.0") + " Hp");
            EditorGUILayout.LabelField("Engine Speed", (engine.rps / (Mathf.PI / 30f)).ToString("0.0") + " RPM");
        }

        GUILayout.Space(20f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Supercharger", MessageType.None);
        GUI.color = backgroundColor;

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed)
        {
            EditorUtility.SetDirty(engine);
            EditorSceneManager.MarkSceneDirty(engine.gameObject.scene);
        }
    }
}
#endif