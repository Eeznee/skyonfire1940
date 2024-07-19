using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


public class SofModule : SofPart      //Modules are parts with HP that can be destroyed
{

    public ModuleMaterial material;

    public float structureDamage { get; private set; }
    public bool ripped;

    private IgnitableExtension ignitableExtension;


    public virtual bool Detachable => false;
    public virtual float MaxHp => material.hp;
    public bool IsBurning => ignitableExtension ? ignitableExtension.burning : false;

    public override void Initialize(SofComplex _complex)
    {
        if (!material) Debug.LogError(name + " Has no material attached", this);
        base.Initialize(_complex);
        structureDamage = 1f;

        if (material.ignitable) ignitableExtension = gameObject.AddComponent<IgnitableExtension>();
    }

    public void Damage(float integrityDamage)
    {
        structureDamage -= integrityDamage;
        structureDamage = Mathf.Clamp01(structureDamage);

        if (structureDamage <= 0f && !ripped) Rip();
    }
    public virtual void SimpleDamage(float damage)
    {
        Damage(damage / MaxHp);
    }
    public virtual void KineticDamage(float damage, float caliber, float fireCoeff)
    {
        Damage(damage / MaxHp);

        ignitableExtension?.TryBurn(caliber, fireCoeff);
    }
    public virtual void BurnDamage(float damage)
    {
        Damage(damage);
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
    public virtual void Rip()
    {
        ripped = true;
    }

    public void Repair() { structureDamage = 1f; ripped = false; ignitableExtension?.StopBurning(); }
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