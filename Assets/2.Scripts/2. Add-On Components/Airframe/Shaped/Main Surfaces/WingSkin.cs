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

    public Collider skinCollider => meshCollider;
    private MeshCollider meshCollider;


    public static WingSkin CreateSkin(Wing parentWing, Mesh skinMesh)
    {
        Transform parentTr = parentWing.tr;
        GameObject skinObject = parentTr.CreateChild(parentWing.name + " Skin").gameObject;

        WingSkin skin = skinObject.AddSofComponent<WingSkin>(parentWing.complex);
        skin.parentWing = parentWing;

        skin.meshCollider = skinObject.gameObject.AddComponent<MeshCollider>();
        skin.meshCollider.sharedMesh = skinMesh;
        skin.meshCollider.isTrigger = false;
        skin.meshCollider.convex = true;

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