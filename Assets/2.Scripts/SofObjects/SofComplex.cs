using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SofComplex : SofObject
{
    const float explosionCoeff = 100f;
    //References
    public SphereCollider bubble;
    public LodModule lod;

    public override void Awake()
    {
        lod = gameObject.AddComponent<LodModule>();
        base.Awake();
        if (bubble) bubble.gameObject.layer = 11;
    }
    public override void Explosion(Vector3 center, float kg, float totalKg)
    {
        base.Explosion(center, kg, totalKg);
        float dis = (center - transform.position).sqrMagnitude;
        if (kg * 2000f < dis) return;   //no calculations if too far
        foreach (Part p in data.parts)
        {
            if (p)
            {
                dis = (center - p.transform.position).sqrMagnitude;
                if (kg * 500f > dis)
                {
                    float realDis = Mathf.Max(1f, Mathf.Sqrt(dis));
                    p.Damage(explosionCoeff * kg / dis * Random.Range(0.5f, 2f));
                    //Shrapnel
                    float shrapnel = (totalKg - kg) / realDis;
                    bool shrapnelhit = Random.value * 10f < shrapnel;
                    if (shrapnelhit)
                    {
                        float shrapnelDamage = Mathf.Lerp(Mathf.Sqrt(totalKg) / 5f, 0f, dis / (kg * 1000f));
                        p.Damage(shrapnelDamage * Random.Range(0.5f, 2f), 20f, 0f);
                    }
                }
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SofComplex))]
public class SofComplexEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SofComplex complex = (SofComplex)target;
        //Physics
        GUILayout.Space(15f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Physics Settings", MessageType.None);
        GUI.color = GUI.backgroundColor;

        complex.viewPoint = EditorGUILayout.Vector3Field("External Camera ViewPoint", complex.viewPoint);
        SerializedProperty crew = serializedObject.FindProperty("crew");
        EditorGUILayout.PropertyField(crew, true);


        if (GUI.changed)
        {
            EditorUtility.SetDirty(complex);
            EditorSceneManager.MarkSceneDirty(complex.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif