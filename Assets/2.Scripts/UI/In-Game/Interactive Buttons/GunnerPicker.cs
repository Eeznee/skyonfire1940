using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunnerPicker : DynamicUI
{
    public Button singleGunnerSelect;

    public Button multiGunnerFoldout;
    public GameObject foldOutParent;
    public RectTransform[] gunnerButtons;


    public override void ResetProperties()
    {
        base.ResetProperties();

        if (!Player.sofObj) return;


        RectTransform rectTransform = GetComponent<RectTransform>();
        int gunners = Player.modular.crew.Length - 1;

        multiGunnerFoldout.gameObject.SetActive(gunners > 1);
        singleGunnerSelect.gameObject.SetActive(gunners == 1);

        if (gunners > 1)
        {
            for (int i = 0; i < gunnerButtons.Length; i++)
            {
                Vector2 anchored;
                anchored.y = -rectTransform.sizeDelta.y * 1.1f;
                anchored.x = rectTransform.sizeDelta.x * 1.1f * (i + 0.5f - gunners / 2f);
                gunnerButtons[i].anchoredPosition = anchored;
                gunnerButtons[i].gameObject.SetActive(i < gunners);
            }
        }
    }


    private void Awake()
    {
        foldOutParent.SetActive(false);
        multiGunnerFoldout.onClick.RemoveAllListeners();
        multiGunnerFoldout.onClick.AddListener(ToggleFoldout);

        ResetProperties();
    }

    private void ToggleFoldout()
    {
        foldOutParent.SetActive(!foldOutParent.activeSelf);
    }
}
