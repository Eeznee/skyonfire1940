using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(SofModular))]
public class SofModularEditor : SofObjectEditor
{
    protected override void OnEnable()
    {
        base.OnEnable();

        SofModular complex = (SofModular)target;
        complex.SetReferences();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        SofModular complex = (SofModular)target;

        serializedObject.ApplyModifiedProperties();
    }
}
#endif