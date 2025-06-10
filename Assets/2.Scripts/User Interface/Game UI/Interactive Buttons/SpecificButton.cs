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
        SofAircraft aircraft = Player.aircraft;
        CrewSeat seat = Player.seat;
        if (aircraft != null)
        {
            switch (spec)
            {
                case Spec.Bomb:
                    return aircraft.armament.bombs.Length > 0 || aircraft.armament.torpedoes.Length > 0;
                case Spec.Rockets:
                    return aircraft.armament.rockets.Length > 0;
                case Spec.Bay:
                    return aircraft.hydraulics.bombBay;
                case Spec.Guns:
                    return aircraft.armament.primaries.Length > 0 || aircraft.armament.secondaries.Length > 0;
                case Spec.Airbrakes:
                    return aircraft.hydraulics.airBrakes;
            }
        }
        switch (spec)
        {
            case Spec.Multiseats:
                return Player.crew.seats.Count > 1;
            case Spec.Multicrew:
                return Player.modular.crew.Length > 1;
            case Spec.Pilot:
                return Player.role == SeatRole.Pilot;
            case Spec.Bomber:
                return Player.role == SeatRole.Bombardier;
            case Spec.Gunner:
                return Player.role == SeatRole.Gunner;
            case Spec.Empty:
                return Player.role == SeatRole.Simple;
        }

        if(seat != null && spec == Spec.Reload) return seat.reloadableGuns.Length > 0;

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
