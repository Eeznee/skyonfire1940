using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.UI;
using UnityEngine.InputSystem.OnScreen;
using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace UnityEngine.InputSystem.OnScreen
{
    [RequireComponent(typeof(Slider))]
    public class OnScreenUISlider : OnScreenControl, IPointerUpHandler, IDragHandler
    {
        [InputControl(layout = "Axis")][SerializeField] private string m_ControlPath;

        [HideInInspector] public Slider slider;
        [HideInInspector] public OnScreenMultiSlider multiSlider;

        public bool reset = true;
        public float resetValue = 0f;

        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (multiSlider)
            {
                multiSlider.OnDrag(this, slider.value);
                return;
            }

            SendValueToControl(slider.value);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (multiSlider)
            {
                multiSlider.OnPointerUp(this);
                return;
            }

            if (reset)
                slider.value = resetValue;

            SendValueToControl(slider.value);
        }

        private void Start()
        {
            slider = GetComponent<Slider>();
        }
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(OnScreenUISlider)), CanEditMultipleObjects]
public class OnScreenUISliderEditor : Editor
{

    SerializedProperty controlPath;

    SerializedProperty reset;
    SerializedProperty resetValue;

    void OnEnable()
    {
        controlPath = serializedObject.FindProperty("m_ControlPath");

        reset = serializedObject.FindProperty("reset");
        resetValue = serializedObject.FindProperty("resetValue");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        OnScreenUISlider onScreen = (OnScreenUISlider)target;

        EditorGUILayout.PropertyField(controlPath);

        EditorGUILayout.PropertyField(reset);
        if (onScreen.reset) EditorGUILayout.PropertyField(resetValue);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif