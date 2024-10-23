using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchRotation : SofComponent
{
    public Transform[] transforms;


    private void Update()
    {
        foreach(Transform t in transforms)
        {
            if (t.root != tr.root) continue;
            t.localRotation = tr.localRotation;
        }
    }
}
