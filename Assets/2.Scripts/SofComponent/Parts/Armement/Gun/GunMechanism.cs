using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunMechanism : MonoBehaviour
{
    public enum RoundState
    {
        None,
        Casing,
        HotRound
    }
    public enum Movement
    {
        Locked,
        Opening,
        Closing,
        Forced
    }
    //References
    private GunTrigger trigger;
    private Gun gun;
    private GunPreset gunPreset;

    //State
    public RoundState roundState { get; private set; }
    public Movement movement { get; private set; }
    public float cycleState { get; private set; }

    const float invert30 = 1f / 30f;

    private void Awake()
    {
        gun = GetComponent<Gun>();
        trigger = GetComponent<GunTrigger>();
        gunPreset = gun.gunPreset;

        gun.OnTriggerEvent += OnTrigger;
    }
    public void ResetMechanism()
    {
        cycleState = gunPreset.openBolt ? 0f : 1f;
        roundState = gunPreset.openBolt ? RoundState.None : RoundState.HotRound;
        movement = Movement.Locked;
    }

    private void FixedUpdate()
    {
        AutoCycle(Time.fixedDeltaTime);
    }
    public void ForceCycle(float state)
    {
        movement = Movement.Forced;
        Cycle(state);
    }
    public void CancelForceCycle()
    {
        if (cycleState <= 0f) TryLock();
    }
    private void AutoCycle(float deltaTime)
    {
        if (movement == Movement.Forced) return;
        if (movement == Movement.Locked) Cycle(cycleState);
        else
        {
            float cycleChange = gunPreset.FireRate * invert30 * deltaTime;
            Cycle(cycleState + (movement == Movement.Opening ? -cycleChange : cycleChange));
        }
    }
    private void Cycle(float newCycleState)
    {
        cycleState = newCycleState;

        if(cycleState < 0.3f) TryEject();

        while (movement != Movement.Locked && Mathf.Clamp01(cycleState) != cycleState)
        {
            TryEject();

            if (cycleState <= 0f)
            {
                TryLock();
                cycleState = movement == Movement.Closing ? -cycleState : 0f;
            }

            TryPullFreshRound();

            if (cycleState >= 1f)
            {
                TryFire();
                cycleState = movement == Movement.Opening ? 2f - cycleState : 1f;
            }
        }
    }
    private void TryEject()
    {
        if (movement == Movement.Closing || movement == Movement.Locked || roundState == RoundState.None) return;
        gun.OnEjectEvent?.Invoke();
        roundState = RoundState.None;
    }
    private void TryLock()
    {
        bool locked = BoltCatch() || (!trigger.On() && gunPreset.openBolt);
        movement = locked ? Movement.Locked : Movement.Closing;
        if (locked) gun.OnLockOpenEvent?.Invoke();
    }
    private void TryPullFreshRound()
    {
        if (movement == Movement.Opening || movement == Movement.Locked || roundState != RoundState.None) return;
        if (gun.Jam()) roundState = RoundState.Casing;
        else if (gun.magazine && gun.magazine.EjectRound())
        {
            roundState = RoundState.HotRound;
            gun.OnChamberRoundEvent?.Invoke();
        }
    }
    private void TryFire()
    {
        bool firing = roundState == RoundState.HotRound;
        if (gunPreset.openBolt) firing &= movement == Movement.Closing;
        else firing &= trigger.On();
        if (firing)
        {
            float excessCycle = cycleState - 1f;
            float delay = excessCycle * 30f / gunPreset.FireRate;
            gun.OnFireEvent?.Invoke(delay);
            movement = Movement.Opening;
        }

        else
        {
            if (roundState == RoundState.None) gun.OnSlamChamberEvent?.Invoke();
            movement = Movement.Locked;
        }
    }
    private void OnTrigger()
    {
        if (gunPreset.openBolt && cycleState == 0f && !BoltCatch()) movement = Movement.Closing;
        if (!gunPreset.openBolt && cycleState == 1f && roundState == RoundState.HotRound) movement = Movement.Closing;
    }
    private bool BoltCatch()
    {
        return gunPreset.boltCatch && (!gun.magazine || gun.magazine.ammo <= 0);
    }
    public bool MustBeCocked()
    {
        bool boltSlammedWithNoRound = movement == Movement.Locked && cycleState == 1f && roundState != RoundState.HotRound;
        bool boltLockedOpen = !gunPreset.openBolt && movement == Movement.Locked && cycleState == 0f;
        return boltSlammedWithNoRound || boltLockedOpen;
    }
    public bool IsFiring()
    {
        return movement != Movement.Locked && movement != Movement.Forced;
    }
}
