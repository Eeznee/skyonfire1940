using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class AmmoContainer : SofComponent, IMassComponent
{
    public GunPreset gunPreset;
    public int capacity = 100;
    public Vector3 ejectVector = new Vector3(0f, 0.2f, 0f);

    [HideInInspector] public int ammo;
    [HideInInspector] public HandGrip grip;
    [HideInInspector] public Gun attachedGun;

    public float RealMass => gunPreset.ammunition.FullMass * ammo;
    public float EmptyMass => 0f;
    public float LoadedMass => gunPreset.ammunition.FullMass * capacity;
    public float FullyLoadedMass => gunPreset.ammunition.FullMass * capacity + EmptyMass;

    public override void Rearm()
    {
        base.Rearm();
        ammo = capacity;
    }
    public virtual void InsertThisMagazine(Gun gun)
    {
        if (gunPreset.ammunition.caliber != gun.gunPreset.ammunition.caliber) return;
        attachedGun = gun;
        transform.parent = gun.magazineAttachPoint;
        transform.position = gun.magazineAttachPoint.position;
        transform.rotation = gun.magazineAttachPoint.rotation;
    }
    public virtual void UnloadThisMagazine()
    {
        if (attachedGun == null) return;

        transform.parent = transform.parent.parent;
        attachedGun = null;
    }

    public override void Initialize(SofModular _complex)
    {
        ammo = capacity;
        base.Initialize(_complex);
        grip = GetComponentInChildren<HandGrip>();
    }
    public virtual bool EjectRound()
    {
        if (ammo <= 0) return false;
        ammo--;
        if (sofComplex) sofComplex.ShiftMass(-gunPreset.ammunition.FullMass);
        return true;
    }
    public static AmmoContainer CreateAmmoBelt(Gun gun, int capacity)
    {
        AmmoContainer belt = new GameObject(gun.name + " Ammo Belt").AddSofComponent<AmmoContainer>(gun.sofModular);
        belt.capacity = belt.ammo = capacity;
        belt.gunPreset = gun.gunPreset;
        gun.LoadMagazine(belt);

        return belt;
    }
}
