using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CopyMainCam : MonoBehaviour
{
    Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }
    void Update()
    {
        cam.fieldOfView = Camera.main.fieldOfView;

    }
}
