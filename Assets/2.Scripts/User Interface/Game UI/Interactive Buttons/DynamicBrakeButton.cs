using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicBrakeButton : MonoBehaviour
{
    public GameObject brakeButton;
    public GameObject buttonToReplace;

    private bool ShowBrakeButton {
        get
        {
            if (Player.aircraft == null) return false;
            if (Player.aircraft.data == null) return false;

            return Player.aircraft.data.grounded.Get || Player.aircraft.TimeSinceLastLanding < 10f;
        }

    }
    private bool brakeButtonIsOn;

    private void Start()
    {
        Refresh();
    }

    void Update()
    {
        if (!brakeButton) return;

        if(brakeButtonIsOn != ShowBrakeButton)
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        brakeButton.SetActive(ShowBrakeButton);
        if (buttonToReplace) buttonToReplace.SetActive(!ShowBrakeButton);

        brakeButtonIsOn = ShowBrakeButton;
    }
}
