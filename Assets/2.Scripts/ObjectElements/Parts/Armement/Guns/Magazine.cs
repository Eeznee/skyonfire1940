using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magazine : CockpitInteractable
{
    public GunPreset gunPreset;
    public int capacity = 100;
    [HideInInspector] public int ammo;
    public Vector3 ejectVector = new Vector3(0f,0.2f,0f);
    public int[] markers;
    public GameObject[] markersGameObjects;
    public MeshRenderer rend;

    [HideInInspector] public Gun attachedGun;
    [HideInInspector] public MagazineStock attachedStock;
    Gun[] guns;

    public override float Mass()
    {
        return gunPreset.ammunition.FullMass * ammo / 1000f;
    }

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        guns = data.GetComponentsInChildren<Gun>();
        if (firstTime)
        {
            rend = GetComponentInChildren<MeshRenderer>();
            ammo = capacity;
        }
    }
    public bool EjectRound()
    {
        if (ammo <= 0) return false;
        ammo--;
        if (markers != null)
        {
            for (int i = 0; i < markers.Length; i++)
            {
                if (ammo < markers[i]) markersGameObjects[i].SetActive(false);
            }
        }
        return true;
    }

    protected override void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {
        transform.SetPositionAndRotation(gripPos, gripRot);

        //The magazine is removed initially
        if (attachedGun) attachedGun.magazine = null;
        attachedGun = null;

        //Check every gun to load into
        for (int i = 0; i < guns.Length; i++)
        {
            Gun g = guns[i];
            Vector3 magPos = g.transform.TransformPoint(g.magazineLocalPos);
            if (g.gunPreset == gunPreset && g.magazine == null)
            {
                if ((magPos - transform.position).sqrMagnitude < 0.01f)
                    g.LoadMagazine(this);//Attach Magazine to the gun
            }
        }
    }

    protected override void Animate()
    {
        if (attachedGun && sofObject)
        {
            transform.parent = attachedGun.transform;
            transform.localPosition = attachedGun.magazineLocalPos;
            transform.localRotation = Quaternion.identity;
        }
    }

    public Vector3 MagTravelPos(Vector3 startPos, Vector3 endPos,float animTime)
    {
        float distance = (startPos - endPos).magnitude;
        float t = animTime * animTime;
        if (animTime > 0.2f) t = Mathf.Lerp(0.04f,1f,(animTime-0.2f)/0.8f);
        Vector3 travelOffset = (endPos - (startPos + ejectVector)) * t;
        return travelOffset + startPos + ejectVector * Mathf.Clamp01(animTime*distance * 3f);
    }
}
