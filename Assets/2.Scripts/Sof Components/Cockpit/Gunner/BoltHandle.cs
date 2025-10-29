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
        float sqrMagnitude = (Player.crew.transform.position - transform.position).magnitude;
        float maxDis = 2.5f;

        if (clip && sofObject == Player.sofObj && sqrMagnitude < maxDis)
        {
            float volume = 1f - sqrMagnitude / maxDis;
            sofModular.objectAudio.PlayAudioClip(clip, volume, SofAudioGroup.Cockpit, false);
        }

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
        float currentState = 1f - gun.mechanism.cycleState;

        if (speed > 2f)
        {
            bool opening = gun.mechanism.movement == GunMechanism.Movement.Opening;

            if (opening) currentState = Mathf.PingPong(Mathf.Clamp(currentState * speed, 0f, 2f),1f);
            else currentState = 0f;
        }


        transform.localPosition = defaultPos + new Vector3(0f,0f,-maxDistance * currentState);
    }
}