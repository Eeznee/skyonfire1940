using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AircraftSwitcher : MonoBehaviour
{
    public bool playerAuto = true;
    public Text squadText;
    public Text wingText;
    [HideInInspector] public SofAircraft current;

    public void OffsetSquadron(int offset)
    {
        if (!current) current = GameManager.squadrons[0][0];
        current = GameManager.OffsetSquadron(offset, current.squadronId);
    }
    public void OffsetWing(int offset)
    {
        if (!current) current = GameManager.squadrons[0][0];
        current = GameManager.OffsetWing(offset, current.placeInSquad,current.squadronId);
    }

    public void SelectPlayerCamera()
    {
        Transform camTr = PlayerCamera.instance.camTr;
        float minAngle = 10f;
        SofObject nearest = null;
        foreach(SofObject so in GameManager.sofObjects)
        {
            float angle = Vector3.Angle(camTr.forward, so.transform.position - camTr.position);
            if (angle < minAngle && so.data.aircraft)
            {
                minAngle = angle;
                nearest = so;
            }
        }
        if (nearest != null) current = nearest.data.aircraft;
    }

    void Update()
    {
        if (playerAuto) current = GameManager.ogPlayer.aircraft;

        if (current)
        {
            squadText.text = "Squadron n°" + (current.squadronId + 1).ToString();
            wingText.text = current.placeInSquad == 0 ? "Leader" : "Wing " + (current.placeInSquad+1).ToString();
        } else
        {
            squadText.text = "Player Null"; 
            wingText.text = "";
        }
    }
}
