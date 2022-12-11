using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SofComplex : SofObject
{
    //References
    public SphereCollider bubble;
    public LodModule lod;

    public override void Initialize()
    {
        lod = gameObject.AddComponent<LodModule>();
        base.Initialize();
        if (bubble) bubble.gameObject.layer = 11;
    }
    public override void Explosion(Vector3 center, float tnt)
    {
        base.Explosion(center, tnt);
        float sqrDis = (center - transform.position).sqrMagnitude;
        if (tnt < sqrDis / 2000f) return;   //no calculations if too far
        foreach (Part p in data.parts) if (p) p.ExplosionDamage(center, tnt);
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