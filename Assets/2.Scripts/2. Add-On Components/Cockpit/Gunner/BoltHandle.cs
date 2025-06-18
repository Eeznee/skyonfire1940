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



    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);

        gun.bolt = this;
        gun.OnSlamChamberEvent += SlamEmptyChamber;
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
    private void SlamEmptyChamber()
    {
        PlayClip(boltReleases);
    }
    public void CycleBoltAnimation()
    {
        StartCoroutine(AnimatedPull());
    }
    private void PlayClip(AudioClip clip)
    {
        if (clip && sofObject == Player.sofObj && (Player.crew.transform.position - transform.position).sqrMagnitude < 4f)
            sofModular.objectAudio.localClipsPlayer.PlayOneShot(clip, 1f);
    }

    IEnumerator AnimatedPull()
    {
        animatedPulling = true;
        float cycle = gun.mechanism.cycleState;
        PlayClip(boltPulls);
        do
        {
            cycle = Mathf.MoveTowards(cycle, 0f, Time.deltaTime / gun.gunPreset.cyclingTime);
            gun.mechanism.ForceCycle(cycle);
            yield return null;
        } while (cycle > 0f);
        gun.mechanism.CancelForceCycle();

        PlayClip(boltLocks);

        animatedPulling = false;
    }

    private void Update()
    {
        CockpitInteractableUpdate();
        transform.localPosition = defaultPos - (1f - gun.mechanism.cycleState) * Vector3.forward * maxDistance;
    }
}