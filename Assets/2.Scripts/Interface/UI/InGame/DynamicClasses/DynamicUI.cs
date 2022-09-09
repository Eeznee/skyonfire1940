using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DynamicUI : MonoBehaviour
{
    public virtual bool IsActive()
    {
        return true;
    }

    public virtual void ResetProperties()
    {
        bool active = IsActive();
        gameObject.SetActive(active);
    }
}
