using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Bullet Hits", menuName = "SOF/Weapons/Bullet Hits")]
public class BulletHits : ScriptableObject
{
    public GameObject mudHit;
    public GameObject sandHit;
    public GameObject stoneHit;
    public GameObject woodHit;
    public GameObject metalHit;
    public GameObject waterHit;
    public GameObject incendiaryHit;
    public GameObject debris;
    public GameObject bulletHole;
    public float debrisChance = 1f;
    public ExplosionFX explosion;

    public AudioClip[] metalClips;
    public AudioClip[] explosionClips;

    public AudioClip GetClip(bool explosive)
    {
        if (explosive && explosionClips.Length > 0) return explosionClips[Random.Range(0, explosionClips.Length)];
        else return metalClips[Random.Range(0, metalClips.Length)];
    }

    public GameObject EffectByMat(string material)
    {
        material = material.Replace(" (Instance)", "");
        switch (material)
        {
            case "":
                Debug.LogError("The collider : " + material + " has no physical material assiocated");
                return woodHit;
            case "Mud":
                return mudHit;
            case "Sand":
                return sandHit;
            case "Stone":
                return stoneHit;
            case "Wood":
                return woodHit;
            case "Metal":
                return metalHit;
            case "Water":
                return waterHit;
            case "Wheel":
                return woodHit;
        }
        Debug.LogError("The physic material : " + material + " has no effect associated");
        return woodHit;
    }

    public void CreateHit(Material material, Vector3 pos, Quaternion rot, Transform tr)
    {
        string matName = material.name.Replace(" (Instance)", "");
        CreateHit(matName, pos, rot, tr);
    }
    public void CreateHit(string material, Vector3 pos, Quaternion rot, Transform tr)
    {
        CreateHit(EffectByMat(material), pos, rot, tr,false);
    }
    public void CreateHit(GameObject effect, Vector3 pos, Quaternion rot, Transform tr, bool permanent)
    {
        GameObject obj;
        if (tr) obj = Instantiate(effect, pos, rot, tr);
        else obj = Instantiate(effect, pos, rot);
        if (!permanent) Destroy(obj, 10f);
    }
    public void AircraftHit(bool incendiary, Vector3 pos, Vector3 normal, Transform tr)
    {
        CreateHit(incendiary ? incendiaryHit : metalHit, pos, Quaternion.LookRotation(normal), tr,false);
        CreateHit(bulletHole, pos + normal.normalized * 0.05f, Quaternion.LookRotation(-normal), tr, true);
        for (float chance = debrisChance; chance > 0f; chance--)
            if (chance >= 1f || Random.value < chance)
                CreateHit(debris, pos, Quaternion.LookRotation(normal), null, false);

        if (Player.tr == tr.root)
            Player.complex.avm.persistent.local.PlayOneShot(GetClip(false), 1f);
    }
    public void AircraftHit(bool incendiary, RaycastHit hit)
    {
        AircraftHit(incendiary, hit.point, hit.normal, hit.collider.transform);
    }
}
