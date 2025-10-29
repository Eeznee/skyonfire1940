using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.UI;


namespace UnityEngine.InputSystem.OnScreen
{
    /// <summary>
    /// A button that is visually represented on-screen and triggered by touch or other pointer
    /// input.
    /// </summary>
    [AddComponentMenu("Input/On-Screen Hold Center")]
    public class OnScreenHoldCenter : OnScreenControl, IPointerDownHandler, IPointerUpHandler
    {
        [InputControl(layout = "Button")]
        [SerializeField] private string m_ControlPath;
        private float downAt = -100f;
        private Vector2 downPos = Vector2.zero;
        private bool down;

        [SerializeField] private float holdTime = 1f;
        public void OnPointerUp(PointerEventData eventData)
        {
            down = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Vector2 center = new Vector2(Screen.width, Screen.height)*0.5f;
            if ((center - eventData.position).magnitude < Screen.height / 6f)
            {
                downAt = Time.unscaledTime;
                down = true;
                downPos = eventData.position;
            }
        }

        private void Update()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (down && (touch.position - downPos).magnitude > 50f) down = false;
            }
            bool time = Time.unscaledTime > downAt + holdTime;

            if (down && time) { down = false; SendValueToControl(1.0f); }
            else SendValueToControl(0f);
        }

        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }
    }
}