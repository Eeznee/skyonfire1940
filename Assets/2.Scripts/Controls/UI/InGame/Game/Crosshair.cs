using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Crosshair : MonoBehaviour
{
    Image image;
    private void Start()
    {
        image = GetComponent<Image>();
    }
    void LateUpdate()
    {
        if (Player.aircraft && !Player.crew.ripped && !Player.crew.humanBody.Gloc())
        {
            Vector3 position = Player.seat.CrosshairPosition;
            transform.position = SofCamera.cam.WorldToScreenPoint(position);
            bool enabled = SofCamera.viewMode != 1;      //Camera must be external
            enabled &= Player.crew;                                                 //And must be controlling the player
            enabled &= !TimeManager.paused;                                         //The game must be playing
            enabled &= transform.position.z > 0f;                                   //The crosshair can't be behind
            enabled &= Player.role == SeatRole.Pilot || Player.role == SeatRole.Gunner;
            image.enabled = enabled;
        }
        else image.enabled = false;
    }
}
