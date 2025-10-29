using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class AircraftSwitcher : MonoBehaviour
{
    public bool playerAuto = true;
    public Text squadText;
    public Text wingText;
    [HideInInspector] public SofAircraft CurrentAircraft { get; set; }
    [HideInInspector] public SofModular CurrentSofModular { get; set; }

    public UnityEvent onValueChanged;

    public void OffsetSquadron(int offset)
    {
        if (!CurrentSofModular) SetCurrent(GameManager.squadrons[0][0]);
        else SetCurrent(Player.OffsetSquadron(offset, CurrentSofModular.SquadronId));
    }
    public void OffsetWing(int offset)
    {
        if (!CurrentSofModular) SetCurrent(GameManager.squadrons[0][0]);
        else SetCurrent(Player.OffsetWing(offset, CurrentSofModular.PlaceInSquad, CurrentSofModular.SquadronId));
    }
    public void SetCurrent(SofModular newSelection)
    {
        if (newSelection == CurrentSofModular) return;

        CurrentSofModular = newSelection;
        CurrentAircraft = newSelection.aircraft;
        onValueChanged.Invoke();

        if (CurrentAircraft)
        {
            squadText.text = "Squadron n°" + (CurrentAircraft.SquadronId + 1).ToString();
            wingText.text = CurrentAircraft.placeInSquad == 0 ? "Leader" : "Wing " + (CurrentAircraft.placeInSquad + 1).ToString();
        }
        else if (CurrentSofModular)
        {
            squadText.text = "Ground/Sea Units";
            wingText.text = CurrentSofModular.name;
        }
        else
        {
            squadText.text = "Player Null";
            wingText.text = "";
        }
    }

    public void SelectPlayerCamera()
    {
        float minAngle = 10f;

        SofObject nearest = null;
        foreach(SofModular so in GameManager.crewedModulars)
        {
            if (so == null) continue;
            float angle = Vector3.Angle(SofCamera.tr.forward, so.transform.position - SofCamera.tr.position);
            if (angle < minAngle)
            {
                minAngle = angle;
                nearest = so;
            }
        }

        foreach (SofObject so in GameManager.sofObjects)
        {
            if (so == null) continue;
            float angle = Vector3.Angle(SofCamera.tr.forward, so.transform.position - SofCamera.tr.position);
            if (angle < minAngle && so.aircraft)
            {
                minAngle = angle;
                nearest = so;
            }
        }

        if (nearest != null) SetCurrent(nearest.modular);
    }

    void Update()
    {
        if (playerAuto && CurrentSofModular != Player.sofObj) SetCurrent(Player.modular);
    }
}
