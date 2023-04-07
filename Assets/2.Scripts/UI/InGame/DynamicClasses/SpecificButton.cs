using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SpecificButton : DynamicUI
{
    public enum Spec { Bomb,Bay,Guns,Multicrew,Airbrakes,Reload, Multiseats, Rockets, Pilot, Bomber, Gunner, Empty}

    public Spec spec = Spec.Bomb;

    public override bool IsActive()
    {
        SofAircraft p = PlayerManager.player.aircraft;
        CrewMember c = PlayerManager.player.crew;

        if (p == null) return false;
        switch (spec)
        {
            case Spec.Bomb:
                return p.bombs.Length > 0;
            case Spec.Rockets:
                return p.rockets.Length > 0;
            case Spec.Bay:
                return p.bombBay;
            case Spec.Guns:
                return p.primaries.Length > 0 || p.secondaries.Length > 0;
            case Spec.Multiseats:
                return PlayerManager.player.crew.seats.Length > 1;
            case Spec.Multicrew:
                return p.crew.Length > 1;
            case Spec.Airbrakes:
                return p.airBrakes;
            case Spec.Reload:
                return c.Seat().reloadableGuns.Length > 0;
            case Spec.Pilot:
                return c.Interface() == SeatInterface.Pilot;
            case Spec.Bomber:
                return c.Interface() == SeatInterface.Bombardier;
            case Spec.Gunner:
                return c.Interface() == SeatInterface.Gunner;
            case Spec.Empty:
                return c.Interface() == SeatInterface.Empty;
        }
        return true;
    }
    public void EnableLate()
    {
        StartCoroutine(EnableLateCoroutine());
    }
    IEnumerator EnableLateCoroutine()
    {
        GetComponent<Image>().enabled = false;
        yield return new WaitForSeconds(0.1f);
        GetComponent<Image>().enabled = true;
    }

}
