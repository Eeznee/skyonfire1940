using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class SofModule : SofPart       //Modules are parts with HP that can be destroyed
{
    //Damage model
    public ModuleMaterial material;
    protected float maxHp;
    public float structureDamage = 1f;
    public bool ripped;
    public bool burning;

    //References
    protected ParticleSystem burningEffect;
    protected AudioSource burningAudio;

    public float Integrity { get { return structureDamage; } }

    public virtual float StructureIntegrity() { return Mathf.Max(structureDamage, 0f); }
    public void Repair() { structureDamage = 1f; ripped = false; burning = false; }

    public virtual bool Detachable()
    {
        return false;
    }
    public override void Initialize(SofComplex _complex)
    {
        if (!material) Debug.LogError(name + " Has no material attached", this);
        base.Initialize(_complex);
        maxHp = material.hp;
    }

    public virtual void DamageTick(float dt) 
    { 

    }
    public void DamageIntegrity(float integrityDamage)
    {
        structureDamage -= integrityDamage;
        structureDamage = Mathf.Clamp01(structureDamage);

        if (structureDamage <= 0f && !ripped) Rip();
    }
    public virtual void SimpleDamage(float damage)
    {
        DamageIntegrity(damage / maxHp);
    }
    public virtual void KineticDamage(float damage, float caliber, float fireCoeff)
    {
        DamageIntegrity(damage / maxHp);

        TryBurn(caliber, fireCoeff);
    }
    public virtual void BurnDamage(float damage)
    {
        DamageIntegrity(damage);
    }

    const float explosionCoeff = 500f;
    const float holeCoeff = 10f;
    public virtual void ExplosionDamage(Vector3 center, float tnt)
    {
        float sqrDis = (center - transform.position).sqrMagnitude;
        if (tnt > sqrDis / 500f)
        {
            float dmg = explosionCoeff * tnt / sqrDis * Random.Range(0.65f, 1.5f);
            float hole = dmg * holeCoeff;
            KineticDamage(dmg, hole, 0f);
        }
    }
    public void TryBurn(float caliber, float fireCoeff)
    {
        if (!material.ignitable || burning || structureDamage > 0.8f) return;

        float roundCoeff = fireCoeff * caliber * caliber / 60f;
        float burnChance = (1f - Mathv.SmoothStart(structureDamage, 1)) * material.burningChance * roundCoeff;
        if (Random.value < burnChance)
        {
            burning = true;
            if (sofObject) sofObject.burning = true;

            burningEffect = Instantiate(material.burningEffect, transform);
            burningEffect.Play();

            burningAudio = burningEffect.GetComponent<AudioSource>();
            burningAudio.Play();
        }
    }

    public virtual void Burning(float dt)
    {
        if (!burning) return;

        foreach (SofModule module in complex.modules)
        {
            if (!module) continue;
            float distanceSqr = (module.transform.position - transform.position).sqrMagnitude;
            distanceSqr = Mathf.Max(distanceSqr, 4f);
            float damage = dt * 0.07f / distanceSqr;
            module.BurnDamage(damage);
        }
    }
    public virtual void Rip()
    {
        ripped = true;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(SofModule)), CanEditMultipleObjects]
public class ModuleEditor : PartEditor
{
    SerializedProperty material;

    protected override void OnEnable()
    {
        base.OnEnable();
        material = serializedObject.FindProperty("material");
    }
    protected override string BasicName()
    {
        return "Module";
    }
    protected override void BasicFoldout()
    {
        base.BasicFoldout();

        SofModule module = (SofModule)target;

        EditorGUILayout.PropertyField(material);
        if (module.material)
        {
            EditorGUILayout.LabelField("HP", module.material.hp.ToString("0") + " HP");
        }
    }
}
#endif