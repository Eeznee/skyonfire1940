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

    public bool fixedRotation = false;
    public float grip;
    public float trigger;
    public float thumbDown;
    public float thumbIn;

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
    SerializedProperty rightPosOffset;
    SerializedProperty rightEulerOffset;
    SerializedProperty leftPosOffset;
    SerializedProperty leftEulerOffset;

    SerializedProperty fixedRotation;
    SerializedProperty grip;
    SerializedProperty trigger;
    SerializedProperty thumbDown;
    SerializedProperty thumbIn;
    protected void OnEnable()
    {
        rightPosOffset = serializedObject.FindProperty("rightPosOffset");
        rightEulerOffset = serializedObject.FindProperty("rightEulerOffset");
        leftPosOffset = serializedObject.FindProperty("leftPosOffset");
        leftEulerOffset = serializedObject.FindProperty("leftEulerOffset");

        fixedRotation = serializedObject.FindProperty("fixedRotation");

        grip = serializedObject.FindProperty("grip");
        trigger = serializedObject.FindProperty("trigger");
        thumbDown = serializedObject.FindProperty("thumbDown");
        thumbIn = serializedObject.FindProperty("thumbIn");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();


        HandGrip handGrip = (HandGrip)target;

        EditorGUILayout.PropertyField(rightPosOffset);
        EditorGUILayout.PropertyField(rightEulerOffset);
        if (GUILayout.Button("Apply Symmetry From Right To Left Grip"))
        {
            Undo.RecordObject(handGrip, "Apply Symmetry From Left To Right Grip");

            handGrip.leftPosOffset = handGrip.rightPosOffset;
            handGrip.leftPosOffset.x *= -1f;
            handGrip.leftEulerOffset = -handGrip.rightEulerOffset;
            handGrip.leftEulerOffset.x *= -1f;

            EditorUtility.SetDirty(handGrip);
        }

        GUILayout.Space(30f);

        EditorGUILayout.PropertyField(leftPosOffset);
        EditorGUILayout.PropertyField(leftEulerOffset);
        if (GUILayout.Button("Apply Symmetry From Left To Right Grip"))
        {
            Undo.RecordObject(handGrip, "Apply Symmetry From Left To Right Grip");

            handGrip.rightPosOffset = handGrip.leftPosOffset;
            handGrip.rightPosOffset.x *= -1f;
            handGrip.rightEulerOffset = -handGrip.leftEulerOffset;
            handGrip.rightEulerOffset.x *= -1f;

            EditorUtility.SetDirty(handGrip);
        }

        GUILayout.Space(30f);

        EditorGUILayout.PropertyField(fixedRotation);

        GUILayout.Space(30f);

        EditorGUILayout.Slider(grip, 0f, 1f);
        EditorGUILayout.Slider(trigger, 0f, 1f);
        EditorGUILayout.Slider(thumbDown, 0f, 1f);
        EditorGUILayout.Slider(thumbIn, 0f, 1f);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
