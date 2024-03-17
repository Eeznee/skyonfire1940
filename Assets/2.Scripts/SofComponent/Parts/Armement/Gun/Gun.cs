using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public enum GunController
{
    Gunner,
    PilotPrimary,
    PilotSecondary,
}
public class Gun : SofPart
{
    public static Gun[] FilterByController(GunController controller, Gun[] guns)
    {
        int counter = 0;
        foreach (Gun gun in guns) if (gun && gun.controller == controller) counter++;
        Gun[] filtered = new Gun[counter];

        counter = 0;
        foreach (Gun gun in guns) if (gun && gun.controller == controller) { filtered[counter] = gun; counter++; }

        return filtered;
    }
    //Settings
    public GunController controller = GunController.Gunner;
    public Transform ejection;
    public Transform muzzleEffects;
    public Transform bulletSpawn;
    public Transform magazineAttachPoint;
    public GunPreset gunPreset;
    public MagazineStorage magStorage;
    public AmmoContainer magazine;
    public int clipAmmo;
    public BoltHandle bolt;
    public bool noConvergeance = false;

    private GunTrigger trigger;
    [HideInInspector] public GunMechanism mechanism;

    //References
    private AmmunitionPreset ammunition;
    private Projectile[] bullets;

    //Data
    public delegate void CycleEvent();
    public delegate void CycleDelayedEvent(float delay);
    public CycleDelayedEvent OnFireEvent;
    public CycleEvent OnTriggerEvent;
    public CycleEvent OnEjectEvent;
    public CycleEvent OnLockOpenEvent;
    public CycleEvent OnChamberRoundEvent;
    public CycleEvent OnSlamChamberEvent;
    public float temperature;

    public int clips;
    private int currentBullet = 0;
    public float fuzeDistance = 0f;
    public bool reloading = false;

    const float critTemperature = 450f;
    const float absoluteTemperature = 800f;
    //const float maxDispersionTemperature = 550f;

    public override float EmptyMass() { return gunPreset.mass; }
    public override float Mass() { return !Application.isPlaying && !magazine ? gunPreset.mass + clipAmmo * gunPreset.ammunition.FullMass : gunPreset.mass; }

    public override void Rearm()
    {
        base.Rearm();
        mechanism.Reset();
    }
    public override void Initialize(SofComplex _complex)
    {
        trigger = this.GetCreateComponent<GunTrigger>();
        mechanism = this.GetCreateComponent<GunMechanism>();

        if (!muzzleEffects) muzzleEffects = transform;
        if (!bulletSpawn) bulletSpawn = transform;
        if (!magazineAttachPoint) magazineAttachPoint = transform;

        base.Initialize(_complex);

        ammunition = gunPreset.ammunition;
        bullets = new Projectile[ammunition.bullets.Length];
        for (int i = 0; i < bullets.Length; i++)
            bullets[i] = gunPreset.ammunition.CreateProjectile(i, transform);

        currentBullet = Random.Range(0, ammunition.defaultBelt.Length);

        if (!magazine) magazine = AmmoContainer.CreateAmmoBelt(this, clipAmmo, sofObject);

        OnFireEvent += LaunchBullet;
        OnFireEvent += RecoilAndHeatup;
        OnChamberRoundEvent += CycleNextBullet;

        temperature = data.temperature.Get;
        fuzeDistance = 0f;

        gameObject.AddComponent<GunFX>();
    }
    //Must be called each frame to fire
    public void Trigger() { trigger.TriggerThisFrame(); }
    private void FixedUpdate()
    {
        if (temperature < 80f) return;
        float delta = Mathf.Max(temperature - data.temperature.Get, 150f);
        temperature = Mathf.MoveTowards(temperature, data.temperature.Get, delta * gunPreset.coolingFactor * Time.fixedDeltaTime);
    }
    private void CycleNextBullet()
    {
        currentBullet = (currentBullet + 1) % ammunition.defaultBelt.Length;
    }
    protected void LaunchBullet(float delay)
    {
        Projectile bullet = Instantiate(bullets[ammunition.defaultBelt[currentBullet]], bulletSpawn.position, bulletSpawn.rotation);
        bullet.gameObject.SetActive(true);

        //float dispersion = gunPreset.dispersion * Mathf.Lerp(1f, gunPreset.overHeatDispersion, Mathv.SmoothStart(temperature / maxDispersionTemperature, 3));
        bullet.transform.rotation = Ballistics.Spread(bulletSpawn.rotation, gunPreset.dispersion);
        bullet.RaycastDamage(bullet.p.baseVelocity * bullet.transform.forward + rb.velocity, rb.velocity, 10f);
        bullet.transform.position += rb.velocity * Time.fixedDeltaTime;
        bullet.transform.position += bullet.transform.forward * bullet.p.baseVelocity * delay * 0.85f;
        bullet.InitializeTrajectory(bullet.transform.forward * bullet.p.baseVelocity + rb.velocity, transform.forward, complex.Bubble(),delay);
        if (fuzeDistance > 50f) bullet.StartFuze(fuzeDistance / bullet.p.baseVelocity);
    }
    protected void RecoilAndHeatup(float delay)
    {
        float energy = ammunition.mass / 1000f * 2f * ammunition.defaultMuzzleVel * ammunition.defaultMuzzleVel;
        rb.AddForceAtPosition(-transform.forward * energy, transform.position,ForceMode.Impulse);
        temperature += gunPreset.temperaturePerShot;
    }
    public void LoadMagazine(AmmoContainer ammoContainer)
    {
        if (gunPreset.ammunition.caliber != ammoContainer.gunPreset.ammunition.caliber) return;

        magazine = ammoContainer;
        magazine.Load(this);
    }
    public void RemoveMagazine()
    {
        magazine.attachedGun = null;
        magazine.transform.parent = transform.root;
        magazine = null;
    }
    public static int AmmunitionCount(Gun[] guns)
    {
        int total = 0;
        foreach (Gun gun in guns) total += gun.AmmoCount();
        return total;
    }
    public int AmmoCount()
    {
        int a = magazine ? magazine.ammo : 0;
        if (mechanism.roundState == GunMechanism.RoundState.HotRound) a++;
        return a;
    }
    public bool Jam()
    {
        if (temperature < critTemperature) return false;
        return Random.value < Mathv.SmoothStart(Mathf.InverseLerp(critTemperature, absoluteTemperature, temperature), 4);
    }
    public bool MustBeCocked() { return mechanism.MustBeCocked(); }
    public bool MustBeReloaded() { return !reloading && (MustBeCocked() || !magazine || magazine.ammo <= 0); }
    public bool CanBeReloaded() { return !reloading && (MustBeCocked() || !magazine || magazine.ammo < magazine.capacity); }
    public bool Firing() { return mechanism.IsFiring(); }
    public Vector3 MagazinePosition() { return magazineAttachPoint.position; }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Gun)), CanEditMultipleObjects]
