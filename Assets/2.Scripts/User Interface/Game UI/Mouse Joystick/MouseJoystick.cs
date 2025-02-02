using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Layouts;

using UnityEngine.UI;
namespace UnityEngine.InputSystem.OnScreen
{
    [RequireComponent(typeof(RectTransform))]
    public class MouseJoystick : OnScreenControl
    {
        private RectTransform tr;

        [InputControl(layout = "Vector2")] [SerializeField] private string m_ControlPath;
        private float sensitivity = 0.5f;
        public float size = 0.6f;
        public Image fixedDot;
        public Image border;
        public Image dot;
        public Transform dotTr;

        private Vector2 dotPos;
        private Vector2 output;
        private float borderAlpha;
        private float fixedDotAlpha;
        private bool alwaysShow;

        void Start()
        {
            sensitivity = PlayerPrefs.GetFloat("MouseStickSensitivity", sensitivity);
            alwaysShow = PlayerPrefs.GetInt("MouseStickAlwaysShow", 1) == 1;
            transform.position = dotTr.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            tr = GetComponent<RectTransform>();
            dotPos = Vector2.zero;
            fixedDotAlpha = fixedDot.color.a;
            borderAlpha = border.color.a;

            float min = (1f - size) / 2f;
            tr.anchorMin = new Vector2(tr.anchorMin.x, min);
            tr.anchorMax = new Vector2(tr.anchorMax.x, 1f - min);
            tr.SetTop(0f);
            tr.SetBottom(0f);

            PlayerActions.pilot.Pitch.performed += _ => OverwritePitch();
            PlayerActions.pilot.Roll.performed += _ => OverwriteRoll();

            PlayerActions.cam.Reset.performed += _ => SetDot(Vector2.zero);
        }
        private float MaxPos()
        {
            return Screen.height * size / 2f;
        }
        private void OverwritePitch()
        {
            if (!SofCamera.lookAround) return;
            SetDot(Vector2.zero);
        }
        private void OverwriteRoll()
        {
            if (!SofCamera.lookAround) return;
            SetDot(new Vector2(0f, dotPos.y));
        }
        int frameCount = 0;
        void Update()
        {
            //Starts at the tenth frame, to prevent unaligned start in editor
            if(frameCount < 10)
            {
                frameCount++;
                return;
            }


            if (!SofCamera.lookAround)
            {
                Vector2 input = Mouse.current.delta.value * sensitivity;
                SetDot(dotPos + input);
            }
            if (!alwaysShow)
            {
                Color c = fixedDot.color;
                c.a = fixedDotAlpha * Mathf.InverseLerp(0.3f, 0f, output.magnitude);
                fixedDot.color = c;

                c = border.color;
                c.a = borderAlpha * Mathf.InverseLerp(0.8f, 1f, output.magnitude);
                border.color = c;
            }
            bool hideAll = SofCamera.lookAround;
            fixedDot.enabled = border.enabled = dot.enabled = !hideAll;
        }
        private void SetDot(Vector2 pos)
        {
            dotPos = pos;
            dotPos.x = Mathf.Clamp(dotPos.x, -MaxPos(), MaxPos());
            dotPos.y = Mathf.Clamp(dotPos.y, -MaxPos(), MaxPos());

            dotTr.position = Features.ScreenSize() / 2f + dotPos;

            output = dotPos * 2f / (Screen.height * size);
            output *= 0.999f;
            SendValueToControl(output);
        }
        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }
    }
}