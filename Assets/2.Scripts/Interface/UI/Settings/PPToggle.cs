using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Toggle))]
public class PPToggle : MonoBehaviour
{
    Toggle toggle;
    public string saveName;
    bool on;

    public void Start()
    {
        toggle = GetComponent<Toggle>();
        toggle.isOn = on = PlayerPrefs.GetInt(saveName, toggle.isOn ? 1 : 0) == 1;
    }
    private void Update()
    {
        if (toggle.isOn != on)
        {
            on = toggle.isOn;
            PlayerPrefs.SetInt(saveName, toggle.isOn ? 1 : 0);
        }
    }
}
