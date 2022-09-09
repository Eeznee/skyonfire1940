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
        if (GameManager.player.aircraft && !GameManager.player.crew.ripped && !GameManager.player.crew.body.Gloc())
        {
            Vector3 worldPos = GameManager.player.crew.transform.position;
            if (GameManager.playerGunner && GameManager.playerGunner.turret) worldPos += GameManager.playerGunner.turret.FiringDirection() * 500f;
            else worldPos += GameManager.player.tr.forward * GameManager.player.aircraft.convergeance;
            transform.position = Camera.main.WorldToScreenPoint(worldPos);
            bool enabled = PlayerCamera.customCam.pos != CamPosition.FirstPerson;   //Camera must be external
            enabled &= PlayerCamera.customCam.player != PlayerIs.None;         //And must be controlling the player
            enabled &= !GameManager.paused;                                         //The game must be playing
            enabled &= transform.position.z > 0f;                                   //The crosshair can't be behind
            enabled &= GameManager.seatInterface == SeatInterface.Pilot || GameManager.seatInterface == SeatInterface.Gunner;
            image.enabled = enabled;
        }
        else image.enabled = false;
    }
}
