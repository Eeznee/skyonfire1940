
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;


namespace UnityEngine.InputSystem.OnScreen
{
    /// <summary>
    /// A button that is visually represented on-screen and triggered by touch or other pointer
    /// input.
    /// </summary>
    [AddComponentMenu("Input/On-Screen Button")]
    public class OnScreenGunsButton : OnScreenControl, IPointerDownHandler, IPointerUpHandler
    {
        public void OnPointerUp(PointerEventData eventData)
        {
            SendValueToControl(0.0f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            SendValueToControl(1.0f);
        }

        /*
        /// <summary>
        /// If true, the button's value is driven from the pressure value of touch or pen input.
        /// </summary>
        /// <remarks>
        /// This essentially allows having trigger-like buttons as on-screen controls.
        /// </remarks>
        [SerializeField] private bool m_UsePressure;
        */

        [InputControl(layout = "Button")]
        [SerializeField]
        private string m_ControlPath;

        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }
    }
}

