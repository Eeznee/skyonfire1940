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

    public abstract ModuleArmorValues Armor (Collider collider);
    public abstract float MaxHp { get; }
    public virtual bool Detachable => false;

    public event Action<float, float, float> OnProjectileOrExplosionDamage;
    public event Action<float> OnDamage;
    public event Action<SofModule> OnRip;
    public event Action OnRepair;

    protected virtual Collider MainCollider => colliderGetComponent;
    private Collider colliderGetComponent;

    public override void SetReferences(SofModular _complex)
    {
        base.SetReferences(_complex);

        colliderGetComponent = GetComponent<Collider>();
    }
    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);
        structureDamage = 1f;
        IIgnitable iIgnitable = GetComponent<IIgnitable>();
        if (iIgnitable != null && iIgnitable.Ignitable) gameObject.AddComponent<IgnitableExtension>();
    }

    public virtual void DirectStructuralDamage(float integrityDamage)
    {
        structureDamage = Mathf.Clamp01(structureDamage - integrityDamage);

        if (structureDamage <= 0f && !ripped) Rip();

        OnDamage?.Invoke(integrityDamage);
    }
    public virtual void ProjectileDamage(float hpDamage, float caliber, float fireCoeff, Collider colliderHit)
    {
        DirectStructuralDamage(hpDamage / MaxHp);

        OnProjectileOrExplosionDamage?.Invoke(hpDamage, caliber, fireCoeff);
    }

    const float tntDamageCoeff = 300f;
    const float holeCoeff = 0.1f;
    public virtual void ExplosionDamage(Vector3 explosionOrigin, float tnt, out float damage, out float hole)
    {
        damage = hole = 0f;
        Vector3 point = MainCollider ? MainCollider.ClosestPoint(explosionOrigin) : transform.position;

        float sqrDis = (explosionOrigin - point).sqrMagnitude;
        sqrDis = Mathf.Max(sqrDis, 2f);
        if (tnt * tntDamageCoeff > sqrDis)
        {
            damage = tntDamageCoeff * tnt / sqrDis * UnityEngine.Random.Range(0.7f, 1.5f);
            hole = damage * holeCoeff;

            DirectStructuralDamage(damage / MaxHp);
            OnProjectileOrExplosionDamage?.Invoke(damage, hole, 0f);
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