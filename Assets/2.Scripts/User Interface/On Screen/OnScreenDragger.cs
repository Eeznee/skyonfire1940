using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.InputSystem.Layouts;



namespace UnityEngine.InputSystem.OnScreen
{
    [AddComponentMenu("Input/On-Screen Dragger")]
    public class OnScreenDragger : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public float coefficient = 1f;

        [InputControl(layout = "Vector2")]
        [SerializeField]
        private string m_ControlPath;

        private RectTransform parentRectTransform;

        private Vector3 startPos;
        private Vector2 previousPos;

        private void Start()
        {
            startPos = ((RectTransform)transform).anchoredPosition;
            parentRectTransform = transform.parent.GetComponentInParent<RectTransform>();
        }
        private bool dragged;
        private Vector2 pointerPosition;

        protected override void OnEnable()
        {
            base.OnEnable();
            dragged = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData == null)
                throw new System.ArgumentNullException(nameof(eventData));

            dragged = true;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, eventData.position, eventData.pressEventCamera, out pointerPosition);
            previousPos = pointerPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData == null)
                throw new System.ArgumentNullException(nameof(eventData));

            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, eventData.position, eventData.pressEventCamera, out pointerPosition);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            dragged = false;
            ((RectTransform)transform).anchoredPosition = startPos;
            SendValueToControl(Vector2.zero);
        }

        private void Update()
        {
            if (dragged)
            {
                SendValueToControl((pointerPosition - previousPos) * coefficient);
                previousPos = pointerPosition;
            }
        }

        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }
    }
}