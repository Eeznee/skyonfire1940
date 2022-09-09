using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinterSwapper : MonoBehaviour
{
    public Material winter;
    public int materialIndex = 0;

    private void Start()
    {
        if (GameManager.weather.winter)
        {
            Material[] mats = GetComponent<MeshRenderer>().materials;
            mats[materialIndex] = winter;
            GetComponent<MeshRenderer>().materials = mats;
        }
    }
}
