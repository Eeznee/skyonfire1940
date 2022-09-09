using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
public class BoltHandle : CockpitInteractable
{
    public AudioClip cockingPull;
    public AudioClip cockingRelease;
    public Gun gun;
    public float maxDistance = 0.1f;
    private float boltState = 0f;
    private float targetBoltState = 0f;
    private bool pulled = false;
    private bool animatedPulling = false;

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        gun.bolt = this;
    }
    protected override void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {
        Vector3 localPos = transform.parent.InverseTransformPoint(gripPos);
        float input = (defaultPos.z - localPos.z) / maxDistance;
        boltState = Mathf.Clamp01(input);
        if (boltState > 0.95f && !pulled) { Pull(); }
        if (boltState < 0.05f && gun && pulled) Release();
    }
    private void Pull()
    {
        gun.Eject();
        pulled = true;
        sofObject.avm.cockpit.local.PlayOneShot(cockingPull, 1f);
    }
    private void Release()
    {
        gun.Chamber();
        sofObject.avm.cockpit.local.PlayOneShot(cockingRelease, 1f);
        pulled = false;
    }
    protected override void Animate()
    {
        if (!xrGrip.isSelected && !animatedPulling)
        {
            float previousBoltState = boltState;
            boltState = Mathf.MoveTowards(boltState, targetBoltState, Time.deltaTime * gun.gunPreset.FireRate / 30f);
            float left = Mathf.Abs(Mathf.Abs(previousBoltState - boltState) - Time.deltaTime * gun.gunPreset.FireRate / 30f);
            if (!gun.Firing()) targetBoltState = gun.gunPreset.openBolt && gun.chambered ? 1f : 0f;
            else if (boltState == targetBoltState) targetBoltState = boltState == 1f ? 0f : 1f;
            boltState = Mathf.MoveTowards(boltState, targetBoltState, left);
        }

        transform.localPosition = defaultPos - boltState * Vector3.forward * maxDistance;
    }

    public void CycleBoltAnimation()
    {
        StartCoroutine(AnimatedPull());
    }

    IEnumerator AnimatedPull()
    {
        animatedPulling = true;
        float count = 0f;
        while (count < gun.gunPreset.cyclingTime * 0.5f)
        {
            boltState = 2f * count / gun.gunPreset.cyclingTime;
            count += Time.deltaTime;
            yield return null;
        }
        Pull();
        yield return new WaitForSeconds(gun.gunPreset.cyclingTime * 0.5f);
        if (!gun.gunPreset.openBolt) Release();
        animatedPulling = false;
    }

    private void Update()
    {
        CockpitInteractableUpdate();
    }
}