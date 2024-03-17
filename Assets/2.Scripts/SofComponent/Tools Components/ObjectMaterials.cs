using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class ObjectMaterials : SofComponent
{
    public Material glassMaterial;
    private static Shader lit;
    private static Shader simpleLit;

    public MeshRenderer FuselageCore()
    {
        return aircraft.GetComponentInChildren<FuselageCore>().GetComponent<MeshRenderer>();
    }

    private Material MainMaterial()
    {
        Material mainMat = new Material(FuselageCore().sharedMaterial);
        mainMat.shader = QualitySettings.GetQualityLevel() == 0 ? simpleLit : lit;

        string textureName = aircraft.squadron.textureName;
        if (textureName != "")
        {
            Texture2D texture = TextureTool.Load(TextureTool.FolderPath(aircraft.card.fileName) + textureName);
            mainMat.mainTexture = texture;
            mainMat.name += textureName;
        }
        return mainMat;
    }
    public void ReplaceMaterial(Material newMat, Renderer refRenderer)
    {
        foreach (Renderer renderer in complex.GetComponentsInChildren<Renderer>())
        {
            if (renderer == refRenderer) continue;
            if (renderer.sharedMaterial == refRenderer.sharedMaterial)
                renderer.sharedMaterial = newMat;
        }
        refRenderer.sharedMaterial = newMat;
    }
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        if (lit == null) lit = Shader.Find("Universal Render Pipeline/Lit");
        if (simpleLit == null) simpleLit = Shader.Find("Universal Render Pipeline/Simple Lit");

        ReplaceMaterial(MainMaterial(), FuselageCore());
        //Cockpit Material
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(ObjectMaterials))]
public class ObjectMaterialsEditor : Editor
{
    public Material materialToApply;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();

        GUILayout.Space(10f);

        ObjectMaterials objectMaterials = (ObjectMaterials)target;

        materialToApply = EditorGUILayout.ObjectField("Material To Apply", materialToApply, typeof(Material), false) as Material;

        if (GUILayout.Button("Apply To Renderers"))
        {
            if (!materialToApply) return;
            objectMaterials.SetReferences();
            objectMaterials.ReplaceMaterial(materialToApply, objectMaterials.FuselageCore());
        }


        if (GUI.changed)
        {
            EditorUtility.SetDirty(objectMaterials);
            EditorSceneManager.MarkSceneDirty(objectMaterials.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif