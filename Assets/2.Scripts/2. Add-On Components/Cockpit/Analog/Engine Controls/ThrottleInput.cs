using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class ThrottleInput : AnalogInteractable
{
    public Engine engine;
    protected override void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {
        base.VRInteraction(gripPos, gripRot);
        engine.Throttle = new CompleteThrottle(input, engine);
    }
    private void Update()
    {
        CockpitInteractableUpdate();
        Animate(engine ? engine.Throttle : input);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(ThrottleInput))]
public class ThrottleEditor : AnalogInteractableEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        ThrottleInput throttle = (ThrottleInput)target;
        GUI.color = Color.red;
        EditorGUILayout.HelpBox("Starter Configuration", MessageType.None);
        GUI.color = GUI.backgroundColor;
        throttle.engine = EditorGUILayout.ObjectField("Piston Engine", throttle.engine, typeof(Engine), true) as Engine;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(throttle);
            EditorSceneManager.MarkSceneDirty(throttle.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif