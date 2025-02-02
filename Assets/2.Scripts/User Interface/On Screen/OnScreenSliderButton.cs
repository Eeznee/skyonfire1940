using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.UI;
using UnityEngine.InputSystem.OnScreen;

namespace UnityEngine.InputSystem.OnScreen
{
    [RequireComponent(typeof(OnScreenUISlider))]
    public class OnScreenSliderButton : OnScreenControl
    {
        public enum Type
        {
            Min,
            Max
        }

        [InputControl(layout = "Button")] [SerializeField] private string m_ControlPath;

        public Type type;

        private OnScreenUISlider throttle;
        private void Start()
        {
            throttle = GetComponent<OnScreenUISlider>();
        }
        void LateUpdate()
        {
            bool auxiliaryValue = type == Type.Min ? throttle.minButtonActive : throttle.maxButtonActive;
            SendValueToControl(auxiliaryValue ? 1f : 0f);
        }

        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }
    }
}

