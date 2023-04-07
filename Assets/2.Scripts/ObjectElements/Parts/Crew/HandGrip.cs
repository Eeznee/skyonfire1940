using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class HandGrip : MonoBehaviour
{
    public Vector3 rightPosOffset;
    public Vector3 rightEulerOffset;
    public Vector3 leftPosOffset;
    public Vector3 leftEulerOffset;
    public Vector3 rightHintPos;
    public Vector3 leftHintPos;

    public bool fixedRotation = false;
    [HideInInspector]public float grip;
    [HideInInspector] public float trigger;
    [HideInInspector] public float thumbDown;
    [HideInInspector] public float thumbIn;

    public void SetGrip(float gr)
    {
        grip = gr;
    }
    public void SetTrigger(float tr)
    {
        trigger = tr;
    }
    public void SetThumbDown(float tDown)
    {
        thumbDown = tDown;
    }
    public void SetThumbIn(float tIn)
    {
        thumbIn = tIn;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(HandGrip))]
public class HandGripEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        HandGrip grip = (HandGrip)target;
        GUI.color = GUI.backgroundColor;

        grip.grip = EditorGUILayout.Slider("Grip", grip.grip, 0f, 1f);
        grip.trigger = EditorGUILayout.Slider("Trigger", grip.trigger, 0f, 1f);
        grip.thumbDown = EditorGUILayout.Slider("Thumb Down", grip.thumbDown, 0f, 1f);
        grip.thumbIn = EditorGUILayout.Slider("Thumb In",grip.thumbIn, 0f, 1f);

        GUILayout.Space(30f);
        if (GUILayout.Button("Apply Symmetry From Right"))
        {
            grip.leftPosOffset = grip.rightPosOffset;
            grip.leftPosOffset.x *= -1f;
            grip.leftEulerOffset = -grip.rightEulerOffset;
            grip.leftEulerOffset.x *= -1f;
        }
        if (GUILayout.Button("Apply Symmetry From Left"))
        {
            grip.rightPosOffset = grip.leftPosOffset;
            grip.rightPosOffset.x *= -1f;
            grip.rightEulerOffset = -grip.leftEulerOffset;
            grip.rightEulerOffset.x *= -1f;
        }


        if (GUI.changed)
        {
            EditorUtility.SetDirty(grip);
            EditorSceneManager.MarkSceneDirty(grip.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
