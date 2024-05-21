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
        worldPos = GameManager.gm.mapmap.transform.InverseTransformPoint(SofCamera.tr.position);
    }

    public void ResetPositions()
    {
        worldPos = GameManager.gm.mapmap.transform.InverseTransformPoint(Player.sofObj.transform.position);
    }
}
