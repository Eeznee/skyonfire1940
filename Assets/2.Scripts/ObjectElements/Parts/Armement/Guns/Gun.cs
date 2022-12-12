using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Gun : Part
{
    //Settings
    public Transform ejection;
    public Transform muzzleEffects;
    public Transform bulletSpawn;
    public GunPreset gunPreset;
    public MagazineStock magStock;
    public Magazine magazine;
    public int clipAmmo;
    public Vector3 magazineLocalPos;
    public BoltHandle bolt;
    public bool noConvergeance = false;

    //References
    protected AmmunitionPreset ammunition;
    protected GameObject bubble;
    protected Projectile[] bullets;

    //Data
    public delegate void CycleEvent();
    public CycleEvent OnFireEvent;
    public CycleEvent OnEjectEvent;
    public CycleEvent OnChamberEvent;
    public CycleEvent OnTriggerEvent;
    public float temperature;

    public int clips;
    protected int currentBullet = 0;
    public float fuzeDistance = 0f;
    public bool reloading = false;

    public float cycleState = 1f;
    protected bool chambered;
    protected bool ejected;
    protected bool lockedBolt = true;
    protected bool trigger = false;
    protected bool reset = true;
    protected bool blockedBolt = false;
    protected float lastFired = 0f;

    private float invertFireRate = 1f;

    const float critTemperature = 450f;
    const float absoluteTemperature = 800f;
    //const float maxDispersionTemperature = 550f;

    public override float EmptyMass()
    {
        return gunPreset.mass;
    }
    public override float Mass()
    {
        return EmptyMass() + (magazine ? 0f : gunPreset.ammunition.FullMass * clipAmmo);
    }
    public static int AmmunitionCount(Gun[] guns)
    {
        int total = 0;
        foreach (Gun gun in guns) total += gun.AmmoCount();
        return total;
    }
    public int AmmoCount()
    {
        int a = chambered && !gunPreset.openBolt ? 1 : 0;
        if (magazine) a += magazine.ammo;
        return a;
    }
    public bool PossibleFire()
    {
        bool shotReady = magazine && magazine.ammo > 0 && !(!chambered && cycleState == 1f);
        return (shotReady && data.type > 0 && transform.position.y > 0f) || Firing();
    }
    public bool Firing()
    {
        return Time.time - lastFired <= 60f * invertFireRate + 0.1f;
    }
    public Vector3 MagazinePosition()
    {
        return transform.TransformPoint(magazineLocalPos);
    }
    public override void Initialize(ObjectData d, bool firstTime)
    {
        material = gunPreset.material;
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            ammunition = gunPreset.ammunition;
            bullets = new Projectile[ammunition.bullets.Length];
            for (int i = 0; i < bullets.Length; i++)
                bullets[i] = gunPreset.ammunition.CreateProjectile(i, transform);

            if (!magazine) //Create a hidden magazine if there is none available
            {
                magazine = new GameObject(gunPreset.name + " Magazine").AddComponent<Magazine>();
                magazine.capacity = clipAmmo;
                magazine.Initialize(data, true);
                magazine.gunPreset = gunPreset;
            }
            if (!muzzleEffects) muzzleEffects = transform;
            if (!bulletSpawn) bulletSpawn = transform;

            cycleState = gunPreset.openBolt ? 0.5f : 1f;
            chambered = !gunPreset.openBolt;
            ejected = true;

            currentBullet = Random.Range(0, ammunition.defaultBelt.Length);
            temperature = data.ambientTemperature;
            emptyMass = gunPreset.mass;

            LoadMagazine(magazine);
            chambered = true;

            OnFireEvent += LaunchBullet;
            OnFireEvent += HeatUp;
            OnFireEvent += Recoil;
            OnFireEvent += OnFire;
            OnChamberEvent += OnChamber;
            OnEjectEvent += OnEject;
            OnEjectEvent += TryLockBolt;
            OnTriggerEvent += OnTrigger;

            fuzeDistance = 0f;
            invertFireRate = 1f / gunPreset.FireRate;

            gameObject.AddComponent<GunFX>();
        }
    }
    //Must be called each frame to fire
    public void Trigger()
    {
        if (reset)
            OnTriggerEvent();
        reset = false;
        trigger = true;
    }
    public virtual void FixedUpdate()
    {
        if (!lockedBolt && !blockedBolt) Cycle(cycleState + gunPreset.FireRate / 60f * Time.fixedDeltaTime);
        float delta = Mathf.Max(temperature - data.ambientTemperature, 150f);
        temperature = Mathf.MoveTowards(temperature, data.ambientTemperature, delta * gunPreset.coolingFactor * Time.fixedDeltaTime);

        if (!trigger) reset = true;
        else trigger = false;
        blockedBolt = false;
    }
    public void ManualCycle(float state)
    {
        Cycle(state);
        TryLockBolt();
        blockedBolt = true;
    }
    protected void Cycle(float state)
    {
        cycleState = state;
        do
        {
            if (cycleState >= 1f)
            {
                if (ejected) OnChamberEvent();
                bool autoFire = gunPreset.openBolt || (trigger && gunPreset.fullAuto);
                if (chambered && autoFire) OnFireEvent();
                else cycleState = 1f;
            }
            if (!ejected && cycleState >= 0.5f) OnEjectEvent();
        }
        while (cycleState >= 1f && !lockedBolt);
    }
    protected void OnTrigger()
    {
        if (gunPreset.openBolt && cycleState == 0.5f) lockedBolt = false;
        if (!gunPreset.openBolt && chambered && cycleState == 1f) OnFireEvent();
    }
    protected void OnChamber()
    {
        float jamChance = Mathv.SmoothStart(Mathf.InverseLerp(critTemperature, absoluteTemperature, temperature), 4);
        chambered = magazine && magazine.EjectRound() && Random.value > jamChance;
        if (chambered) currentBullet = (currentBullet + 1) % ammunition.defaultBelt.Length;
        lockedBolt = true;
    }
    protected void OnEject()
    {
        ejected = true;
    }
    protected void TryLockBolt()
    {
        //bool catchBolt = magazine.ammo <= 0 && gunPreset.boltCatch;
        bool openBoltStop = gunPreset.openBolt && (!trigger || !gunPreset.fullAuto);
        if (openBoltStop && cycleState >= 0.5f) { lockedBolt = true; cycleState = 0.5f; }
        else lockedBolt = false;
    }
    protected void OnFire()
    {
        cycleState -= 1f;
        ejected = false;
        chambered = false;
        lockedBolt = false;
        lastFired = Time.time;
    }
    protected void LaunchBullet()
    {
        Projectile bullet = Instantiate(bullets[ammunition.defaultBelt[currentBullet]], bulletSpawn.position, bulletSpawn.rotation);
        bullet.gameObject.SetActive(true);

        //float dispersion = gunPreset.dispersion * Mathf.Lerp(1f, gunPreset.overHeatDispersion, Mathv.SmoothStart(temperature / maxDispersionTemperature, 3));
        bullet.transform.rotation = Ballistics.Spread(bulletSpawn.rotation, gunPreset.dispersion);
        bullet.RaycastDamage(bullet.p.baseVelocity * bullet.transform.forward + rb.velocity, rb.velocity, 10f);
        bullet.transform.position += rb.velocity * Time.fixedDeltaTime;
        bullet.InitializeTrajectory(bullet.transform.forward * bullet.p.baseVelocity + rb.velocity, transform.forward, complex ? complex.bubble : null);
        if (fuzeDistance > 50f) bullet.StartFuze(fuzeDistance / bullet.p.baseVelocity);
    }
    protected void HeatUp()
    {
        temperature += gunPreset.temperaturePerShot;
    }
    protected void Recoil()
    {
        float energy = ammunition.mass * 2f * ammunition.defaultMuzzleVel;
        rb.AddForceAtPosition(-transform.forward * energy / rb.mass, transform.position, ForceMode.VelocityChange);
    }

    public void LoadMagazine(Magazine mag)
    {
        if (gunPreset == mag.gunPreset)
        {
            magazine = mag;
            magazine.attachedGun = this;
            magazine.transform.parent = transform;
            magazine.transform.localPosition = magazineLocalPos;
            magazine.transform.localRotation = Quaternion.identity;
        }
    }
    public void RemoveMagazine()
    {
        magazine.attachedGun = null;
        magazine.transform.parent = transform.root;
        magazine = null;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Gun)), CanEditMultipleObjects]
