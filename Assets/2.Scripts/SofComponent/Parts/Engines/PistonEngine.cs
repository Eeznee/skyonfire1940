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
        float basePower = preset.gear1.Evaluate(data.altitude.Get) * 745.7f;
        float efficiency = preset.RpmPowerEffectiveness(radSec, boost) * Mathv.SmoothStart(Integrity, 2);
        return basePower * (boost ? preset.Boost(data.altitude.Get) : thr) * efficiency;
    }
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        propeller = GetComponentInChildren<Propeller>();
        if (preset.WaterCooled()) waterCircuit = new Circuit(transform, water);
        boostTime = preset.boostTime;
        randomFrictionModifier = Random.Range(0.5f, 2f);
    }
    public override float ConsumptionRate() { return preset.ConsumptionRate(trueThrottle,brakePower); }
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
[CustomEditor(typeof(PistonEngine)), CanEditMultipleObjects]
public class PistonEngineEditor : EngineEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PistonEngine engine = (PistonEngine)target;
        if (!engine.GetComponentInChildren<Propeller>())
        {
            GUI.color = Color.red;
            GUILayout.Space(20f);
            EditorGUILayout.HelpBox("You must create a propeller as a child of this engine", MessageType.Warning);
        }
    }
}
#endif