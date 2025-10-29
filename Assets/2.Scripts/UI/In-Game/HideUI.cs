using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HideUI : MonoBehaviour
{
    private bool hidden = false;
    private Mask mask;

    public static HideUI instance;



    public void Toggle(bool force)
    {
        Toggle(!hidden, force);
    }
    public void Toggle(bool uiHidden, bool force)
    {
        if (!gameObject.activeInHierarchy) return;

        hidden = uiHidden;

        if (Extensions.IsMobile && !force && !SofSettingsSO.CurrentSettings.hudHiding) hidden = false;

        mask.enabled = hidden;
    }

    private void Start()
    {
        instance = this;

        if (!mask)
        {
            mask = GetComponent<Mask>();

            ControlsManager.menu.HideUI.performed += _ => Toggle(false);
        }
        mask.enabled = hidden;
    }
}
