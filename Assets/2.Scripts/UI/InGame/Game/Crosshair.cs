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
        if (PlayerManager.player.aircraft && !PlayerManager.player.crew.ripped && !PlayerManager.player.crew.body.Gloc())
        {
            Vector3 worldPos = PlayerManager.player.crew.transform.position;
            worldPos += PlayerManager.player.crew.Seat().CrosshairDirection();
            transform.position = Camera.main.WorldToScreenPoint(worldPos);
            bool enabled = PlayerCamera.subCam.pos != CamPosition.FirstPerson;   //Camera must be external
            enabled &= PlayerCamera.subCam.player != PlayerIs.None;         //And must be controlling the player
            enabled &= !TimeManager.paused;                                         //The game must be playing
            enabled &= transform.position.z > 0f;                                   //The crosshair can't be behind
            enabled &= PlayerManager.seatInterface == SeatInterface.Pilot || PlayerManager.seatInterface == SeatInterface.Gunner;
            image.enabled = enabled;
        }
        else image.enabled = false;
    }
}
