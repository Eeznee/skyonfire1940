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
        SofAircraft p = Player.aircraft;
        CrewSeat s = Player.seat;
        if (p != null && s != null)
        {
            switch (spec)
            {
                case Spec.Bomb:
                    return p.armament.bombs.Length > 0;
                case Spec.Rockets:
                    return p.armament.rockets.Length > 0;
                case Spec.Bay:
                    return p.hydraulics.bombBay;
                case Spec.Guns:
                    return p.armament.primaries.Length > 0 || p.armament.secondaries.Length > 0;
                case Spec.Multiseats:
                    return Player.crew.seats.Count > 1;
                case Spec.Multicrew:
                    return p.crew.Length > 1;
                case Spec.Airbrakes:
                    return p.hydraulics.airBrakes;
            }
        }
        switch (spec)
        {
            case Spec.Reload:
                return s.reloadableGuns.Length > 0;
            case Spec.Pilot:
                return Player.role == SeatRole.Pilot;
            case Spec.Bomber:
                return Player.role == SeatRole.Bombardier;
            case Spec.Gunner:
                return Player.role == SeatRole.Gunner;
            case Spec.Empty:
                return Player.role == SeatRole.Simple;
        }
        return false;
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