public class GunEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Gun gun = (Gun)target;

        //General settings
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("General Settings", MessageType.None);
        GUI.color = GUI.backgroundColor;

        gun.ejection = EditorGUILayout.ObjectField("Ejection Transform", gun.ejection, typeof(Transform), true) as Transform;
        gun.muzzleEffects = EditorGUILayout.ObjectField("Muzzle Effects Transform", gun.muzzleEffects, typeof(Transform), true) as Transform;
        gun.bulletSpawn = EditorGUILayout.ObjectField("Bullet Spawn Transform", gun.bulletSpawn, typeof(Transform), true) as Transform;
        gun.gunPreset = EditorGUILayout.ObjectField("Gun Preset", gun.gunPreset, typeof(GunPreset), false) as GunPreset;
        gun.noConvergeance = EditorGUILayout.Toggle("No Gun Auto Convergeance", gun.noConvergeance);
        gun.magazine = EditorGUILayout.ObjectField("Magazine", gun.magazine, typeof(Magazine), true) as Magazine;

        if (!gun.magazine)
        {
            EditorGUILayout.HelpBox("The gun is belt fed", MessageType.None);
            gun.clipAmmo = EditorGUILayout.IntField("Default Ammo Count", gun.clipAmmo);
        }
        else
        {
            EditorGUILayout.HelpBox("The gun is magazine fed", MessageType.None);
            gun.magStock = EditorGUILayout.ObjectField("Magazine Stock", gun.magStock, typeof(MagazineStock), true) as MagazineStock;
            gun.magazineLocalPos = EditorGUILayout.Vector3Field("Mag Local Pos", gun.magazineLocalPos);
        }

        //Weight settings
        if (gun.gunPreset)
        {
            GUI.color = Color.green;
            EditorGUILayout.HelpBox("Weight Infos", MessageType.None);
            GUI.color = GUI.backgroundColor;
            gun.emptyMass = gun.gunPreset.mass;
            EditorGUILayout.LabelField("Gun Mass", gun.EmptyMass().ToString("0.0") + " Kg");
            EditorGUILayout.LabelField("Ammo Mass", (gun.Mass() - gun.EmptyMass()).ToString("0.0") + " Kg");
            EditorGUILayout.LabelField("Total Mass", gun.Mass().ToString("0.0") + " Kg");
        }
        else
        {
            GUI.color = Color.red;
            EditorGUILayout.HelpBox("Please assign a gun and ammo belt preset", MessageType.Warning);
        }
        if (GUI.changed)
        {
            EditorUtility.SetDirty(gun);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif