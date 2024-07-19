using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NWH.Common.Demo
{
    public class DemoOscillator : MonoBehaviour
    {
        public Vector3 travel;
        public float speed = 1f;

        private Vector3 initPos;
        private float time;

        private Rigidbody _rb;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            initPos = transform.position;
        }

        void FixedUpdate()
        {
            time += Time.fixedDeltaTime * speed;
            _rb.MovePosition(initPos + travel * Mathf.Sin(time));
        }
    }
}

