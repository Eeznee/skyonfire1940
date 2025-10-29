using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CrewMember))]
[AddComponentMenu("Sof Components/Crew Seats/Crew Bailing")]
public class CrewBailing : SofComponent
{
    public Parachute parachute;
    public Parachute specialPlayerParachute;

    private Parachute selectedParachute => specialPlayerParachute && Player.crew == crew ? specialPlayerParachute : parachute;

    private CrewMember crew;

    private float bailingCount;
    private bool tryingToBail = false;

    const float bailTime = 3f;
    const float minBailAltitude = 30f;
    const float minCrashTime = 2f;

    public override void SetReferences(SofModular _modular)
    {
        if (aircraft)
        {
            aircraft.OnStartBurning -= CheckAutoBail;
            aircraft.OnDestroyed -= CheckAutoBail;
        }
        base.SetReferences(_modular);
    }
    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);
        crew = GetComponent<CrewMember>();

        if (aircraft)
        {
            aircraft.OnStartBurning += CheckAutoBail;
            aircraft.OnDestroyed += CheckAutoBail;
        }
    }

    private void CheckAutoBail()
    {
        if (tryingToBail) return;
        if (Player.aircraft == aircraft && Player.controllingPlayer) return;
        if (crew.ripped) return;

        if (aircraft && (aircraft.Burning || aircraft.Destroyed)) StartBailing(Random.Range(1f, 3f));
    }
    private bool CanBail()
    {
        bool canBail = crew.data.relativeAltitude.Get + crew.data.vsp.Get * minCrashTime > minBailAltitude;
        if (crew.Seat.canopy) canBail &= crew.Seat.canopy.state > 0.5f || crew.Seat.canopy.disabled;
        return canBail && !crew.ActionsUnavailable;
    }
    IEnumerator BailingSequence()
    {
        while(bailingCount > 0f)
        {
            if (!tryingToBail) yield break;

            if(CanBail()) bailingCount -= Time.deltaTime;

            if (Player.crew == crew) Log.Print("Bailout in " + bailingCount.ToString("0.0") + " s", "bailout");

            yield return null;
        }
        BailInstant();
    }

    public void StartBailing(float delay)
    {
        if (!aircraft || crew.ripped || tryingToBail) return;

        tryingToBail = true;
        if (crew.Seat.canopy) crew.Seat.canopy.Set(1f);
        bailingCount = bailTime + delay;

        StartCoroutine(BailingSequence());
    }
    public void CancelBailing()
    {
        if (!aircraft || crew.ripped) return;

        tryingToBail = false;
        if (crew.Seat.canopy) crew.Seat.canopy.Set(0f);
    }
    public void BailInstant()
    {
        if (!aircraft || crew.ripped) return;

        tryingToBail = false;
        if (crew.IsPilot) aircraft.Destroy();
        Instantiate(selectedParachute, tr.position, tr.rotation).TriggerParachute(aircraft, crew);
    }
}
