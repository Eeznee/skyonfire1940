using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollidersManager : ObjectElement
{
    private List<Collider> colliders = new List<Collider>();
    private List<Collider> triggerColliders = new List<Collider>();
    private bool toggled;

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            foreach (Collider col in transform.root.GetComponentsInChildren<Collider>())
            {
                if (col.gameObject.layer == 9 && !col.GetComponent<WheelCollider>())
                {
                    if (col.isTrigger) colliders.Add(col);
                    else triggerColliders.Add(col);
                }
            }

            ToggleColliders(false);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        ToggleColliders(true);
    }
    private void OnTriggerExit(Collider other)
    {
        ToggleColliders(false);
    }
    public void ToggleColliders(bool on)
    {
        //LODGroup lodGroup;
        if (toggled == on) return;
        toggled = on;
        foreach(Collider col in colliders)
            col.enabled = on;
    }
}
