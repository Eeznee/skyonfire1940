using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Airframe/Link Spar")]
public class LinkSpar : BarFrame
{
    public SofAirframe[] linkedAirframes;
    public float damageRatio = 0.55f;

    public override void Rip()
    {
        base.Rip();
        foreach(SofAirframe airf in linkedAirframes)
        {
            airf.DirectStructuralDamage(damageRatio);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LinkSpar)), CanEditMultipleObjects]
public class LinkSparEditor : BarFrameEditor
{
    SerializedProperty linkedAirframes;
    SerializedProperty damageRatio;

    protected override void OnEnable()
    {
        base.OnEnable();

        linkedAirframes = serializedObject.FindProperty("linkedAirframes");
        damageRatio = serializedObject.FindProperty("damageRatio");
    }

    static bool showLink = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        showLink = EditorGUILayout.Foldout(showLink, new GUIContent("Link Spar"), true, EditorStyles.foldoutHeader);
        if (showLink)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(linkedAirframes);
            EditorGUILayout.PropertyField(damageRatio);

            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
