using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DynamicUI : MonoBehaviour
{
    private void Start()
    {
        ResetProperties();
    }

    public virtual bool IsActive()
    {
        return true;
    }

    public virtual void ResetProperties()
    {
        bool active = IsActive();
        if (active != gameObject.activeSelf) gameObject.SetActive(active);
    }


}
public static class DynamicUIExtension
{
    public static void ResetProperties(this DynamicUI[] dynamicUIs)
    {
        foreach(DynamicUI dynamicUI in dynamicUIs)
        {
            if (dynamicUI && dynamicUI.gameObject) dynamicUI.ResetProperties();
        }
    }
}
