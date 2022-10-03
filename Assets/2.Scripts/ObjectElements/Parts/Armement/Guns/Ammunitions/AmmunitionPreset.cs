#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "New Ammunition Preset", menuName = "Weapons/Ammunition")]
public class AmmunitionPreset : ScriptableObject
{
    public float caliber = 7.62f;
    public float caseLength = 51;
    public float mass = 10.5f;
    public float noRicochetAlpha = 50f;
    public float ricochetAlpha = 80f;
    public float defaultMuzzleVel = 750f;
    public BulletHits bulletHits;

    public Color tracerColor = Color.white;
    public Material tracerMaterial;
    public GameObject trailEffect;
    public float tracerLength = 5f;
    public float tracerWidth = 0.45f;
    public float tracerScatter = 3f;

    public BulletPreset[] bullets = new BulletPreset[3];
    public int[] defaultBelt = new int[3] { 0, 1, 2 };

    const float cartridgeMassCoeff = 0.008f;

    public float KineticEnergy { get { return mass * defaultMuzzleVel * defaultMuzzleVel / 2000f; } }
    public float FullMass { get { return caliber * caliber * caseLength * cartridgeMassCoeff; } }

    [System.Serializable]
    public class BulletPreset
    {
        //General
        public string name = "New Bullet";
        public AmmunitionPreset ammunition;
        public float penetration = 10f;
        public float fireMultiplier = 1f;
        public float tntMass = 5f;
        public float muzzleVelocity;
        public float mass;

        public bool armorPiercing = true;
        public bool explosive = false;
        public bool fuze = false;
        public bool adjusting = false;
        public bool tracer = false;

