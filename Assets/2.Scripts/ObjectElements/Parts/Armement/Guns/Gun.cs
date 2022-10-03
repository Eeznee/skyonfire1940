using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Gun : Part
{
    //Settings
    public Transform ejection;
    public Transform muzzle;
    public Transform muzzleBulletSpawn;
    public GunPreset gunPreset;
    public MagazineStock magStock;
    public Magazine magazine;
    public int clipAmmo;
    public Vector3 magazineLocalPos;
    public BoltHandle bolt;
    public bool noConvergeance = false;

    //References
    protected AmmunitionPreset ammuPreset;
    protected ParticleSystem[] muzzleFlashes;
    protected ParticleSystem casings;
    protected ParticleSystem.ForceOverLifetimeModule casingsDrag;
    protected GameObject bubble;
    protected Bullet[] bullets;

    //Data
    public float temperature;
    public bool chambered; //for open bolt, this is boltCocked
    public int clips;
    protected int currentBullet = 0;
    public float fuzeDistance;

    protected float counter;

    const float critTemperature = 450f;
    const float maxDispersionTemperature = 550f;
    const float absoluteTemperature = 800f;
    const float dispersionFactor = 4f;

    public override float EmptyMass()
    {
        return gunPreset.mass;
    }
    public override float Mass()
    {
        return EmptyMass() + (magazine ? 0f : gunPreset.ammunition.FullMass * 0.001f * clipAmmo);
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
    public bool ShotReady()
    {
        return gunPreset.openBolt ? magazine && magazine.ammo > 0 && chambered : chambered;
    }
    private bool PossibleFire()
    {
        return ShotReady() && data.type > 0 && transform.position.y > 0f;
    }
    public bool Firing()
    {
        return counter != -Mathf.Infinity && PossibleFire();
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
            ammuPreset = gunPreset.ammunition;
            bullets = new Bullet[ammuPreset.bullets.Length];
            for (int i = 0; i < bullets.Length; i++)
            {
                bullets[i] = gunPreset.ammunition.bullets[i].CreateBullet();
                bullets[i].gameObject.SetActive(false);
                bullets[i].transform.parent = transform;
            }
            if (!magazine) //Create a hidden magazine if there is none available
            {
                magazine = new GameObject(gunPreset.name + " Magazine").AddComponent<Magazine>();
                magazine.capacity = clipAmmo;
                magazine.Initialize(data, true);
                magazine.gunPreset = gunPreset;
            }
            if (!muzzle) muzzle = transform;
            counter = -Mathf.Infinity;
            currentBullet = Random.Range(0, ammuPreset.defaultBelt.Length);
            temperature = data.ambientTemperature;
            emptyMass = gunPreset.mass;
        }
    }
    private void Start()
    {
        LoadMagazine(magazine);
        chambered = true;

        if (gunPreset.casingsFX && ejection)
        {
            casings = Instantiate(gunPreset.casingsFX, ejection.position, ejection.rotation, ejection).GetComponent<ParticleSystem>();
            casingsDrag = casings.forceOverLifetime;
        }


        muzzleFlashes = Instantiate(gunPreset.FireFX, muzzle.position, muzzle.rotation, muzzle).GetComponentsInChildren<ParticleSystem>();
        muzzleFlashes[0].transform.localScale = Vector3.one * Mathf.Pow(gunPreset.ammunition.caliber / 7.62f, 0.8f);
        fuzeDistance = 1200f;
        if (muzzleBulletSpawn) muzzle = muzzleBulletSpawn;
    }
    //Must be called each frame to fire
    public void Trigger()
    {
        while (PossibleFire() && counter <= 0f && Time.timeScale > 0f)
        {
            FireBullet();
            if (counter == -Mathf.Infinity) counter = 0f;
            counter = 60f / gunPreset.FireRate + counter;
        }
    }
    public virtual void Update()
    {
        if (counter <= 0f) counter = -Mathf.Infinity;
        else if (counter > 0f) counter -= Time.deltaTime;

        float delta = Mathf.Max(temperature - data.ambientTemperature, 150f);
        temperature = Mathf.MoveTowards(temperature, data.ambientTemperature, delta * gunPreset.coolingFactor * Time.deltaTime);
        if (casings)
            casingsDrag.z = -data.ias * data.ias / (ammuPreset.caliber * 150f);
    }
    private void IgniteCartridge()
    {
        Bullet bullet = Instantiate(bullets[ammuPreset.defaultBelt[currentBullet]], muzzle.position, muzzle.rotation);
        bullet.gameObject.SetActive(true);

        float dispersion = gunPreset.dispersion * Mathf.Lerp(1f, gunPreset.overHeatDispersion, Mathf.Pow(temperature / maxDispersionTemperature, 3));
        bullet.transform.Rotate(Vector3.forward * Random.Range(-90, 90));
        bullet.transform.Rotate(Vector3.right * Random.Range(-1f, 1f) * dispersion);
        GameObject bubble = data.complex ? data.complex.bubble.gameObject : null;
        bullet.bubbleShotFrom = bubble;
        bullet.BulletAction(bullet.bullet.muzzleVelocity,bullet.transform.forward, 10f);
        bullet.InitializeTrajectory(bullet.transform.forward * bullet.bullet.muzzleVelocity + rb.velocity);
        if (fuzeDistance > 50f) bullet.SetFuze(fuzeDistance);
        //Recoil and temperature
        float energy = bullet.bullet.mass / 500f * bullet.bullet.muzzleVelocity;
        rb.AddForceAtPosition(-transform.forward * energy / rb.mass, transform.position, ForceMode.VelocityChange);
        //Temperature
        temperature += gunPreset.temperaturePerShot;
    }
    private void FireBullet()
    {
        IgniteCartridge();
        float jamChance = Mathf.Pow(Mathf.InverseLerp(critTemperature, absoluteTemperature, temperature), 4);
        if (gunPreset.openBolt)
        {
            Chamber();
            if (Random.value > jamChance) Eject();
        }
        else
        {
            Eject();
            if (Random.value > jamChance) Chamber();
        }

        //Firing effect
        if (complex && complex.lod.LOD() < 2) foreach (ParticleSystem ps in muzzleFlashes) ps.Emit(1);
        if (complex && complex.lod.LOD() == 0) if (casings && ejection) casings.Emit(1);
    }
    //Used after each shot and when cycling the bolt
    public void Eject()
    {
        chambered = gunPreset.openBolt && magazine.ammo > 0;
    }
    public void Chamber()
    {
        chambered = magazine && magazine.EjectRound();
        currentBullet = (currentBullet + 1) % ammuPreset.defaultBelt.Length;
        if (gunPreset.openBolt) chambered = false;
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
        gun.muzzle = EditorGUILayout.ObjectField("Muzzle Transform", gun.muzzle, typeof(Transform), true) as Transform;
        gun.muzzleBulletSpawn = EditorGUILayout.ObjectField("Bullet Spawn Transform", gun.muzzleBulletSpawn, typeof(Transform), true) as Transform;
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