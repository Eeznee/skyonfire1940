using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class AirfoilSkin : Airframe
{
    const float caliberToHoleRatio = 25f;
    public override void Initialize(ObjectData d,bool firstTime)
    {
        base.Initialize(d,firstTime);
        emptyMass = 0f;
        detachable = false;
    }
    public override void Damage(float damage, float caliber, float fireCoeff)
    {
        float holeArea = Mathf.Pow(caliber * caliberToHoleRatio / 2000f, 2) * Mathf.PI;
        structureDamage -= holeArea / area * structureDamage;
        structureDamage = Mathf.Clamp01(structureDamage);
    }
    public override void Burning(bool receive, float coeff)
    {
        structureDamage -= 0.01f * coeff * Time.deltaTime;
    }
    public override void Rip()
    {
        return;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AirfoilSkin))]
public class AirfoilSkinEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //
        AirfoilSkin skin = (AirfoilSkin)target;
        skin.material = EditorGUILayout.ObjectField("Material", skin.material, typeof(PartMaterial), false) as PartMaterial;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(skin);
            EditorSceneManager.MarkSceneDirty(skin.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
