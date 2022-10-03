using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingCrosshair : MonoBehaviour
{
    public Transform dynamic;
    public Transform level;
    void LateUpdate()
    {
        if (GameManager.player.aircraft && !GameManager.player.crew.ripped && !GameManager.player.crew.body.Gloc())
        {
            Vector3 pos = Camera.main.WorldToScreenPoint(GameManager.player.tr.position + PlayerCamera.directionInput * GameManager.player.aircraft.convergeance);
            transform.position = pos;
            level.rotation = Quaternion.Euler(0f, 0f, -PlayerCamera.instance.camTr.eulerAngles.z);

            bool active = pos.z > 0f;
            active &= !GameManager.paused;
            active &= PlayerCamera.customCam.WorldLookAround();
            active &= GameManager.seatInterface != SeatInterface.Bombardier;
            active &= !(GameManager.seatInterface == SeatInterface.Gunner && PlayerCamera.customCam.pos == CamPosition.FirstPerson);
            bool dyn = PlayerCamera.dynamic || (GameManager.seatInterface == SeatInterface.Gunner);
            dynamic.gameObject.SetActive(dyn && active);
            level.gameObject.SetActive(!dyn && active);
        } else
        {
            dynamic.gameObject.SetActive(false);
            level.gameObject.SetActive(false);
        }
    }
}
