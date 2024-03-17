using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class Pedal : MonoBehaviour
    {
        public bool offset;
        public float maxOffset = 0.1f;
        public bool rotation;
        public float maxRotation = 10f;
        public Vector3 axis = Vector3.right;

        Vector3 originalPos;
        Quaternion originalRot;

        SofAircraft controller;

        private void Start()
        {
            controller = GetComponentInParent<SofAircraft>();
            originalPos = transform.localPosition;
            originalRot = transform.localRotation;
        }

        private void Update()
        {
            if (offset)
            {
                transform.localPosition = originalPos;
                transform.localPosition += Vector3.forward * maxOffset * controller.controlValue.y;
            }
            if (rotation)
            {
                transform.localRotation = originalRot;
                transform.Rotate(axis, controller.controlValue.y * maxRotation);
            }
        }
    }

