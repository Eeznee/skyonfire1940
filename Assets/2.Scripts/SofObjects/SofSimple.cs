using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SofSimple : SofObject
{
    //Damage model
    public float radius = 5f;
    public float tntHp = 20f;
    public bool bulletAffected;
    public float bulletHp = 200f;

    //Destroy Effect
    public bool destroyAnim = false;
    public float lifeTime = 30f;
    public GameObject intactVersion;
    public GameObject destroyedVersion;

    float hp;

    public override void Initialize()
    {
        base.Initialize();
        hp = tntHp;
    }
    public override void Explosion(Vector3 center, float tnt)
    {
        float distance = (transform.position - center).magnitude;
        if (distance < radius + Ballistics.ExplosionRangeSimple(tnt))
        {
            hp -= tnt;
        } else if (distance < radius + Ballistics.HalfExplosionRangeSimple(tnt))
        {
            hp -= tnt / 2f;
        }
        if (hp <= 0f) SofDestroy();
    }

    public void BulletDamage(float energy)
    {
        hp -= energy / 1000f / bulletHp * tntHp;
        if (hp <= 0f) SofDestroy();
    }

    public void SofDestroy()
    {
        if (destroyed) return;
        intactVersion.SetActive(false);
        destroyedVersion.SetActive(true);
        destroyed = true;
        GameManager.sofObjects.Remove(this);
        if (destroyAnim) Destroy(gameObject, lifeTime);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SofSimple))]
public class SofSimpleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SofSimple sofSimple = (SofSimple)target;
        sofSimple.warOnly = EditorGUILayout.Toggle("War Only", sofSimple.warOnly);
        //Physics
        GUILayout.Space(15f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Damage Settings", MessageType.None);
        GUI.color = GUI.backgroundColor;
        sofSimple.radius = EditorGUILayout.FloatField("Radius", sofSimple.radius);
        sofSimple.tntHp = EditorGUILayout.FloatField("Tnt Kg Hp", sofSimple.tntHp);
        sofSimple.bulletAffected = EditorGUILayout.Toggle("Affected by bullets", sofSimple.bulletAffected);
        if (sofSimple.bulletAffected)
        {
            sofSimple.bulletHp = EditorGUILayout.FloatField("Bullet Hp", sofSimple.bulletHp);
        }
        GUILayout.Space(15f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Destroy Animation", MessageType.None);
        GUI.color = GUI.backgroundColor;
        sofSimple.intactVersion = EditorGUILayout.ObjectField("Intact GameObject", sofSimple.intactVersion, typeof(GameObject), true) as GameObject;
        sofSimple.destroyedVersion = EditorGUILayout.ObjectField("Destroyed GameObject", sofSimple.destroyedVersion, typeof(GameObject), true) as GameObject;
        sofSimple.destroyAnim = EditorGUILayout.Toggle("Destruction Animation", sofSimple.destroyAnim);
        if (sofSimple.destroyAnim)
        {
            sofSimple.lifeTime = EditorGUILayout.FloatField("Lifetime On Destroy", sofSimple.lifeTime);
        }

        SerializedProperty crew = serializedObject.FindProperty("crew");
        EditorGUILayout.PropertyField(crew, true);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(sofSimple);
            EditorSceneManager.MarkSceneDirty(sofSimple.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif