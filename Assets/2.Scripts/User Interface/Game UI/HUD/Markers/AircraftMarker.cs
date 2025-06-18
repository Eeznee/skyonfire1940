using System;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(RectTransform))]
public class AircraftMarker : Marker
{
    protected SofAircraft targetAircraft;

    protected override Vector3 TargetPos => target.transform.position - target.transform.forward * targetAircraft.stats.wingSpan * 0.15f;
    protected override float TargetDimensions => targetAircraft.stats.wingSpan * 0.5f;

    public override void Init(SofObject _target)
    {
        base.Init(_target);
        targetAircraft = target.aircraft;
    }
    protected override Color MarkerColor()
    {
        if (target.Destroyed) return Color.black;

        Color color = base.MarkerColor();

        if (Player.aircraft != null)
        {
            if (Player.aircraft && targetAircraft.SquadronId == Player.aircraft.SquadronId) color = Color.green;
            if (Player.aircraft == targetAircraft) color = Color.yellow;
        }

        return color;
    }
    protected override string TextToShow(float distance)
    {
        if (!targetAircraft) return "";

        string name = targetAircraft.card.completeName;
        string distanceTxt = (UnitsConverter.distance.Multiplier * distance).ToString("0.00") + " " + UnitsConverter.distance.Symbol;
        string difficultyTxt = (targetAircraft.Difficulty * 100f).ToString("0");

        string txt = name + "\n" + difficultyTxt + "  " + distanceTxt + "\n";

        if (UIManager.gameUI == GameUI.CamEditor)
        {
            txt += "Sqdr " + (targetAircraft.SquadronId + 1);
            txt += targetAircraft.placeInSquad == 0 ? " Leader" : " Wing " + targetAircraft.placeInSquad;
        }
        else if (targetAircraft && targetAircraft.crew[0] != null)
        {
            txt += targetAircraft.crew[0].Seat.Action;
        }
        return txt;
    }
}