public class GunEditor : Editor
{
    SerializedProperty gunPreset;
    SerializedProperty controller;
    SerializedProperty noConvergeance;

    SerializedProperty ejection;
    SerializedProperty muzzleEffects;
    SerializedProperty bulletSpawn;

    SerializedProperty magazine;
    SerializedProperty clipAmmo;
    SerializedProperty magStorage;
    SerializedProperty magazineAttachPoint;
    void OnEnable()
    {
        gunPreset = serializedObject.FindProperty("gunPreset");
        controller = serializedObject.FindProperty("controller");
        noConvergeance = serializedObject.FindProperty("noConvergeance");

        ejection = serializedObject.FindProperty("ejection");
        muzzleEffects = serializedObject.FindProperty("muzzleEffects");
        bulletSpawn = serializedObject.FindProperty("bulletSpawn");
        magazineAttachPoint = serializedObject.FindProperty("magazineAttachPoint");

        magazine = serializedObject.FindProperty("magazine");
        clipAmmo = serializedObject.FindProperty("clipAmmo");
        magStorage = serializedObject.FindProperty("magStorage");
    }

    static bool showGunMain = true;
    static bool showTransforms = true;
    static bool showAmmo = true;
    static bool showMassInfo = true;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Gun gun = (Gun)target;

        showGunMain = EditorGUILayout.Foldout(showGunMain, "Gun", true, EditorStyles.foldoutHeader);
        if (showGunMain)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(gunPreset);
            EditorGUILayout.PropertyField(controller);
            EditorGUILayout.PropertyField(noConvergeance);

            EditorGUI.indentLevel--;
        }

        showAmmo = EditorGUILayout.Foldout(showAmmo, "Ammo & Mags", true, EditorStyles.foldoutHeader);
        if (showAmmo)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(gun.magazine ? "Can be reloaded from a stock" : "No magazine system, cannot be reloaded", MessageType.None);
            EditorGUILayout.PropertyField(magazine);
            if (!gun.magazine)
                EditorGUILayout.PropertyField(clipAmmo, new GUIContent("Ammo Capacity"));
            else
                EditorGUILayout.PropertyField(magStorage, new GUIContent("Magazine Stock"));

            EditorGUI.indentLevel--;
        }

        showTransforms = EditorGUILayout.Foldout(showTransforms, "Transforms", true, EditorStyles.foldoutHeader);
        if (showTransforms)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(ejection, new GUIContent("Ejected Casings"));
            EditorGUILayout.PropertyField(muzzleEffects, new GUIContent("Muzzle Effects"));
            EditorGUILayout.PropertyField(bulletSpawn, new GUIContent("Muzzle Bullets"));
            if (gun.magazine) EditorGUILayout.PropertyField(magazineAttachPoint, new GUIContent("Mag Attach Point"));
            EditorGUI.indentLevel--;
        }

        if (gun.gunPreset)
        {
            showMassInfo = EditorGUILayout.Foldout(showMassInfo, "Mass Infos", true, EditorStyles.foldoutHeader);
            if (showMassInfo)
            {
                EditorGUI.indentLevel++;
                gun.emptyMass = gun.gunPreset.mass;
                float gunMass = gun.gunPreset.mass;
                float ammoMass = gun.magazine ? gun.magazine.Mass() : gun.Mass() - gunMass;
                EditorGUILayout.LabelField("Unloaded Gun", gunMass.ToString("0.0") + " Kg");
                EditorGUILayout.LabelField("Loaded Gun", (ammoMass + gunMass).ToString("0.0") + " Kg");
                //EditorGUILayout.LabelField("Mag + Ammo", ammoMass.ToString("0.0") + " Kg");
                EditorGUI.indentLevel--;
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif