using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class SofModule : SofComponent
{
    public float structureDamage { get; private set; }
    public bool ripped { get; private set; }

    public abstract ModuleArmorValues Armor { get; }
    public abstract float MaxHp { get; }
    public virtual bool Detachable => false;

    public event Action<float, float, float> OnProjectileDamage;
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
    }
    public virtual void ProjectileDamage(float hpDamage, float caliber, float fireCoeff)
    {
        DirectStructuralDamage(hpDamage / MaxHp);

        OnProjectileDamage?.Invoke(hpDamage, caliber, fireCoeff);
    }

    const float explosionCoeff = 500f;
    const float holeCoeff = 10f;

    public void ExplosionDamage(Vector3 center, float tnt)
    {
        float sqrDis = (center - transform.position).sqrMagnitude;
        if (tnt > sqrDis / 500f)
        {
            float dmg = explosionCoeff * tnt / sqrDis * UnityEngine.Random.Range(0.65f, 1.5f);
            float hole = dmg * holeCoeff;
            ProjectileDamage(dmg, hole, 0f);
        }
    }
    public virtual void Rip()
    {
        ripped = true;
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