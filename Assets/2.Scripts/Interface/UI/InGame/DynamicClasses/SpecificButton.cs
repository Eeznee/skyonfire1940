using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecificButton : DynamicUI
{
    public enum Spec { Bomb,Bay,Guns,Multicrew,Airbrakes,Reload, Multiseats}

    public Spec spec = Spec.Bomb;

    public override bool IsActive()
    {
        SofAircraft p = GameManager.player.aircraft;
        if (p == null) return false;
        switch (spec)
        {
            case Spec.Bomb:
                return p.card.bomb;
            case Spec.Bay:
                return p.card.bombBay;
            case Spec.Guns:
                return p.card.forwardGuns;
            case Spec.Multiseats:
                return GameManager.player.crew.seats.Length > 1;
            case Spec.Multicrew:
                return p.crew.Length > 1;
            case Spec.Airbrakes:
                return p.card.airbrakes;
            case Spec.Reload:
                return p.crew[0].seats[0].reloadableGuns.Length > 0;
        }
        return true;
    }
}
