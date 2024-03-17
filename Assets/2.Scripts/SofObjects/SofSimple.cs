using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[RequireComponent(typeof(SofObject))]
public class SofSimple : MonoBehaviour
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

    private float hp;

    private void Start()
    {
        hp = tntHp;
    }

    public void Explosion(Vector3 center, float tnt)
    {
        float distance = (transform.position - center).magnitude;
        if (distance < radius + Ballistics.ExplosionRangeSimple(tnt))
        {
            hp -= tnt;
        }
        else if (distance < radius + Ballistics.HalfExplosionRangeSimple(tnt))
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
        SofObject sofObject = GetComponent<SofObject>();
        if (sofObject)
        {
            if (sofObject.destroyed) return;
            sofObject.destroyed = true;
            GameManager.sofObjects.Remove(sofObject);
        }
        if (intactVersion) intactVersion.SetActive(false);
        if (destroyedVersion) destroyedVersion.SetActive(true);
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

        SofSimple simple = (SofSimple)target;
        //Physics
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


        if (GUI.changed)
        {
            EditorUtility.SetDirty(simple);
            EditorSceneManager.MarkSceneDirty(simple.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif