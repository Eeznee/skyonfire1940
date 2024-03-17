using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class WingSkin : SofModule
{
    private Wing parentWing;
    const float caliberToHoleRatio = 25f;

    public override float EmptyMass()
    {
        return 0f;
    }

    public static WingSkin CreateSkin(Wing parentWing, Mesh skinMesh)
    {
        Transform parentTr = parentWing.tr;
        GameObject skinObject = parentTr.CreateChild(parentWing.name + " Skin").gameObject;

        MeshCollider meshCo = skinObject.gameObject.AddComponent<MeshCollider>();
        meshCo.sharedMesh = skinMesh;
        meshCo.isTrigger = false;
        meshCo.convex = true;

        WingSkin skin = skinObject.AddComponent<WingSkin>();
        skin.parentWing = parentWing;
        skin.material = parentWing.aircraft.materials.Material(skin);
        skin.InitializeComponent(parentWing.complex);
        return skin;
    }
    public override void KineticDamage(float damage, float caliber, float fireCoeff)
    {
        float holeArea = Mathv.SmoothStart(caliber * caliberToHoleRatio / 2000f, 2) * Mathf.PI;
        DamageIntegrity(holeArea / parentWing.area * Integrity);
    }
    public override void BurnDamage(float damage)
    {
        base.BurnDamage(damage * 0.4f);
    }
    public override void Rip()
    {
        return;
    }
}