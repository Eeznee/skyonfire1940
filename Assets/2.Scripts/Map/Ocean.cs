using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ocean : MonoBehaviour
{
    public Material simple;
    public Material complex;
    public int index = 0;
    void Awake()
    {
        Material[] mats = GetComponent<Renderer>().sharedMaterials;
        mats[index] = QualitySettings.GetQualityLevel() == 3 ? complex : simple;
        GetComponent<Renderer>().sharedMaterials = mats;
    }
}
