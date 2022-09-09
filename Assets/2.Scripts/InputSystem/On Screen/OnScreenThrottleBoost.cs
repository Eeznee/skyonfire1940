using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.UI;

namespace UnityEngine.InputSystem.OnScreen
{
    [RequireComponent(typeof(OnScreenThrottle))]
    public class OnScreenThrottleBoost : OnScreenControl
    {
        [InputControl(layout = "Button")] [SerializeField] private string m_ControlPath;

        private OnScreenThrottle throttle;
        private void Start()
        {
            throttle = GetComponent<OnScreenThrottle>();
        }
        void LateUpdate()
        {
            SendValueToControl(throttle.boosting ? 1f : 0f);
        }

        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }
    }
}

