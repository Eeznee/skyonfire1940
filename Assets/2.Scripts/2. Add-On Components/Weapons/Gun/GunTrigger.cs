using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Gun))]
public class GunTrigger : MonoBehaviour
{
    private bool fullAuto;

    private Gun gun;


    private void Awake()
    {
        gun = GetComponent<Gun>();
        fullAuto = GetComponent<Gun>().gunPreset.fullAuto;
        
    }


}
