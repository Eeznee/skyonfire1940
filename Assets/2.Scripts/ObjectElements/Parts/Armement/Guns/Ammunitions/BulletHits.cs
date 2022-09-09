using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GunHits Preset", menuName = "Weapons/GunHits")]
public class BulletHits : ScriptableObject
{
    public GameObject mudHit;
    public GameObject sandHit;
    public GameObject stoneHit;
    public GameObject woodHit;
    public GameObject metalHit;
    public GameObject waterHit;
    public GameObject explosiveHit;
    public GameObject adjustingHit;
    public GameObject debris;
    public GameObject bulletHole;
    public float debrisChance = 1f;

    public AudioClip[] metalClips;
    public AudioClip[] explosionClips;

    public AudioClip GetClip(bool explosive)
    {
        if (explosive && explosionClips.Length > 0) return explosionClips[Random.Range(0, explosionClips.Length)];
        else return metalClips[Random.Range(0, metalClips.Length)];
    }
}
