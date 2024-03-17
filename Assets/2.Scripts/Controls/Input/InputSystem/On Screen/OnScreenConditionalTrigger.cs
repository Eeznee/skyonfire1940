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
    }
}