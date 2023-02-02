using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrackingCrosshair : MonoBehaviour
{
    public Image dynamic;
    public Image level;
    private bool previousDyn = false;
    private float delta;


    void LateUpdate()
    {
        if (PlayerManager.player.aircraft && !PlayerManager.player.crew.ripped && !PlayerManager.player.crew.body.Gloc())
        {
            Vector3 pos = Camera.main.WorldToScreenPoint(PlayerManager.player.tr.position + PlayerCamera.directionInput * PlayerManager.player.aircraft.convergeance);
            transform.position = pos;
            level.transform.rotation = Quaternion.Euler(0f, 0f, -PlayerCamera.camTr.eulerAngles.z);

            bool active = pos.z > 0f;
            active &= !TimeManager.paused;
            active &= GameManager.Controls() == ControlsMode.Tracking || PlayerManager.seatInterface == SeatInterface.Gunner;
            active &= !(PlayerManager.seatInterface == SeatInterface.Gunner && PlayerCamera.subCam.pos == CamPosition.FirstPerson);
            bool dyn = PlayerCamera.dynamic || (PlayerManager.seatInterface == SeatInterface.Gunner);
            dynamic.gameObject.SetActive(dyn && active);
            level.gameObject.SetActive(!dyn && active);

            //Change alpha to not hide the gunsight
            Color c = dynamic.color;
            if (PlayerManager.player.aircraft && PlayerCamera.subCam.pos == CamPosition.FirstPerson)
            {
                float angle = Vector3.Angle(PlayerCamera.directionInput, PlayerManager.player.aircraft.transform.forward);
                c.a = Mathf.Clamp01((angle - 2f) / 2f);
                c.a = Mathf.Max(c.a, 1f - delta);
            } else c.a = 1f;

            dynamic.color = level.color = c;

            delta += Time.deltaTime;
            if (previousDyn != dyn)
            {
                delta = 0f;
                previousDyn = dyn;
            }

        } else
        {
            dynamic.gameObject.SetActive(false);
            level.gameObject.SetActive(false);
        }


    }
}
