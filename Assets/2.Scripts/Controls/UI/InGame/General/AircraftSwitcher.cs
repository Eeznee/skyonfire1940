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
    [HideInInspector] public SofAircraft Current { get; set; }


    public UnityEvent onValueChanged;

    public void OffsetSquadron(int offset)
    {
        if (!Current) Current = GameManager.squadrons[0][0];
        SetCurrent(Player.OffsetSquadron(offset, Current.squadronId));
    }
    public void OffsetWing(int offset)
    {
        if (!Current) Current = GameManager.squadrons[0][0];
        SetCurrent(Player.OffsetWing(offset, Current.placeInSquad, Current.squadronId));
    }
    public void SetCurrent(SofAircraft newSelection)
    {
        if (newSelection == Current) return;
        Current = newSelection;
        onValueChanged.Invoke();

        if (Current)
        {
            squadText.text = "Squadron n°" + (Current.squadronId + 1).ToString();
            wingText.text = Current.placeInSquad == 0 ? "Leader" : "Wing " + (Current.placeInSquad + 1).ToString();
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
        foreach(SofObject so in GameManager.sofObjects)
        {
            if (so == null) continue;
            float angle = Vector3.Angle(SofCamera.tr.forward, so.transform.position - SofCamera.tr.position);
            if (angle < minAngle && so.aircraft)
            {
                minAngle = angle;
                nearest = so;
            }
        }
        if (nearest != null) SetCurrent(nearest.aircraft);
    }

    void Update()
    {
        if (playerAuto && Current != Player.aircraft) SetCurrent(Player.aircraft);
    }
}
