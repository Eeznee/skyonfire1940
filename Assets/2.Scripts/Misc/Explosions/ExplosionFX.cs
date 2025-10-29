using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(AudioSource))]
public class ExplosionFX : MonoBehaviour
{
    [System.Serializable]
    public struct SpecificExplosion
    {
        public GameObject effect;
        public AudioClip[] audioClips;
        [HideInInspector] public ParticleSystem[] effects;
    }
    public float tntEquivalent = 50f;
    public float despawnTime = 15f;
    public AudioSource audioSource;

    public SpecificExplosion groundExplosion;
    public bool water;
    public SpecificExplosion waterExplosion;
    public bool air;
    public SpecificExplosion airExplosion;
    public BulletHits fragmentHits;

    private float range = 0f;
    private bool soundPlayed = true;

    public void Explode(float tnt)
    {
        //Chose appropriate effect and audio
        float height = GameManager.mapTool.HeightAtPoint(transform.position);
        SpecificExplosion explosion;
        if (transform.position.y < 2f && height < 2f && water)
            explosion = waterExplosion;
        else if (transform.position.y - height > 2f && air)
            explosion = airExplosion;
        else
            explosion = groundExplosion;
        if (height < 2f)
            transform.position = new Vector3(transform.position.x, transform.position.y - height, transform.position.z);


        //Start Effect
        float scaleFactor = Mathf.Pow(tnt / tntEquivalent, 1f / 3f);
        explosion.effect.transform.localScale = Vector3.one * scaleFactor;
        if (transform.position.y > 1f)
            foreach (ParticleSystem p in explosion.effects)
            {
                var main = p.main;
                main.simulationSpeed = 1f / scaleFactor;
            }
        explosion.effect.SetActive(true);


        //Play Audio
        float maxHearing = Mathf.Sqrt(tntEquivalent / 25f) * 10000f;
        audioSource.maxDistance = maxHearing;
        audioSource.minDistance = maxHearing / 20f;
        audioSource.outputAudioMixerGroup = GameManager.gm.listener.persistent;
        audioSource.clip = explosion.audioClips[Random.Range(0, explosion.audioClips.Length)];
        soundPlayed = false;

        Destroy(gameObject, 30f);
    }

    private void Update()
    {
        if (!soundPlayed)
        {
            range += Time.deltaTime * 343f;
            float disToCam = (transform.position - SofCamera.tr.position).sqrMagnitude;
            if (disToCam < range * range)
            {
                soundPlayed = true;
                audioSource.Play();
            }
        }

    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(ExplosionFX))]
public class ExplosionFXEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        ExplosionFX fx = (ExplosionFX)target; fx.tntEquivalent = EditorGUILayout.FloatField("TNT Equivalent", fx.tntEquivalent);

        fx.despawnTime = EditorGUILayout.FloatField("Despawn Time", fx.despawnTime);
        SerializedProperty explosion = serializedObject.FindProperty("groundExplosion");
        EditorGUILayout.PropertyField(explosion, true);
        fx.water = EditorGUILayout.Toggle("Water Explosion", fx.water);
        if (fx.water)
        {
            explosion = serializedObject.FindProperty("waterExplosion");
            EditorGUILayout.PropertyField(explosion, true);
        }
        fx.air = EditorGUILayout.Toggle("Air Explosion", fx.air);
        if (fx.air)
        {
            explosion = serializedObject.FindProperty("airExplosion");
            EditorGUILayout.PropertyField(explosion, true);
        }
        if (fx.groundExplosion.effect != null) fx.groundExplosion.effects = fx.groundExplosion.effect.GetComponentsInChildren<ParticleSystem>();
        if (fx.water && fx.waterExplosion.effect != null) fx.waterExplosion.effects = fx.waterExplosion.effect.GetComponentsInChildren<ParticleSystem>();
        if (fx.air && fx.airExplosion.effect != null) fx.airExplosion.effects = fx.airExplosion.effect.GetComponentsInChildren<ParticleSystem>();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("fragmentHits"), true);

        fx.audioSource = fx.GetComponent<AudioSource>();
        fx.audioSource.playOnAwake = false;
        fx.audioSource.volume = 1f;
        if (fx.audioSource.spatialBlend != 1f) fx.audioSource.spatialBlend = 1f;
        fx.audioSource.dopplerLevel = 0.5f;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(fx);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
