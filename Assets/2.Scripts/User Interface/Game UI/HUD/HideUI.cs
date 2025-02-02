using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
public class HideUI : MonoBehaviour
{
    private bool hidden = false;
    Mask mask;
    // Start is called before the first frame update

    private void Toggle()
    {
        if (!gameObject.activeInHierarchy) return;
        hidden = !hidden;
#if MOBILE_INPUT
        if (PlayerPrefs.GetInt("HideUI", 0) == 0) hidden = false;
#endif
        mask.enabled = hidden;
    }

    private void Start()
    {
        if (!mask)
        {
            mask = GetComponent<Mask>();
            PlayerActions.menu.HideUI.performed += _ => Toggle();
        }
        mask.enabled = hidden;
    }
}
