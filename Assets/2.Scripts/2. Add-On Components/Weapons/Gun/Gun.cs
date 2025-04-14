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
[AddComponentMenu("Sof Components/Weapons/Guns/Gun")]
public class Gun : SofComponent, IMassComponent
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

    public bool separateBulletPos;
    public bool ejectCasings;
    public Vector3 ejectionPos;
    public Vector3 muzzlePos;
    public Vector3 bulletPos;

    public bool noConvergeance = false;
    public Quaternion convergence;
    public Quaternion cheatConvergence;
    public float cheatTime;

    public Transform magazineAttachPoint;
    public GunPreset gunPreset;
    public MagazineStorage magStorage;
    public AmmoContainer magazine;
    public int clipAmmo;
    public BoltHandle bolt;


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

    private int currentBullet = 0;
    private float fuzeTimer = 0f;
    public bool reloading = false;

    const float critTemperature = 450f;
    const float absoluteTemperature = 800f;
    //const float maxDispersionTemperature = 550f;

    public float RealMass => EmptyMass;
    public float RealMassIncludingMagazine
    {
        get
        {
            if (magazine) return EmptyMass + magazine.RealMass;
            else return EmptyMass;
        }
    }
    public float LoadedMass
    {
        get
        {
            if (magazine) return EmptyMass;
            if (!gunPreset) return EmptyMass;
            return EmptyMass + clipAmmo * gunPreset.ammunition.FullMass;
        }
    }
    public float EmptyMass => gunPreset ? gunPreset.mass : 0f;

    public override void Rearm()
    {
        base.Rearm();
        mechanism.ResetMechanism();
    }
    public void SetFuze(float _fuzeTimer)
    {
        if (ammunition.caliber < 35f) fuzeTimer = 0f;
        else fuzeTimer = _fuzeTimer;
    }
    public override void Initialize(SofComplex _complex)
    {
        trigger = this.GetCreateComponent<GunTrigger>();
        mechanism = this.GetCreateComponent<GunMechanism>();

        if (!separateBulletPos) bulletPos = muzzlePos;
        if (!magazineAttachPoint) magazineAttachPoint = transform;

        base.Initialize(_complex);

        ammunition = gunPreset.ammunition;
        bullets = new Projectile[ammunition.bullets.Length];
        for (int i = 0; i < bullets.Length; i++)
            bullets[i] = gunPreset.ammunition.CreateProjectile(i, transform);

        currentBullet = Random.Range(0, ammunition.defaultBelt.Length);

        if (controller == GunController.Gunner) noConvergeance = true;
        if (noConvergeance) convergence = Quaternion.identity;
        cheatConvergence = Quaternion.identity;
        cheatTime = 0f;

        if (!magazine) magazine = AmmoContainer.CreateAmmoBelt(this, clipAmmo);

        OnFireEvent += FireBullet;
        OnFireEvent += RecoilAndHeatup;
        OnChamberRoundEvent += CycleNextBullet;

        temperature = data.temperature.Get;

        gameObject.AddComponent<GunParticles>();
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
    private void FireBullet(float delay)
    {
        //float dispersion = gunPreset.dispersion * Mathf.Lerp(1f, gunPreset.overHeatDispersion, Mathv.SmoothStart(temperature / maxDispersionTemperature, 3));

        Projectile bullet = Instantiate(bullets[ammunition.defaultBelt[currentBullet]]);

        Quaternion convergeanceRotation = cheatTime > Time.time ? cheatConvergence : convergence;
        Quaternion bulletRotation = tr.rotation * convergeanceRotation;
        bulletRotation = Ballistics.Spread(bulletRotation, gunPreset.dispersion);
        Vector3 bulletDirection = bulletRotation * Vector3.forward;

        bullet.transform.rotation = bulletRotation;
        bullet.transform.position = tr.TransformPoint(bulletPos);
        bullet.gameObject.SetActive(true);

        bullet.StartDamage(bullet.properties.baseVelocity * bulletDirection, 10f);
        bullet.transform.position += rb.velocity * Time.fixedDeltaTime;
        Collider ignoreCollider = complex.bubble ? complex.bubble.bubble : null;
        bullet.InitializeTrajectory(bulletDirection * bullet.properties.baseVelocity + rb.velocity, ignoreCollider);

        if (fuzeTimer > 0.1f) bullet.StartFuze(fuzeTimer);
    }
    private void RecoilAndHeatup(float delay)
    {
        float energy = ammunition.mass / 1000f * 2f * ammunition.defaultMuzzleVel * ammunition.defaultMuzzleVel;
        rb.AddForceAtPosition(-transform.forward * energy, transform.position, ForceMode.Impulse);
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
