using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class LinkSpar : BoundedAirframe
{
    public SofAirframe[] linkedAirframes;
    public float damageRatio = 0.55f;

    public override void Rip()
    {
        base.Rip();
        foreach(SofAirframe airf in linkedAirframes)
        {
            airf.Damage(damageRatio);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LinkSpar))]
public class LinkSparEditor : AirframeEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        //
        LinkSpar frame = (LinkSpar)target;

        frame.damageRatio = EditorGUILayout.FloatField("Damage Ratio", frame.damageRatio);

        SerializedProperty linkedAirframes = serializedObject.FindProperty("linkedAirframes");
        EditorGUILayout.PropertyField(linkedAirframes, true);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(frame);
            EditorSceneManager.MarkSceneDirty(frame.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
