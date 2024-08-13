using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(CustomWheel)), CanEditMultipleObjects]
public class CustomWheelEditor : ModuleEditor
{
    protected override string BasicName()
    {
        return "Module";
    }
    static bool showWheel = true;
    private SerializedProperty radius;
    private SerializedProperty frictionMultiplier;

    static bool showBrakes = true;
    private SerializedProperty brakes;
    private SerializedProperty maxBrakeTorque;
    protected override void OnEnable()
    {
        base.OnEnable();
        radius = serializedObject.FindProperty("radius");
        brakes = serializedObject.FindProperty("brakes");
        frictionMultiplier = serializedObject.FindProperty("frictionMultiplier");
        maxBrakeTorque = serializedObject.FindProperty("maxBrakeTorque");
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CustomWheel wheel = (CustomWheel)target;

        serializedObject.Update();

        if (wheel.GetComponentInParent<Suspension>() == null)
            EditorGUILayout.HelpBox("This wheel needs a suspension as its parent", MessageType.Warning);

        showWheel = EditorGUILayout.Foldout(showWheel, "Wheel", true, EditorStyles.foldoutHeader);
        if (showWheel)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(radius);
            EditorGUILayout.Slider(frictionMultiplier, 0f, 3f, new GUIContent("Side Friction Multiplier"));

            if (wheel.brakes != CustomWheel.BrakeSystem.None)
            {
                //EditorGUILayout.PropertyField(brakeForce, new GUIContent("Brake Force in N"));
            }

            EditorGUI.indentLevel--;
        }

        showBrakes = EditorGUILayout.Foldout(showBrakes, "Brakes", true, EditorStyles.foldoutHeader);
        if (showBrakes)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(brakes);
            if (wheel.brakes != CustomWheel.BrakeSystem.None)
            {
                EditorGUILayout.PropertyField(maxBrakeTorque, new GUIContent("Max Torque N.m"));
            }


            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    const int segments = 32;
    Color color = Color.red;
    protected void OnSceneGUI()
    {
        CustomWheel wheel = (CustomWheel)target;
        Transform tr = wheel.transform;

        Handles.color = color;

        Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, tr.rotation, Vector3.one);

        Vector3 startPoint = Vector3.forward * wheel.radius;
        startPoint = rotationMatrix.MultiplyPoint3x4(startPoint) + tr.position;

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * 2.0f * Mathf.PI / segments;

            Vector3 localEndPoint = new Vector3(0.0f, Mathf.Sin(angle), Mathf.Cos(angle)) * wheel.radius;

            Vector3 endPoint = rotationMatrix.MultiplyPoint3x4(localEndPoint) + tr.position;

            Handles.DrawLine(startPoint, endPoint);
            startPoint = endPoint;
        }
    }
}
#endif