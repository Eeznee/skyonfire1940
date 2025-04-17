using System;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.InputSystem.OnScreen
{
    public class TiltInput : OnScreenControl
    {
        [InputControl(layout = "Vector2")][SerializeField] private string m_ControlPath;

        private float tiltSensitivity = 1f;

        private float fullPitchAngle;
        private float fullRollAngle;
        const float pitchTiltAngle = 30f;
        const float rollTiltAngle = 30f;
        public float pitchZeroing = 0;

        public Vector3[] rawInputs;
        const int averageAmount = 6;

        private float lastPitch = 0f;

        private void AddInput()
        {
            for (int i = rawInputs.Length - 1; i > 0; i--)
            {
                rawInputs[i] = rawInputs[i - 1];
            }
            rawInputs[0] = AccelerometerInput;
        }
        public Vector3 AccelerometerInput
        {
            get
            {
                if (GravitySensor.current != null) return GravitySensor.current.gravity.ReadValue();
                if (Accelerometer.current != null) return Accelerometer.current.acceleration.ReadValue();

                return Vector3.down;
            }
        }
        private Vector3 AverageInput()
        {
            Vector3 sum = Vector3.zero;
            foreach (Vector3 vec in rawInputs) sum += vec;
            return sum / averageAmount;
        }
        private void Start()
        {
            if(GravitySensor.current != null) InputSystem.EnableDevice(GravitySensor.current);
            if(GravitySensor.current == null && Accelerometer.current != null) InputSystem.EnableDevice(Accelerometer.current);

            rawInputs = new Vector3[averageAmount];

            tiltSensitivity = PlayerPrefs.GetFloat("TiltSensitivity", 1f);
            fullPitchAngle = pitchTiltAngle / tiltSensitivity;
            fullRollAngle = rollTiltAngle / tiltSensitivity;
        }

        private void Update()
        {
            AddInput();
            Vector2 tilt = GetTilt(1f - Mathf.Abs(lastPitch));
            tilt.y = Mathf.Clamp(tilt.y - pitchZeroing, -fullPitchAngle, fullPitchAngle) / fullPitchAngle;
            tilt.x = Mathf.Clamp(tilt.x, -fullRollAngle, fullRollAngle) / fullRollAngle;
            SendValueToControl(tilt);
            lastPitch = tilt.y;
        }

        public void Recalibrate()
        {
            pitchZeroing = GetTilt(0f).y;
        }

        private Vector2 GetTilt(float smoothing)
        {
            Vector2 tilt = Vector2.zero;
            Vector3 input = Vector3.Lerp(AccelerometerInput, AverageInput(), smoothing);
            if (input != Vector3.zero)
            {
                tilt.x = Mathf.Atan2(input.x, new Vector2(input.z, input.y).magnitude) * Mathf.Rad2Deg;
                tilt.y = Mathf.Atan2(input.z, input.y) * Mathf.Rad2Deg;
                if (pitchZeroing < -90f && tilt.y > 0f) tilt.y -= 360f;
                if (pitchZeroing > 90f && tilt.y < 0f) tilt.y += 360f;
            }
            return tilt;
        }

        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }
    }
}
