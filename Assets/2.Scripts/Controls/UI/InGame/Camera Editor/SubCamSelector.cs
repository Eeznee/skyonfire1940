using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
public class SubCamSelector : MonoBehaviour
{
    public Button[] camButtons;
    public Text[] camText;

    public Color baseButtonColor;
    public Color selectedColor;

    private void OnEnable()
    {
        SofCamera.OnSwitchCamEvent += UpdateButtons;
        CameraEditor.OnSubcamSettingsChange += UpdateButtons;
        UpdateButtons();
    }
    private void OnDisable()
    {
        SofCamera.OnSwitchCamEvent -= UpdateButtons;
        CameraEditor.OnSubcamSettingsChange -= UpdateButtons;
    }

    private void UpdateButtons()
    {
        foreach (Button button in camButtons) button.targetGraphic.color = baseButtonColor;
        if (SofCamera.viewMode < 0) camButtons[-SofCamera.viewMode - 1].targetGraphic.color = selectedColor;

        for (int i = 0; i < camText.Length; i++) camText[i].text = ((CustomCamLogic)PlayerPrefs.GetInt("camBehaviour" + (i + 1), 0)).ToString();
    }
}
