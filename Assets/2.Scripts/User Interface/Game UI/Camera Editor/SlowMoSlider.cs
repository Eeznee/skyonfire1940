using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SlowMoSlider : MonoBehaviour
{
    private Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
    }

    public void SetSlowMoBasedOnSlider(float value)
    {
        TimeManager.SetSlowMo(value);
    }

    void Update()
    {
        slider.value = TimeManager.GetSlowMoFactor();
    }
}
