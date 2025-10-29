using System.Collections;
using UnityEngine;

public class BungeeBombBay : SofComponent
{
    [System.Serializable]
    public class BayDoor
    {
        public Transform transform;
        public Vector3 openedRotation;

        [HideInInspector]public Quaternion openedQuaternion;
    }

    public float animTime = 0.5f;
    public BayDoor[] doors;

    public AudioClip onCloseClip;
    public float onCloseVolume = 1f;
    public AudioClip onOpenClip;
    public float onOpenVolume = 1f;

    private bool animating;
    private float animationState;

    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);

        foreach (BayDoor door in doors)
        {
            door.openedQuaternion = Quaternion.Euler(door.openedRotation);
        }

        animating = false;
        Animate(0f);

        BombsLoad bombsLoad = GetComponentInChildren<BombsLoad>();
        if (bombsLoad) bombsLoad.OnOrdnanceLaunched += OnBombDropped;
    }

    private void OnBombDropped()
    {
        if (!animating) StartCoroutine(DoorsAnimation());
        else if (animationState > 0.4f)
        {
            //a bombs interrupts the animation and hits the bomb bay, playing it again, can only happen if the bomb bay is closed enough

            if (onOpenClip) sofModular.objectAudio.PlayAudioClip(onOpenClip, onOpenVolume, SofAudioGroup.Persistent, false);
            animationState = -animationState;
        }
    }


    private IEnumerator DoorsAnimation()
    {
        if (onOpenClip) sofModular.objectAudio.PlayAudioClip(onOpenClip, onOpenVolume, SofAudioGroup.Persistent, false);

        animating = true;
        animationState = -1f;

        do
        {
            animationState += 2f * Time.deltaTime / animTime;

            float realState = 1f - Mathf.Abs(animationState);
            realState = Mathv.SmoothStop(realState, 2);

            Animate(realState);

            yield return null;


        } while (animationState < 1f);

        Animate(0f);
        animating = false;
        if (onCloseClip) sofModular.objectAudio.PlayAudioClip(onCloseClip, onCloseVolume, SofAudioGroup.Persistent, false);
    }

    public void Animate(float state)
    {
        foreach (BayDoor door in doors)
        {
            door.transform.localRotation = Quaternion.Lerp(Quaternion.identity, door.openedQuaternion, state);
        }
    }
}
