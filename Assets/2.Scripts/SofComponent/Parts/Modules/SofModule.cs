using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public abstract class SofModule : SofComponent
{
    public float structureDamage { get; private set; }
    public bool ripped { get; private set; }

    public abstract ModuleArmorValues Armor { get; }
    public abstract float MaxHp { get; }
    public virtual bool Detachable => false;

    public event Action<float, float, float> OnProjectileDamage;
    public event Action<float> OnDirectDamage;
    public event Action<SofModule> OnRip;
    public event Action OnRepair;
    

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        structureDamage = 1f;

        IIgnitable iIgnitable = GetComponent<IIgnitable>();
        if (iIgnitable != null && iIgnitable.Ignitable) gameObject.AddComponent<IgnitableExtension>();
    }

    public void DirectStructuralDamage(float integrityDamage)
    {
        structureDamage = Mathf.Clamp01(structureDamage - integrityDamage);

        if (structureDamage <= 0f && !ripped) Rip();

        OnDirectDamage?.Invoke(integrityDamage);
    }
    public virtual void ProjectileDamage(float hpDamage, float caliber, float fireCoeff)
    {
        DirectStructuralDamage(hpDamage / MaxHp);

        OnProjectileDamage?.Invoke(hpDamage, caliber, fireCoeff);
    }

    const float holeCoeff = 2f;
    const float damageAtMaxRange = 1f;
    const float minDamageForHole = 10f;

    public void ExplosionDamage(Vector3 center, float tnt)
    {
        float squaredMaxDamageRange = Ballistics.MaxExplosionDamageRangeSqrt(tnt);
        float sqrDis = (center - transform.position).sqrMagnitude;
        if (sqrDis < squaredMaxDamageRange)
        {
            float dmg = damageAtMaxRange * squaredMaxDamageRange / sqrDis * UnityEngine.Random.Range(0.5f, 2f);
            float hole = 0f;
            if(dmg > minDamageForHole) hole = dmg * holeCoeff;
            ProjectileDamage(dmg, hole, 0f);
        }
    }
    public virtual void Rip()
    {
        ripped = true;
        OnRip?.Invoke(this);
    }

    public void Repair() { structureDamage = 1f; ripped = false; OnRepair?.Invoke(); }
}
#if UNITY_EDITOR
[CustomEditor(typeof(SofModule)), CanEditMultipleObjects]
public class ModuleEditor : SofComponentEditor
{
    protected override void OnEnable()
    {
        base.OnEnable();
    }
    protected override string BasicName()
    {
        return "Module";
    }
    protected override void BasicFoldout()
    {
        base.BasicFoldout();

        SofModule module = (SofModule)target;

        EditorGUILayout.LabelField("HP", module.MaxHp.ToString("0") + " HP");
    }
}
#endif