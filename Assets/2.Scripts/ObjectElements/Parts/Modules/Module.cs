using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class Module : Part       //Parts are Object Elements with mass
{
    //References
    public ParticleSystem burningEffect;
    protected AudioSource burningAudio;

    //Damage model
    public ModuleMaterial material;
    public float maxHp;
    private float structureDamage = 1f;
    public bool ripped;
    public bool burning;

    public float Integrity { get { return structureDamage; } }

    public virtual float StructureIntegrity() { return Mathf.Max(structureDamage, 0f); }

    public virtual bool Detachable()
    {
        return false;
    }
    public override void Initialize(ObjectData d, bool firstTime)
    {
        if (!material) Debug.LogError(material.name + " Has no material attached");

        base.Initialize(d, firstTime);
        if (firstTime) maxHp = material.hp;
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

    public virtual void Burning()
    {
        if (!burning) return;

        foreach (Module module in data.modules)
        {
            if (!module) continue;
            float distanceSqr = (module.transform.position - transform.position).sqrMagnitude;
            distanceSqr = Mathf.Max(distanceSqr, 4f);
            float damage = Time.fixedDeltaTime * 0.07f / distanceSqr;
            module.BurnDamage(damage);
        }
    }
    public virtual void Rip()
    {
        ripped = true;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Module))]
public class ModuleEditor : PartEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        Module module = (Module)target;

        module.material = EditorGUILayout.ObjectField("Part Material", module.material, typeof(ModuleMaterial), false) as ModuleMaterial;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(module);
            EditorSceneManager.MarkAllScenesDirty();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
