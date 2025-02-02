using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class ObjectMaterials : SofComponent
{
    public Material replacementMaterial;

    private MeshRenderer coreRenderer;

    private static Shader lit;
    private static Shader simpleLit;

    private Material MainMaterial
    {
        get
        {
            Material mainMat = new Material(replacementMaterial? replacementMaterial : coreRenderer.sharedMaterial);
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
    }

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        if (lit == null) lit = Shader.Find("Universal Render Pipeline/Lit");
        if (simpleLit == null) simpleLit = Shader.Find("Universal Render Pipeline/Simple Lit");

        coreRenderer = aircraft.GetComponentInChildren<FuselageCore>().GetComponent<MeshRenderer>();

        ReplaceMaterial(MainMaterial, coreRenderer);
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
}
#if UNITY_EDITOR
[CustomEditor(typeof(ObjectMaterials))]
public class ObjectMaterialsEditor : SofComponentEditor
{
    protected override void BasicFoldout()
    {
        base.BasicFoldout();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("replacementMaterial"));
    }
    protected override string BasicName()
    {
        return "Material";
    }
}
#endif