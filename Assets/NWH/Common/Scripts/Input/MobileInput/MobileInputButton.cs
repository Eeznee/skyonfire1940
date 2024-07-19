using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NWH.Common.Input
{
    /// <summary>
    ///     Adds clicked and pressed flags to the standard Unity UI Button.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public class MobileInputButton : Button
    {
        public bool hasBeenClicked;
        public bool isPressed;

        private void Update()
        {
            isPressed = IsPressed();
            hasBeenClicked = false;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            hasBeenClicked = true;
        }
    }
}