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

#if MOBILE_INPUT
        if (!force && PlayerPrefs.GetInt("HideUI", 0) == 0) hidden = false;
#endif

        mask.enabled = hidden;
    }

    private void Start()
    {
        instance = this;

        if (!mask)
        {
            mask = GetComponent<Mask>();

            PlayerActions.menu.HideUI.performed += _ => Toggle(false);
        }
        mask.enabled = hidden;
    }
}
