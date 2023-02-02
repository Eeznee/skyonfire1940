using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.UI;

namespace UnityEngine.InputSystem.OnScreen
{
    [RequireComponent(typeof(Scrollbar))]
    public class OnScreenThrottle : OnScreenControl
    {
        [InputControl(layout = "Axis")] [SerializeField] private string m_ControlPath;

        public float noBoostMax = 0.85f;
        [HideInInspector] public bool boosting;

        private Scrollbar scrollbar;

        private void Start()
        {
            scrollbar = GetComponent<Scrollbar>();
        }
        void LateUpdate()
        {
            boosting = scrollbar.value == 1f;
            if (!boosting)
            {
                scrollbar.value = Mathf.Clamp(scrollbar.value, 0f, noBoostMax);
            }
            SendValueToControl(Mathf.Clamp01(scrollbar.value/noBoostMax));
        }

        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }
    }
}

