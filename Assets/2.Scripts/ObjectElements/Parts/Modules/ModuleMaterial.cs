#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "New Part Material", menuName = "Aircraft/Part Material Preset")]
public class ModuleMaterial : ScriptableObject
{
    //Healthpoints
    public float hp = 20f;
    public float hpPerSq = 5f;

    //Ignition
    public bool ignitable = false;
    public float burningChance = 0.2f;
    public ParticleSystem burningEffect;

    //Armor
    public float armor = 1f;
    public float totalThickness = 2f;
}

#if UNITY_EDITOR
[CustomEditor(typeof(ModuleMaterial))]
public class ModuleMaterialEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        ModuleMaterial mat = (ModuleMaterial)target;

        //General settings
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Healthpoints", MessageType.None);
        EditorGUILayout.HelpBox("7.62 rounds deals 3 dmg", MessageType.Info);
        GUI.color = GUI.backgroundColor;
        mat.hp = EditorGUILayout.FloatField("Fixed Hp", mat.hp);
        mat.hpPerSq = EditorGUILayout.FloatField("Hp Per Square meter", mat.hpPerSq);

        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Armor", MessageType.None);
        GUI.color = GUI.backgroundColor;
        mat.armor = EditorGUILayout.FloatField("Armor thickness mm", mat.armor);
        mat.totalThickness = EditorGUILayout.FloatField("Full pen armor mm", mat.totalThickness);

        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Ignition", MessageType.None);
        GUI.color = GUI.backgroundColor;
        mat.ignitable = EditorGUILayout.Toggle("Ignitable", mat.ignitable);
        if (mat.ignitable) {
            mat.burningChance = EditorGUILayout.Slider("Burning Chance", mat.burningChance, 0f,1f);
            mat.burningEffect = EditorGUILayout.ObjectField("Burning Effect", mat.burningEffect, typeof(ParticleSystem), false) as ParticleSystem;
        } 


        if (GUI.changed)
        {
            EditorUtility.SetDirty(mat);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
