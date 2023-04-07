using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ocean : MonoBehaviour
{
    public Material simple;
    public Material complex;
    void Awake()
    {
        Material mat = QualitySettings.GetQualityLevel() == 3 ? complex : simple;
        GetComponent<Renderer>().sharedMaterial = mat;
    }
}
