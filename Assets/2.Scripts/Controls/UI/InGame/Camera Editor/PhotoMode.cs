using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
public class PhotoMode : MonoBehaviour
{
    public InputField tilt;

    private float speed;
    public float minSpeed = 1f;
    public float maxSpeed = 100f;
    private Vector3 speeds = Vector3.zero;
    private Vector3 worldPos = Vector3.zero;


    private void OnEnable()
    {
        worldPos = GameManager.gm.mapTr.InverseTransformPoint(SofCamera.tr.position);
    }

    private SubCam currentCam = null;

    public void ResetPositions()
    {
        worldPos = GameManager.gm.mapTr.InverseTransformPoint(Player.sofObj.transform.position);
    }
}
