using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.UI;

namespace UnityEngine.InputSystem.OnScreen
{
    public class OnScreenConditionalTrigger : OnScreenControl, IPointerDownHandler, IPointerUpHandler
    {
        [InputControl(layout = "Button")]
        [SerializeField]
        private string m_ControlPath;

        public Toggle toggle;

        public void OnPointerUp(PointerEventData eventData)
        {
            SendValueToControl(0.0f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (toggle.isOn) SendValueToControl(1.0f);
        }


        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }


        void Start()
        {
            if (toggle.name == "Primary Toggle")
            {
                toggle.onValueChanged.AddListener(UpdatePrimaries);
                UpdatePrimaries(toggle.isOn);
            }

            if (toggle.name == "Secondary Toggle")
            {
                toggle.onValueChanged.AddListener(UpdateSecondaries);
                UpdateSecondaries(toggle.isOn);
            }


        }

        void UpdatePrimaries(bool a) { PrimariesEnabled = a; }
        void UpdateSecondaries(bool a) { SecondariesEnabled = a; }

        public static bool PrimariesEnabled;
        public static bool SecondariesEnabled;
    }
}