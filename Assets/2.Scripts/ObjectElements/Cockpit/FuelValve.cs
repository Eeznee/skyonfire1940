using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class FuelValve : AnalogInteractable
{
    public PistonEngine engine;

    protected override void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {
        base.VRInteraction(gripPos, gripRot);
        engine.onInput = input > 0.5f;
    }
    private void Update()
    {
        CockpitInteractableUpdate();
        Animate(engine ? engine.onInput : input > 0.5f);
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
        valve.engine = EditorGUILayout.ObjectField("Piston Engine", valve.engine, typeof(PistonEngine), true) as PistonEngine;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(valve);
            EditorSceneManager.MarkSceneDirty(valve.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif