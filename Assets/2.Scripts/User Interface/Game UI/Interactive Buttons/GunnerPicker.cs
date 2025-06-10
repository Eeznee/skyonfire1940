using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class GunnerPicker : DynamicUI
{
    public RectTransform[] gunnerButtons;

    public override void ResetProperties()
    {
        base.ResetProperties();

        if (!Player.sofObj) return;

        RectTransform rectTransform = GetComponent<RectTransform>();
        int gunners = Player.modular.crew.Length - 1;
        for (int i = 0; i < gunnerButtons.Length; i++)
        {
            Vector2 anchored;
            anchored.y = -rectTransform.sizeDelta.y * 1.1f;
            anchored.x = rectTransform.sizeDelta.x * 1.1f * (i + 0.5f - gunners / 2f);
            gunnerButtons[i].anchoredPosition = anchored;
            gunnerButtons[i].gameObject.SetActive(i < gunners);
        }
    }
    void Start()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public void DisplayTrigger()
    {
        transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeSelf);
    }
}
