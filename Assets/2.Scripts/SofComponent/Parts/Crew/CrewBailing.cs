using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CrewMember))]
[AddComponentMenu("Sof Components/Crew Seats/Crew Bailing")]
public class CrewBailing : MonoBehaviour
{
    public Parachute parachute;
    public Parachute specialPlayerParachute;

    private Parachute selectedParachute => specialPlayerParachute && Player.crew == crew ? specialPlayerParachute : parachute;

    private CrewMember crew;
    private SofAircraft aircraft => crew.aircraft;
    private Transform tr => crew.tr;

    private float bailingCount;
    private bool tryingToBail = false;

    const float bailTime = 3f;
    const float minBailAltitude = 30f;
    const float minCrashTime = 2f;

    private void Awake()
    {
        crew = GetComponent<CrewMember>();
    }
    private void AutoBail()
    {
        if (tryingToBail) return;
        bool aircraftDestroyed = aircraft && (aircraft.burning || aircraft.destroyed);
        bool notInPlayerAircraft = Player.aircraft != aircraft;
        if (aircraftDestroyed && notInPlayerAircraft) StartBailing(Random.Range(1f, 3f));
    }
    private bool CanBail()
    {
        bool canBail = crew.data.relativeAltitude.Get + crew.data.vsp.Get * minCrashTime > minBailAltitude;
        if (crew.Seat.canopy) canBail &= crew.Seat.canopy.state > 0.5f || crew.Seat.canopy.disabled;
        return canBail;
    }
    private void Update()
    {
        if (crew.ActionsUnavailable) return;

        AutoBail();

        if (tryingToBail && CanBail())
        {
            bailingCount -= Time.deltaTime;
            if (Player.crew == crew) Log.Print("Bailout in " + bailingCount.ToString("0.0") + " s", "bailout");

            if (bailingCount < 0f) BailInstant();
        }
    }

    public void StartBailing(float delay)
    {
        if (!aircraft || crew.ripped || tryingToBail) return;
        tryingToBail = true;
        if (crew.Seat.canopy) crew.Seat.canopy.Set(1f);
        bailingCount = bailTime + delay;
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
        if (crew.IsPilot) aircraft.destroyed = true;
        Instantiate(selectedParachute, tr.position, tr.rotation).TriggerParachute(aircraft, crew);
    }
}
