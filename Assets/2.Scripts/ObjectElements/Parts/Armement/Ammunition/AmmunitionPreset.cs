#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "New Ammunition Preset", menuName = "Weapons/Ammunition")]
public class AmmunitionPreset : ScriptableObject
{
    public float caliber = 7.62f;
    public float caseLength = 51;
    [HideInInspector] public float mass = 10.5f;
    [HideInInspector] public float defaultMuzzleVel = 750f;
    public BulletHits bulletHits;

    public GameObject trailEffect;
    public TracerProperties tracer;

    [SerializeField] public ProjectileProperties[] bullets = new ProjectileProperties[3];
    public int[] defaultBelt = new int[3] { 0, 1, 2 };

    const float cartridgeMassCoeff = 0.000008f;

    public float FullMass { get { return caliber * caliber * caseLength * cartridgeMassCoeff; } }

    public Projectile CreateProjectile(int i, Transform parentGun)
    {
        ProjectileProperties prop = bullets[i];
        prop.diameter = caliber;
        prop.bulletHits = bulletHits;
        Projectile bullet = new GameObject(name + " " + prop.name).AddComponent<Projectile>();
        bullet.gameObject.SetActive(false);
        if (prop.tracer)
            bullet.gameObject.AddComponent<Tracer>().InitializeTracer(tracer);
        bullet.Setup(prop);

        bullet.transform.SetPositionAndRotation(parentGun.position, parentGun.rotation);
        bullet.transform.parent = parentGun;
        if (trailEffect && prop.tracer) Instantiate(trailEffect, bullet.transform.position, bullet.transform.rotation, bullet.transform);

        return bullet;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AmmunitionPreset))]
    public class AmmunitionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            AmmunitionPreset ammo = (AmmunitionPreset)target;

            //General settings
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("General Settings", MessageType.None);
            GUI.color = GUI.backgroundColor;
            ammo.caliber = EditorGUILayout.FloatField("Caliber mm", ammo.caliber);
            ammo.caseLength = EditorGUILayout.FloatField("Case Length mm", ammo.caseLength);
            EditorGUILayout.LabelField("Dimensions", ammo.caliber.ToString() + "x" + ammo.caseLength.ToString() + " mm");
            EditorGUILayout.LabelField("Whole Cartridge Mass", ammo.FullMass.ToString("0.0") + " kg");
            ammo.mass = EditorGUILayout.FloatField("Bullet Mass kg", ammo.mass);
            ammo.defaultMuzzleVel = EditorGUILayout.FloatField("Default Muzzle Velocity m/s", ammo.defaultMuzzleVel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultBelt"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bullets"));

            for (int i = 0; i < ammo.bullets.Length; i++)
            {
                ProjectileProperties bullet = ammo.bullets[i];
                if (!bullet.Equals(null))
                {
                    bullet.displayAsBullet = true;
                    bullet.diameter = ammo.caliber;
                    bullet.basePenetration = Mathf.Max(0.5f, bullet.basePenetration);
                    if (bullet.mass == 0f || bullet.approximated) bullet.mass = ammo.mass;
                    if (bullet.baseVelocity == 0f || bullet.approximated) bullet.baseVelocity = ammo.defaultMuzzleVel;
                    if (bullet.approximated)
                    {
                        bullet.name = bullet.AutoName();
                        bullet.basePenetration = bullet.ApproxPenetration();
                    }
                }
            }

            GUILayout.Space(20f);
            EditorGUILayout.HelpBox("Effects", MessageType.None);
            ammo.bulletHits = EditorGUILayout.ObjectField("Bullet Hits Preset", ammo.bulletHits, typeof(BulletHits), false) as BulletHits;
            ammo.trailEffect = EditorGUILayout.ObjectField("Trail Effect", ammo.trailEffect, typeof(GameObject), false) as GameObject;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("tracer"));

            if (GUI.changed)
            {
                EditorUtility.SetDirty(ammo);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
