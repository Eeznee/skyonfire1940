using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.InputSystem.Layouts;

////TODO: custom icon for OnScreenStick component

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

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData == null)
                throw new System.ArgumentNullException(nameof(eventData));

            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, eventData.position, eventData.pressEventCamera, out previousPos);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData == null)
                throw new System.ArgumentNullException(nameof(eventData));

            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, eventData.position, eventData.pressEventCamera, out Vector2 pos);
            SendValueToControl((pos - previousPos) * coefficient);
            previousPos = pos;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ((RectTransform)transform).anchoredPosition = startPos;
            SendValueToControl(Vector2.zero);
        }

        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }
    }
}