        public BulletPreset()
        {
            name = "New Bullet";
            armorPiercing = true;
        }
        public Bullet CreateBullet()
        {
            //Set references and stuff
            Bullet bullet = new GameObject(ammunition.name + " " + name).AddComponent<Bullet>();
            bullet.gameObject.layer = 10;
            bullet.bullet = this;
            bullet.ammo = ammunition;

            //Add Collider
            CapsuleCollider c = bullet.gameObject.AddComponent<CapsuleCollider>();
            c.isTrigger = true;
            c.radius = 0.08f;
            c.height = 4f;
            c.direction = 2;
            c.center = new Vector3(0f, 0f, 3f);

            //Add Tracer
            if (tracer)
            {
                LineRenderer line = bullet.gameObject.AddComponent<LineRenderer>();
                bullet.line = line;
                line.startColor = ammunition.tracerColor;
                line.endColor = ammunition.tracerColor;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                line.receiveShadows = false;
                line.material = ammunition.tracerMaterial;
                line.endWidth = line.startWidth = ammunition.tracerWidth;
                line.numCapVertices = 4;
                line.positionCount = 4;
                line.useWorldSpace = true;

                if (ammunition.trailEffect) Instantiate(ammunition.trailEffect, bullet.transform.position, Quaternion.identity, bullet.transform);
            }
            return bullet;
        }
        public GameObject EnvironmentHit(Collider mat)
        {
            string matName = mat.material.name.Replace(" (Instance)", "");
            switch (matName)
            {
                case "":
                    Debug.LogError("The collider : " + matName + " has no physical material assiocated");
                    return ammunition.bulletHits.woodHit;
                case "Mud":
                    return ammunition.bulletHits.mudHit;
                case "Sand":
                    return ammunition.bulletHits.sandHit;
                case "Stone":
                    return ammunition.bulletHits.stoneHit;
                case "Wood":
                    return ammunition.bulletHits.woodHit;
                case "Metal":
                    return ammunition.bulletHits.metalHit;
                case "Water":
                    return ammunition.bulletHits.waterHit;
                case "Wheel":
                    return ammunition.bulletHits.woodHit;
            }
            Debug.LogError("The physic material : " + matName + " has no effect associated");
            return ammunition.bulletHits.woodHit;
        }
        public GameObject AircraftHit()
        {
            if (adjusting) return ammunition.bulletHits.adjustingHit;
            return ammunition.bulletHits.metalHit;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AmmunitionPreset))]
    public class AmmunitionEditor : Editor
    {
        int bulletsAmount = 0;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            AmmunitionPreset ammo = (AmmunitionPreset)target;

            //General settings
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("General Settings", MessageType.None);
            GUI.color = GUI.backgroundColor;
            ammo.caliber = EditorGUILayout.FloatField("Caliber in mm", ammo.caliber);
            ammo.caseLength = EditorGUILayout.FloatField("Case Length in mm", ammo.caseLength);
            EditorGUILayout.LabelField("Dimensions", ammo.caliber.ToString() + "x" + ammo.caseLength.ToString() + " mm");
            EditorGUILayout.LabelField("Whole Cartridge Mass", ammo.FullMass.ToString("0.0") + " g");
            ammo.mass = EditorGUILayout.FloatField("Bullet Mass in g", ammo.mass);
            ammo.defaultMuzzleVel = EditorGUILayout.FloatField("Default Muzzle Velocity m/s", ammo.defaultMuzzleVel);
            EditorGUILayout.HelpBox("Effects", MessageType.None);
            ammo.bulletHits = EditorGUILayout.ObjectField("Bullet Hits Preset", ammo.bulletHits, typeof(BulletHits), false) as BulletHits;
            ammo.tracerColor = EditorGUILayout.ColorField("Tracer Color", ammo.tracerColor);
            ammo.tracerLength = EditorGUILayout.FloatField("Tracer Length", ammo.tracerLength);
            ammo.tracerWidth = EditorGUILayout.FloatField("Tracer Width", ammo.tracerWidth);
            ammo.tracerScatter = EditorGUILayout.FloatField("Tracer Scatter", ammo.tracerScatter);
            ammo.tracerMaterial = EditorGUILayout.ObjectField("Tracer Material", ammo.tracerMaterial, typeof(Material), false) as Material;
            ammo.trailEffect = EditorGUILayout.ObjectField("Trail Effect", ammo.trailEffect, typeof(GameObject), false) as GameObject;

            SerializedProperty belt = serializedObject.FindProperty("defaultBelt");
            EditorGUILayout.PropertyField(belt, true);

            //BULLETS
            GUILayout.Space(20f);
            GUI.color = Color.red;
            EditorGUILayout.HelpBox("Bullets", MessageType.None);
            GUI.color = GUI.backgroundColor;
            bulletsAmount = (bulletsAmount == 0) ? ammo.bullets.Length : bulletsAmount;
            bulletsAmount = EditorGUILayout.IntSlider("Amount Of Bullets", bulletsAmount, 1, 7);
            if (GUILayout.Button("Apply Amount"))
            {
                BulletPreset[] newBullets = new BulletPreset[bulletsAmount];
                for (int i = 0; i < Mathf.Min(bulletsAmount, ammo.bullets.Length); i++)
                {
                    newBullets[i] = ammo.bullets[i];
                }
                ammo.bullets = newBullets;
            }
            for (int i = 0; i < ammo.bullets.Length; i++)
            {
                BulletPreset bullet = ammo.bullets[i];
                EditorGUILayout.HelpBox("Bullet n° : " + (i + 1).ToString(), MessageType.None);
                if (!bullet.Equals(null))
                {
                    bullet.ammunition = ammo;
                    bullet.name = EditorGUILayout.TextField("Name", bullet.name);
                    bullet.mass = EditorGUILayout.FloatField("Mass", bullet.mass);
                    bullet.muzzleVelocity = EditorGUILayout.FloatField("Muzzle Velocity", bullet.muzzleVelocity);
                    if (bullet.mass == 0f) bullet.mass = ammo.mass;
                    if (bullet.muzzleVelocity == 0f) bullet.muzzleVelocity = ammo.defaultMuzzleVel;
                    bullet.penetration = EditorGUILayout.FloatField(bullet.explosive ? "Trigger Penetration" : "Point Blank Penetration", bullet.penetration);
                    if (!bullet.explosive) bullet.fireMultiplier = EditorGUILayout.FloatField("Fire Chance Multiplier", bullet.fireMultiplier);
                    bullet.tracer = EditorGUILayout.Toggle("Tracer", bullet.tracer);
                    bullet.explosive = (bullet.ammunition.caliber >= 15f) ? EditorGUILayout.Toggle("High Explosive", bullet.explosive) : false;
                    if (!bullet.explosive) bullet.adjusting = EditorGUILayout.Toggle("Adjusting", bullet.adjusting);
                    bullet.fuze = bullet.explosive ? EditorGUILayout.Toggle("Fuze", bullet.fuze) : false;
                    bullet.tntMass = bullet.explosive ? EditorGUILayout.FloatField("TNT Mass in g", bullet.tntMass) : 0f;
                }
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(ammo);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}

[CreateAssetMenu(fileName = "New Explosive Preset", menuName = "Weapons/Explosive")]
public class ExplosivePreset : ScriptableObject
{
    public float reFactor = 1f;
}