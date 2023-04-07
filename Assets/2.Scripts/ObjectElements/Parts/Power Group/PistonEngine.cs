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
        float efficiency = preset.RpmPowerEffectiveness(radSec, boost) * Mathv.SmoothStart(Integrity, 2);
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
            float torque = (rps != 0f) ? Integrity * brakePower / rps : 0f;
            torque += propeller.torque + preset.Friction(Working(), ripped) * randomFrictionModifier;

            float rpsSpeedUp = torque / (propeller.reductionGear * propeller.MomentOfInertia);
            rps = Mathf.Clamp(rps + rpsSpeedUp * Time.fixedDeltaTime, 0f, preset.boostRPS);
        }
    }
    public override void KineticDamage(float damage, float caliber, float fireCoeff)
    {
        base.KineticDamage(damage, caliber, fireCoeff);
        if (Random.value < leakChance) oilCircuit.Damage(caliber);
        //if (Random.value < leakChance && waterCooled) waterCircuit.Damage(caliber);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PistonEngine))]
public class PistonEngineEditor : EngineEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PistonEngine engine = (PistonEngine)target;
        engine.propeller = engine.GetComponentInChildren<Propeller>();
        if (!engine.propeller)
        {
            GUI.color = Color.red;
            GUILayout.Space(20f);
            EditorGUILayout.HelpBox("Please create a propeller as child", MessageType.Warning);
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