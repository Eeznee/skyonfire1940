using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class WingSkin : SofModule
{
    public override ModuleArmorValues Armor => ModulesHPData.DuraluminArmor;
    public override float MaxHp => 100f;

    private Wing parentWing;
    const float caliberToHoleRatio = 25f;


    public static WingSkin CreateSkin(Wing parentWing, Mesh skinMesh)
    {
        Transform parentTr = parentWing.tr;
        GameObject skinObject = parentTr.CreateChild(parentWing.name + " Skin").gameObject;

        MeshCollider meshCo = skinObject.gameObject.AddComponent<MeshCollider>();
        meshCo.sharedMesh = skinMesh;
        meshCo.isTrigger = false;
        meshCo.convex = true;

        WingSkin skin = skinObject.AddSofComponent<WingSkin>(parentWing.complex);
        skin.parentWing = parentWing;
        return skin;
    }
    public override void ProjectileDamage(float damage, float caliber, float fireCoeff)
    {
        float holeArea = Mathv.SmoothStart(caliber * caliberToHoleRatio / 2000f, 2) * Mathf.PI;
        DirectStructuralDamage(holeArea / parentWing.area * structureDamage);
    }
    public override void Rip()
    {
        return;
    }
}