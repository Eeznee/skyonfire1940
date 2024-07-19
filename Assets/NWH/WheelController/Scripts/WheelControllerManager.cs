using System;
using NWH.Common.Vehicles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace NWH.WheelController3D
{
    [DefaultExecutionOrder(60)]
    public class WheelControllerManager : MonoBehaviour
    {
        [NonSerialized] public float combinedLoad;

        private List<WheelUAPI> _wheels = new List<WheelUAPI>();
        private int _wheelCount;


        private void Awake()
        {
            _wheels = new List<WheelUAPI>();
            _wheelCount = 0;
        }


        private void FixedUpdate()
        {
            UpdateCombinedLoad();
        }


        private void UpdateCombinedLoad()
        {
            combinedLoad = 0f;
            for (int i = 0; i < _wheelCount; i++)
            {
                WheelUAPI wheel = _wheels[i];
                combinedLoad += wheel.Load;
            }
        }


        public void Register(WheelUAPI wheel)
        {
            if (!_wheels.Contains(wheel))
            {
                _wheels.Add(wheel);
                _wheelCount++;
            }
        }


        public void Deregister(WheelUAPI wheel)
        {
            if (_wheels.Contains(wheel))
            {
                _wheels.Remove(wheel);
                _wheelCount--;
            }
        }



    }
}
