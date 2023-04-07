using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class WingSkin : Module
{
    private Wing parentWing;
    const float caliberToHoleRatio = 25f;

    public static WingSkin CreateSkin(Wing parentWing, Mesh skinMesh)
    {
        WingSkin skin = new GameObject(parentWing.name + " Skin").AddComponent<WingSkin>();
        skin.parentWing = parentWing;
        Transform parentTr = parentWing.tr;
        skin.transform.SetParent(parentTr);
        skin.transform.SetPositionAndRotation(parentTr.position, parentTr.rotation);
        MeshCollider meshCo = skin.gameObject.AddComponent<MeshCollider>();
        meshCo.sharedMesh = skinMesh;
        meshCo.isTrigger = false;
        meshCo.convex = true;
        skin.material = parentWing.aircraft.materials.Material(skin);
        skin.gameObject.layer = 9;
        skin.Initialize(parentWing.data, true);
        return skin;
    }
    public override void Initialize(ObjectData d,bool firstTime)
    {
        base.Initialize(d,firstTime);
        emptyMass = 0f;
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