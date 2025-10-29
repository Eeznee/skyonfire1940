using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class FuelValve : AnalogInteractable
{
    public Engine engine;

    protected override void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {
        base.VRInteraction(gripPos, gripRot);

        bool on = input > 0.5f;
        if (engine.OnInput != on)
            engine.SetOnInput(on);
    }
    private void Update()
    {
        CockpitInteractableUpdate();
        Animate(engine ? engine.OnInput : input > 0.5f);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(FuelValve))]
public class FuelValveEditor : AnalogInteractableEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        FuelValve valve = (FuelValve)target;
        GUI.color = Color.red;
        EditorGUILayout.HelpBox("Starter Configuration", MessageType.None);
        GUI.color = GUI.backgroundColor;
        valve.engine = EditorGUILayout.ObjectField("Piston Engine", valve.engine, typeof(Engine), true) as Engine;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(valve);
            EditorSceneManager.MarkSceneDirty(valve.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif