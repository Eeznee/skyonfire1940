using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class CamSpeedSlider : MonoBehaviour
{
    private Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
    }

    public void SetCamSpeedBasedOnSlider(float value)
    {
        CameraInputs.SetCamSpeed(value);
    }

    void Update()
    {
        slider.value = CameraInputs.GetCamSpeedFactor();
    }
}

