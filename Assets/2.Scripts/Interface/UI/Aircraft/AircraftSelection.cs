using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AircraftSelection : MonoBehaviour
{
    public Text aircraftName;
    public Image aircraftIcon;

    private AircraftCard card;
    private int aircraftId = 0;


    public void SendCard(AircraftCard _card)
    {
        card = _card;
        aircraftName.text = " " + card.completeName;
        aircraftIcon.sprite = card.icon;
        GetComponent<Button>().interactable = _card.Available();
        aircraftId = card.id;
    }

    public void Select()
    {
        GetComponentInParent<AircraftScrollableList>().Select(aircraftId);
    }
}
