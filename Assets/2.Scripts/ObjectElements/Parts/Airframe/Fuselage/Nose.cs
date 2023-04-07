using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Nose : Fuselage
{
    public Mesh brokenModel;

    public override bool Detachable()
    {
        return false;
    }
    public override void Rip()
    {
        if (ripped) return;
        base.Rip();
        if (brokenModel && !Detachable()) GetComponent<MeshFilter>().mesh = brokenModel;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Nose))]
public class NoseEditor : FuselageEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //
        Nose nose = (Nose)target;
        nose.brokenModel = EditorGUILayout.ObjectField("Broken Model", nose.brokenModel, typeof(Mesh), false) as Mesh;

        base.OnInspectorGUI();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(nose);
            EditorSceneManager.MarkSceneDirty(nose.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
