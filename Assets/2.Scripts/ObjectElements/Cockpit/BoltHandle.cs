using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
public class BoltHandle : CockpitInteractable
{
    public AudioClip boltPulls;
    public AudioClip boltLocks;
    public AudioClip boltReleases;
    public Gun gun;
    public float speed = 1f;
    public float maxDistance = 0.1f;
    [HideInInspector] public bool animatedPulling = false;

    

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            gun.bolt = this;
            gun.OnChamberEvent += PlayBoltRelease;
        }
    }
    protected override void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {
        Vector3 localPos = transform.parent.InverseTransformPoint(gripPos);
        float input = (defaultPos.z - localPos.z) / maxDistance;
        return;
        /*
        gun.Cycle(Mathf.Clamp01(input));
        if (boltState > 0.95f && !pulled) { Pull(); }
        if (boltState < 0.05f && gun && pulled) Release();
        */
    }
    private void PlayBoltRelease()
    {
        if (gun.gunPreset.openBolt && (!gun.magazine || gun.magazine.ammo == 0))
            PlayClip(boltReleases);
    }
    public void CycleBoltAnimation()
    {
        StartCoroutine(AnimatedPull());
    }
    private void PlayClip(AudioClip clip)
    {
        if (clip && sofObject == PlayerManager.player.sofObj && (PlayerManager.player.crew.transform.position - transform.position).sqrMagnitude < 4f) 
            sofObject.avm.persistent.local.PlayOneShot(clip, 1f);
    }

    IEnumerator AnimatedPull()
    {
        animatedPulling = true;
        float cycle = 0f;
        PlayClip(boltPulls);
        while (cycle < 0.5f)
        {
            cycle = Mathf.MoveTowards(cycle, 0.5f, Time.deltaTime / gun.gunPreset.cyclingTime);
            gun.ManualCycle(cycle);
            yield return null;
        }
        PlayClip(boltLocks);

        animatedPulling = false;
    }

    private void Update()
    {
        CockpitInteractableUpdate();
        transform.localPosition = defaultPos - Mathf.PingPong(Mathf.Clamp01(gun.cycleState * speed) * 2f , 1f) * Vector3.forward * maxDistance;
    }
}