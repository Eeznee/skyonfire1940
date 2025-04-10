using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Cover : AnalogInteractable
{
    public Collider covered;
    protected override void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {
        base.VRInteraction(gripPos, gripRot);
        covered.enabled = input > 0.5f;
    }
    private void Update()
    {
        CockpitInteractableUpdate();
        //if (xrGrab) Animate(covered.enabled ? 1f : 0f);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Cover))]
public class CoverEditor : AnalogInteractableEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        Cover cover = (Cover)target;
        GUI.color = Color.red;
        EditorGUILayout.HelpBox("Starter Configuration", MessageType.None);
        GUI.color = GUI.backgroundColor;
        cover.covered = EditorGUILayout.ObjectField("Covered Collider", cover.covered, typeof(Collider), true) as Collider;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(cover);
            EditorSceneManager.MarkSceneDirty(cover.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif