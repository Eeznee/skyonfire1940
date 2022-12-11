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
        if (playerAircraft != PlayerManager.player.aircraft)
        {
            playerAircraft = PlayerManager.player.aircraft;
            image.sprite = playerAircraft.card.icon;
        }
    }
}
