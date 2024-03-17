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
                    return p.bombs.Length > 0;
                case Spec.Rockets:
                    return p.rockets.Length > 0;
                case Spec.Bay:
                    return p.bombBay;
                case Spec.Guns:
                    return p.primaries.Length > 0 || p.secondaries.Length > 0;
                case Spec.Multiseats:
                    return Player.crew.seats.Length > 1;
                case Spec.Multicrew:
                    return p.crew.Length > 1;
                case Spec.Airbrakes:
                    return p.airBrakes;
            }
        }
        switch (spec)
        {
            case Spec.Reload:
                return s.reloadableGuns.Length > 0;
            case Spec.Pilot:
                return Player.seatInterface == SeatInterface.Pilot;
            case Spec.Bomber:
                return Player.seatInterface == SeatInterface.Bombardier;
            case Spec.Gunner:
                return Player.seatInterface == SeatInterface.Gunner;
            case Spec.Empty:
                return Player.seatInterface == SeatInterface.Empty;
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
