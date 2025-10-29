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

        private float timeCounter = 0f;


        private void Start()
        {
            fpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
            m_Text = GetComponent<Text>();
        }


        private void Update()
        {
            // measure average frames per second
            fpsCounter++;
            timeCounter += Time.unscaledDeltaTime;
            if (timeCounter > fpsMeasurePeriod)
            {
                currentFPS = (int) (fpsCounter / timeCounter);
                fpsCounter = 1;
                timeCounter = 0f;
                m_Text.text = string.Format(display, currentFPS);
            }
        }
    }
}
