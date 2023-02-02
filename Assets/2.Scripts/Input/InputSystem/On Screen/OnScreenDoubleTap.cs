using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.UI;
////TODO: custom icon for OnScreenButton component

namespace UnityEngine.InputSystem.OnScreen
{
    /// <summary>
    /// A button that is visually represented on-screen and triggered by touch or other pointer
    /// input.
    /// </summary>
    [AddComponentMenu("Input/On-Screen Double Tap")]
    public class OnScreenDoubleTap : OnScreenControl, IPointerDownHandler, IPointerUpHandler
    {
        [InputControl(layout = "Button")]
        [SerializeField]
        private string m_ControlPath;
        private float lastTap = 0f;

        const float minTapInterval = 0.27f;
        public void OnPointerUp(PointerEventData eventData)
        {
            SendValueToControl(0.0f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (lastTap > 0f) SendValueToControl(1.0f);
            else lastTap = minTapInterval;
        }
        private void Update()
        {
            lastTap =  Mathf.Max(0f,lastTap - Time.unscaledDeltaTime);
        }

        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }
    }
}