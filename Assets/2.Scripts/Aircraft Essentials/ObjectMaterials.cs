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

    private Material mainMaterial;

    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);

        if (lit == null) lit = Shader.Find("Universal Render Pipeline/Lit");
        if (simpleLit == null) simpleLit = Shader.Find("Universal Render Pipeline/Simple Lit");

        coreRenderer = aircraft.GetComponentInChildren<FuselageCore>().GetComponent<MeshRenderer>();

        mainMaterial = new Material(replacementMaterial ? replacementMaterial : coreRenderer.sharedMaterial);

        string textureName = aircraft.squadron.textureName;
        if (textureName != "")
        {
            Texture2D texture = TextureTool.Load(TextureTool.FolderPath(aircraft.card.fileName) + textureName);
            mainMaterial.mainTexture = texture;
            mainMaterial.name += textureName;
        }

        foreach (Renderer renderer in sofModular.GetComponentsInChildren<Renderer>())
        {
            if (renderer == coreRenderer) continue;
            if (renderer.sharedMaterial == coreRenderer.sharedMaterial)
                renderer.sharedMaterial = mainMaterial;
        }
        coreRenderer.sharedMaterial = mainMaterial;

        UpdateShaderQuality();
    }
    private void OnEnable()
    {
        SofSettingsSO.OnUpdateSettings += UpdateShaderQuality;
    }
    private void OnDisable()
    {
        SofSettingsSO.OnUpdateSettings -= UpdateShaderQuality;
    }
    private void UpdateShaderQuality()
    {
        Shader targetShader = SofSettingsSO.CurrentSettings.graphicsPreset == 0 ? simpleLit : lit;
        if(targetShader != mainMaterial.shader) mainMaterial.shader = targetShader;
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