using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Game;

public static class SofAircraftActionsExtensions
{
    public static bool CanPairUp(this SofAircraft aircraft)
    {
        if (aircraft.card.forwardGuns || aircraft.placeInSquad % 2 == 0) return false;
        if (GameManager.squadrons[aircraft.SquadronId][aircraft.placeInSquad - 1].destroyed) return false;
        return true;
    }
}
