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
            if (PlayerManager.playerGunner && PlayerManager.playerGunner.turret) worldPos += PlayerManager.playerGunner.turret.FiringDirection() * 500f;
            else worldPos += PlayerManager.player.tr.forward * PlayerManager.player.aircraft.convergeance;
            transform.position = Camera.main.WorldToScreenPoint(worldPos);
            bool enabled = PlayerCamera.customCam.pos != CamPosition.FirstPerson;   //Camera must be external
            enabled &= PlayerCamera.customCam.player != PlayerIs.None;         //And must be controlling the player
            enabled &= !TimeManager.paused;                                         //The game must be playing
            enabled &= transform.position.z > 0f;                                   //The crosshair can't be behind
            enabled &= GameManager.seatInterface == SeatInterface.Pilot || GameManager.seatInterface == SeatInterface.Gunner;
            image.enabled = enabled;
        }
        else image.enabled = false;
    }
}
