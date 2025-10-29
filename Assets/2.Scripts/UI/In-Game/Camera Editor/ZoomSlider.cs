using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class ZoomSlider : MonoBehaviour
{
    private Slider slider;
    void Start()
    {
        slider = GetComponent<Slider>();
    }
    public void SetZoomBasedOnSlider(float value)
    {
        CameraInputs.SetFov(value);
    }

    void Update()
    {
        slider.value = CameraInputs.GetZoomFactor();
    }
}
