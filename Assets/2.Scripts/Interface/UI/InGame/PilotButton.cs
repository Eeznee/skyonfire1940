using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PilotButton : MonoBehaviour
{
    public Image image;

    SofAircraft playerAircraft;
    void Update()
    {
        if (playerAircraft != GameManager.player.aircraft)
        {
            playerAircraft = GameManager.player.aircraft;
            image.sprite = playerAircraft.card.icon;
        }
    }
}
