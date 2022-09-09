using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PPSlider : MonoBehaviour
{
    public string parameterName = "PitchSensitivity";
    public Slider slider;
    public void Start()
    {
        slider.value = PlayerPrefs.GetFloat(parameterName, slider.value);
        SetLevel();
    }
    public void SetLevel()
    {
        PlayerPrefs.SetFloat(parameterName, slider.value);
    }
}
