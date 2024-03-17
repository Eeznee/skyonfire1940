using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.UI;
using UnityEngine.InputSystem.OnScreen;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace UnityEngine.InputSystem.OnScreen
{
    public class OnScreenMultiSlider : OnScreenControl
    {
        [InputControl(layout = "Axis")] [SerializeField] private string m_ControlPath;

        public OnScreenUISlider[] sliders;
        private float[] sliderValues;

        public bool reset;
        public float resetValue;

        private void Awake()
        {
            foreach (OnScreenUISlider slider in sliders) slider.multiSlider = this;
            sliderValues = new float[sliders.Length];
            for (int i = 0; i < sliderValues.Length; i++) sliderValues[i] = float.NaN;
        }
        private void ApplyValueToUntouchedSliders(float value)
        {
            for (int i = 0; i < sliderValues.Length; i++)
                if (float.IsNaN(sliderValues[i]))
                    sliders[i].slider.value = value;
        }
        public void OnDrag(OnScreenUISlider draggedSlider, float value)
        {
            for (int i = 0; i < sliders.Length; i++)
                if (sliders[i] == draggedSlider) sliderValues[i] = value;
        }
        public void OnPointerUp(OnScreenUISlider draggedSlider)
        {
            for (int i = 0; i < sliders.Length; i++)
                if (sliders[i] == draggedSlider) sliderValues[i] = float.NaN;
        }

        private void LateUpdate()
        {
            float total = 0f;
            int receivedValues = 0;
            for (int i = 0; i < sliderValues.Length; i++)
            {
                if (float.IsNaN(sliderValues[i])) continue;
                receivedValues++;
                total += sliderValues[i];
            }
            if (receivedValues == 0)
            {
                if (reset)
                {
                    SendValueToControl(resetValue);
                    ApplyValueToUntouchedSliders(resetValue);
                }
                return;
            }

            float output = total / receivedValues;
            SendValueToControl(output);
            ApplyValueToUntouchedSliders(output);
        }
        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }

    }
}