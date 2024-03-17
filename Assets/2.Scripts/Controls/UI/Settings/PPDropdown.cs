using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Dropdown))]
public class PPDropdown : MonoBehaviour
{
    Dropdown dropdown;
    public string saveName;
    int val;

    public void Start()
    {
        dropdown = GetComponent<Dropdown>();
        dropdown.value = val = PlayerPrefs.GetInt(saveName, dropdown.value);
    }
    private void Update()
    {
        if (dropdown.value != val)
        {
            val = dropdown.value;
            PlayerPrefs.SetInt(saveName, val);
        }
    }
}
