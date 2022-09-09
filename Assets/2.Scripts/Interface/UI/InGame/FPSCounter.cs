using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityStandardAssets.Utility
{
    [RequireComponent(typeof (Text))]
    public class FPSCounter : MonoBehaviour
    {
        const float fpsMeasurePeriod = 1f;
        private int fpsCounter = 0;
        private float fpsNextPeriod = 0;
        private int currentFPS;
        const string display = "{0} FPS";
        private Text m_Text;


        private void Start()
        {
            fpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
            m_Text = GetComponent<Text>();
        }


        private void Update()
        {
            // measure average frames per second
            fpsCounter++;
            if (Time.realtimeSinceStartup > fpsNextPeriod)
            {
                currentFPS = (int) (fpsCounter / fpsMeasurePeriod);
                fpsCounter = 0;
                fpsNextPeriod += fpsMeasurePeriod;
                m_Text.text = string.Format(display, currentFPS);
            }
        }
    }
}
