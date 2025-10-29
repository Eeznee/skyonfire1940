using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[RequireComponent(typeof(SofObject))]
public class SimpleDamageModel : SofDamageModel
{
    public float radius = 5f;
    public float tntHp = 20f;
    public bool bulletAffected;
    public float bulletHp = 200f;

    public bool destroyAnim = false;
    public float lifeTime = 30f;
    public GameObject intactVersion;
    public GameObject destroyedVersion;

    public Action OnDestroy;

    private float integrity;

    protected override void Start()
    {
        base.Start();
        integrity = 1f;
    }

    private void InflictDamage(float integrityDamage)
    {
        integrity -= integrityDamage;

        if(integrity < 0f)
        {
            Destroy();
        }
    }
    public void Destroy()
    {
        if (sofObject.Destroyed) return;
        sofObject.Destroy();
        GameManager.sofObjects.Remove(sofObject);

        if (intactVersion) intactVersion.SetActive(false);
        if (destroyedVersion) destroyedVersion.SetActive(true);
        if (destroyAnim) Destroy(gameObject, lifeTime);

        OnDestroy?.Invoke();
    }
    public override void Explosion(Vector3 center, float tnt)
    {
        float distance = (transform.position - center).magnitude;
        if (distance < radius + Ballistics.ExplosionRangeSimple(tnt))
        {
            InflictDamage(tnt / tntHp);
        }
        else if (distance < radius + Ballistics.HalfExplosionRangeSimple(tnt))
        {
            InflictDamage(tnt * 0.5f / tntHp);
        }
    }
    public override HitResult ProjectileRaycast(Vector3 position, Vector3 velocity, ProjectileChart chart)
    {
        if (!bulletAffected) return HitResult.NoHit(velocity);

        int mask = LayerMask.GetMask("SofComplex","Default");

        bool miss = !Physics.Raycast(position, velocity, out RaycastHit hit, velocity.magnitude * Time.fixedDeltaTime, mask);

        if (miss)
            return HitResult.NoHit(velocity);

        bool otherColliderHit = hit.collider.GetComponentInParent<SimpleDamageModel>() != this;
        if (otherColliderHit)
            return HitResult.NoHit(velocity);

        float damageHp = chart.KineticDamage(velocity.sqrMagnitude);
        InflictDamage(damageHp / bulletHp);

        return new HitResult(hit, hit, Vector3.zero, HitSummary.Stopped);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SimpleDamageModel))]
public class SofSimpleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SimpleDamageModel simple = (SimpleDamageModel)target;

        GUILayout.Space(15f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Damage Settings", MessageType.None);
        GUI.color = GUI.backgroundColor;

        simple.radius = EditorGUILayout.FloatField("Radius", simple.radius);
        simple.tntHp = EditorGUILayout.FloatField("Tnt Kg Hp", simple.tntHp);
        simple.bulletAffected = EditorGUILayout.Toggle("Affected by bullets", simple.bulletAffected);
        if (simple.bulletAffected)
        {
            simple.bulletHp = EditorGUILayout.FloatField("Bullet Hp", simple.bulletHp);
        }
        GUILayout.Space(15f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Destroy Animation", MessageType.None);
        GUI.color = GUI.backgroundColor;
        simple.intactVersion = EditorGUILayout.ObjectField("Intact GameObject", simple.intactVersion, typeof(GameObject), true) as GameObject;
        simple.destroyedVersion = EditorGUILayout.ObjectField("Destroyed GameObject", simple.destroyedVersion, typeof(GameObject), true) as GameObject;
        simple.destroyAnim = EditorGUILayout.Toggle("Destruction Animation", simple.destroyAnim);

        if (simple.destroyAnim)
        {
            simple.lifeTime = EditorGUILayout.FloatField("Lifetime On Destroy", simple.lifeTime);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif