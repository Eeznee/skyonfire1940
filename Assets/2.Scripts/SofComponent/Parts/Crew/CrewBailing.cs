using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrewBailing
{
    private CrewMember crew;
    private SofAircraft aircraft;
    private Transform tr;

    private float bailingCount;
    private bool tryingToBail = false;

    const float bailTime = 3f;
    const float minBailAltitude = 30f;
    const float minCrashTime = 2f;

    public CrewBailing(CrewMember _crew)
    {
        crew = _crew;
        aircraft = crew.aircraft;
        tr = crew.tr;
    }
    private void AutoBail()
    {
        if (tryingToBail) return;
        bool aircraftDestroyed = aircraft && (aircraft.burning || aircraft.destroyed);
        bool notInPlayerAircraft = Player.aircraft != aircraft;
        if (aircraftDestroyed && notInPlayerAircraft) Start(Random.Range(1f, 3f));
    }
    private bool CanBail()
    {
        bool canBail = crew.data.relativeAltitude.Get + crew.data.vsp.Get * minCrashTime > minBailAltitude;
        if (crew.seat.canopy) canBail &= crew.seat.canopy.state > 0.5f || crew.seat.canopy.disabled;
        return canBail;
    }
    public void Update()
    {
        AutoBail();

        if (tryingToBail && CanBail())
        {
            bailingCount -= Time.deltaTime;
            if (Player.crew == crew) Log.Print("Bailout in " + bailingCount.ToString("0.0") + " s", "bailout");

            if (bailingCount < 0f) BailInstant();
        }
    }

    public void Start(float delay)
    {
        if (!aircraft || crew.ripped || tryingToBail) return;
        tryingToBail = true;
        if (crew.seat.canopy) crew.seat.canopy.Set(1f);
        bailingCount = bailTime + delay;
    }
    public void Cancel()
    {
        if (!aircraft || crew.ripped) return;
        tryingToBail = false;
        if (crew.seat.canopy) crew.seat.canopy.Set(0f);
    }
    public void BailInstant()
    {
        if (!aircraft || crew.ripped) return;
        tryingToBail = false;
        if (crew.IsPilot) aircraft.destroyed = true;
        Parachute para = crew.specialPlayerParachute && Player.crew == crew ? crew.specialPlayerParachute : crew.parachute;
        GameObject.Instantiate(para, tr.position, tr.rotation).TriggerParachute(aircraft, crew);
    }
}
