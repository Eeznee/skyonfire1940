using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.UI;


namespace UnityEngine.InputSystem.OnScreen
{
    public class OnScreenSlider : OnScreenControl,IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public float m_MovementRange = 50;
        public bool vertical = true;
        public bool reset = true;
        public float startValue = 0f;

        [InputControl(layout = "Axis")]
        [SerializeField]
        private string m_ControlPath;

        private Vector3 m_StartPos;
        

        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }
        public void SendInput(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera, out var position);
            Vector2 delta = position - (Vector2)m_StartPos;
            float deltaAxis = vertical ? delta.y : delta.x;
            deltaAxis = Mathf.Clamp(deltaAxis, -movementRange, movementRange);
            ((RectTransform)transform).anchoredPosition = m_StartPos + deltaAxis * (vertical ? Vector3.up : Vector3.right);
            SendValueToControl(deltaAxis / movementRange);
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (eventData == null)
                throw new System.ArgumentNullException(nameof(eventData));

            SendInput(eventData);
        }
        public void OnPointerDown(PointerEventData eventData)
        {
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (reset)
            {
                ((RectTransform)transform).anchoredPosition = m_StartPos;
                SendValueToControl(0f);
            } else
                SendInput(eventData);
        }

        private void Start()
        {
            m_StartPos = ((RectTransform)transform).anchoredPosition;
            ((RectTransform)transform).anchoredPosition = m_StartPos + startValue * movementRange * (vertical ? Vector3.up : Vector3.right);
        }

        public float movementRange
        {
            get => m_MovementRange;
            set => m_MovementRange = value;
        }
    }
}


/*
         [InputControl(layout = "Axis")] [SerializeField] private string m_ControlPath;

        private Slider slider;

        private void Start()
        {
            slider = GetComponent<Slider>();
        }
        void Update()
        {
            SendValueToControl(slider.value);
            //if (slider.value == 0f)
            SentDefaultValueToControl();
        }

        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }
*/