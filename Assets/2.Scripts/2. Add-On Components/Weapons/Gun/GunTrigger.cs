using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Gun))]
public class GunTrigger : MonoBehaviour
{
    public enum TriggerStage
    {
        Off,
        On,
        WaitingForReset
    }

    private bool fullAuto;
    private TriggerStage stage = TriggerStage.Off;
    private bool triggeredThisFrame = false;
    private Gun gun;

    public bool On()
    {
        return stage == TriggerStage.On;
    }
    private void Awake()
    {
        gun = GetComponent<Gun>();
        fullAuto = GetComponent<Gun>().gunPreset.fullAuto;
        if (!fullAuto) GetComponent<Gun>().OnEjectEvent += LockForSingleFire;
    }
    public void TriggerThisFrame()
    {
        if (stage == TriggerStage.Off)
        {
            stage = TriggerStage.On;
            gun.OnTriggerEvent();
        }

        triggeredThisFrame = true;
    }
    private void Update()
    {
        if (!triggeredThisFrame) stage = TriggerStage.Off;

        triggeredThisFrame = false;
    }
    private void LockForSingleFire()
    {
        stage = TriggerStage.WaitingForReset;
    }
}
