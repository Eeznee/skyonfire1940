using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public class OnScreenThrottle : OnScreenControl, IPointerUpHandler, IDragHandler
    {
        [InputControl(layout = "Axis")][SerializeField] private string m_ControlPath;

        [HideInInspector] public Slider slider;

        public float minButtonThreshold = 0.185f;
        public float maxButtonThreshold = 0.85f;
        [HideInInspector] public bool minButtonActive;
        [HideInInspector] public bool maxButtonActive;


        [NonSerialized] public bool dragged;

        public Action OnControlValueSent;

        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }
        public void OnDrag(PointerEventData eventData)
        {
            dragged = true;
            //Debug.Log(slider.value);
            UpdateSliderAndSendValue();
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            dragged = false;

            UpdateSliderAndSendValue();
        }
        private void UpdateSliderAndSendValue()
        {
            minButtonActive = slider.value < (minButtonThreshold + slider.minValue) * 0.5f;
            maxButtonActive = slider.value > (maxButtonThreshold + slider.maxValue) * 0.5f;
            if (!minButtonActive && !maxButtonActive)
            {
                slider.value = Mathf.Clamp(slider.value, minButtonThreshold, maxButtonThreshold);
                float valueToSend = Mathf.InverseLerp(minButtonThreshold, maxButtonThreshold, slider.value);
                SendValueToControl(valueToSend);
            }
            else
            {
                slider.value = maxButtonActive ? slider.maxValue : slider.minValue;
                SendValueToControl(slider.value);
            }


            OnControlValueSent?.Invoke();
        }
        private void Start()
        {
            dragged = false;
            slider = GetComponent<Slider>();
        }

        void Update()
        {
            if (dragged) return;
            if (!Player.aircraft) return;

            CompleteThrottle throttle = Player.aircraft.engines.Throttle;

            if (throttle.Boost)
            {
                slider.value = slider.maxValue;
                UpdateSliderAndSendValue();
            }
            else if (Player.aircraft.controls.brake > 0.5f)
            {
                slider.value = 0f;
            } 
            else
            {
                slider.value = Mathf.Lerp(minButtonThreshold, maxButtonThreshold, throttle);
            }

        }
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(OnScreenThrottle)), CanEditMultipleObjects]
public class OnScreenThrottleEditor : Editor
{

    SerializedProperty controlPath;


    SerializedProperty minButtonThreshold;
    SerializedProperty maxButtonThreshold;
    void OnEnable()
    {
        controlPath = serializedObject.FindProperty("m_ControlPath");

        minButtonThreshold = serializedObject.FindProperty("minButtonThreshold");
        maxButtonThreshold = serializedObject.FindProperty("maxButtonThreshold");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        OnScreenThrottle onScreen = (OnScreenThrottle)target;

        EditorGUILayout.PropertyField(controlPath);

        EditorGUILayout.PropertyField(minButtonThreshold);
        EditorGUILayout.PropertyField(maxButtonThreshold);


        serializedObject.ApplyModifiedProperties();
    }
}
#endif