using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


[RequireComponent(typeof(Slider))]
public class AudioSlider : MonoBehaviour
{
    public AudioMixer mixer;
    public string parameterName = "MusicVolume";

    Slider slider;
    public float multiplier = 1f;

    public void Start()
    {
        slider = GetComponent<Slider>();
        slider.value = PlayerPrefs.GetFloat(parameterName, 0.75f) / multiplier;
        SetLevel();
    }
    public void SetLevel()
    {
        mixer.SetFloat(parameterName, Mathf.Log10(slider.value * multiplier) * 20f);
        PlayerPrefs.SetFloat(parameterName, slider.value);
    }
}
