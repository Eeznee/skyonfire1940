using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Starter : ButtonInteractable
{
    public PistonEngine engine;

    private void Update()
    {
        CockpitInteractableUpdate();
        if (activated) engine.TryIgnite();
        Animate(input);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Starter))]
public class StarterEditor : ButtonInteractableEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        Starter starter = (Starter)target;
        GUI.color = Color.red;
        EditorGUILayout.HelpBox("Starter Configuration", MessageType.None);
        GUI.color = GUI.backgroundColor;
        starter.engine = EditorGUILayout.ObjectField("Piston Engine", starter.engine, typeof(PistonEngine), true) as PistonEngine;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(starter);
            EditorSceneManager.MarkSceneDirty(starter.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif