using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class AmmoContainer : Part
{
    public GunPreset gunPreset;
    public int capacity = 100;
    public Vector3 ejectVector = new Vector3(0f, 0.2f, 0f);

    [HideInInspector] public int ammo;
    [HideInInspector] public HandGrip grip;
    [HideInInspector] public Gun attachedGun;

    public override float Mass(){ return gunPreset.ammunition.FullMass * ammo + EmptyMass(); }
    public float LoadedMass() { return gunPreset.ammunition.FullMass * capacity + EmptyMass();  }

    public AmmoContainer Load(Gun gun)
    {
        if (gunPreset.ammunition.caliber != gun.gunPreset.ammunition.caliber) return null;
        attachedGun = gun;
        transform.parent = gun.transform;
        transform.localPosition = gun.magazineLocalPos;
        transform.localRotation = Quaternion.identity;
        return this;
    }

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            grip = GetComponentInChildren<HandGrip>();
            ammo = capacity;
        }
    }
    public virtual bool EjectRound()
    {
        if (ammo <= 0) return false;
        ammo--;
        return true;
    }

    public Vector3 MagTravelPos(Vector3 startPos, Vector3 endPos, float animTime)
    {
        float distance = (startPos - endPos).magnitude;
        float t = animTime * animTime;
        if (animTime > 0.2f) t = Mathf.Lerp(0.04f, 1f, (animTime - 0.2f) / 0.8f);
        Vector3 travelOffset = (endPos - (startPos + ejectVector)) * t;
        return travelOffset + startPos + ejectVector * Mathf.Clamp01(animTime * distance * 3f);
    }
    public static AmmoContainer CreateAmmoBelt(GunPreset gunPreset, int capacity, ObjectData data)
    {
        AmmoContainer belt = new GameObject(gunPreset.name + " Ammo Belt").AddComponent<Magazine>();
        belt.capacity = capacity;
        belt.gunPreset = gunPreset;
        belt.Initialize(data, true);
        return belt;
    }
}
