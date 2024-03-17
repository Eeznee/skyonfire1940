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
        if (Player.aircraft && !Player.crew.ripped && !Player.crew.humanBody.Gloc())
        {
            Vector3 pos = SofCamera.cam.WorldToScreenPoint(Player.tr.position + SofCamera.directionInput * Player.aircraft.convergeance);
            transform.position = pos;
            level.transform.rotation = Quaternion.Euler(0f, 0f, -SofCamera.tr.eulerAngles.z);

            bool active = pos.z > 0f;
            active &= !TimeManager.paused;
            active &= GameManager.Controls() == ControlsMode.Tracking || Player.seatInterface == SeatInterface.Gunner;
            active &= !(Player.seatInterface == SeatInterface.Gunner && SofCamera.viewMode == 1);
            bool dyn = PlayerActions.dynamic || (Player.seatInterface == SeatInterface.Gunner);
            dynamic.gameObject.SetActive(dyn && active);
            level.gameObject.SetActive(!dyn && active);

            //Change alpha to not hide the gunsight
            Color c = dynamic.color;
            if (Player.aircraft && SofCamera.viewMode == 1)
            {
                float angle = Vector3.Angle(SofCamera.directionInput, Player.aircraft.transform.forward);
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
