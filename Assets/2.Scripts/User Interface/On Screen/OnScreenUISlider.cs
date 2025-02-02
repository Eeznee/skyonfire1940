using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.UI;
using UnityEngine.InputSystem.OnScreen;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace UnityEngine.InputSystem.OnScreen
{
    [RequireComponent(typeof(Slider))]
    public class OnScreenUISlider : OnScreenControl, IPointerUpHandler, IDragHandler
    {
        [InputControl(layout = "Axis")] [SerializeField] private string m_ControlPath;

        [HideInInspector] public Slider slider;
        [HideInInspector] public OnScreenMultiSlider multiSlider;

        public bool reset = true;
        public float resetValue = 0f;

        public bool extremitiesButtons = false;
        public float minButtonThreshold = 0185f;
        public float maxButtonThreshold = 0.85f;
        [HideInInspector] public bool minButtonActive;
        [HideInInspector] public bool maxButtonActive;

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

            SendCorrectedValue();
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (multiSlider)
            {
                multiSlider.OnPointerUp(this);
                return;
            }

            if (reset)
                SendCorrectedValue(resetValue);
            else
                SendCorrectedValue();
        }

        private void SendCorrectedValue() { SendCorrectedValue(slider.value); }
        private void SendCorrectedValue(float value)
        {
            if (extremitiesButtons)
            {
                minButtonActive = slider.value == slider.minValue;
                maxButtonActive = slider.value == slider.maxValue;
                if (!minButtonActive && !maxButtonActive) slider.value = Mathf.Clamp(slider.value, minButtonThreshold, maxButtonThreshold);
                value = Mathf.InverseLerp(minButtonThreshold, maxButtonThreshold, slider.value);
                value = Mathf.Clamp01(value);
            }
            else slider.value = value;

            SendValueToControl(value);
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

    SerializedProperty extremitiesButtons;
    SerializedProperty minButtonThreshold;
    SerializedProperty maxButtonThreshold;
    void OnEnable()
    {
        controlPath = serializedObject.FindProperty("m_ControlPath");

        reset = serializedObject.FindProperty("reset");
        resetValue = serializedObject.FindProperty("resetValue");

        extremitiesButtons = serializedObject.FindProperty("extremitiesButtons");
        minButtonThreshold = serializedObject.FindProperty("minButtonThreshold");
        maxButtonThreshold = serializedObject.FindProperty("maxButtonThreshold");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        OnScreenUISlider onScreen = (OnScreenUISlider)target;

        EditorGUILayout.PropertyField(controlPath);

        EditorGUILayout.PropertyField(reset);
        if (onScreen.reset) EditorGUILayout.PropertyField(resetValue);

        EditorGUILayout.PropertyField(extremitiesButtons);
        if (onScreen.extremitiesButtons)
        {
            EditorGUILayout.PropertyField(minButtonThreshold);
            EditorGUILayout.PropertyField(maxButtonThreshold);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif