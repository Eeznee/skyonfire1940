using UnityEngine;
using UnityEngine.Events;


public class ActionWheelSelection : MonoBehaviour
{
    public enum Condition
    {
        ActiveAtAllTimes,
        MultiCrew,
        MultiSeats,
        Bombardier,
        HasLandingGear,
        HasCanopy,
        HasAirbrakes,
        HasBombBay,
        PlayerAircraftNotNull
    }
    public Condition activeCondition;
    public UnityEvent OnSelected;


    public bool WheelSelectionActive()
    {
        if (activeCondition == Condition.ActiveAtAllTimes) return true;

        if (Player.aircraft == null) return false;
        if (Player.crew == null) return false;


        switch (activeCondition)
        {
            case Condition.MultiCrew:
                return Player.aircraft.crew.Length > 1;
            case Condition.MultiSeats:
                return Player.crew.seats.Count > 1;
            case Condition.Bombardier:
                return Player.aircraft.bombardierSeat != null;
            case Condition.HasLandingGear:
                return Player.aircraft.hydraulics.gear != null;
            case Condition.HasCanopy:
                return Player.aircraft.hydraulics.canopy != null;
            case Condition.HasAirbrakes:
                return Player.aircraft.hydraulics.airBrakes != null;
            case Condition.HasBombBay:
                return Player.aircraft.hydraulics.bombBay != null;
        }

        return true;
    }

    public void InvokeCustomAction()
    {
        OnSelected.Invoke();
    }
}
