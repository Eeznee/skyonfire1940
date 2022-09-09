using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class HydraulicsInteractable : AnalogInteractable
{
    public HydraulicSystem hydraulics;
    public AnalogInteractable linked;

    protected override void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {
        base.VRInteraction(gripPos, gripRot);
        linked.input = Mathf.Clamp(linked.input, 0f, 1f - input);
        SendToHydraulics();
    }

    private void SendToHydraulics()
    {
        if (input > 0.5f && linked.input < input) hydraulics.Set(1f);
        if (linked.input > 0.5f && linked.input > input) hydraulics.Set(0f);
        if (linked.input < 0.5f && input < 0.5f) hydraulics.Stop();
    }

    private void Update()
    {
        CockpitInteractableUpdate();
        if (linked.xrGrip.isSelected)
        {
            input = Mathf.Clamp(input, 0f, 1f - linked.input);
            SendToHydraulics();
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(HydraulicsInteractable))]
public class HydraulicsInteractableEditor : AnalogInteractableEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        HydraulicsInteractable inter = (HydraulicsInteractable)target;
        GUI.color = Color.red;
        EditorGUILayout.HelpBox("Hydraulics Configuration", MessageType.None);
        GUI.color = GUI.backgroundColor;
        inter.hydraulics = EditorGUILayout.ObjectField("Hydraulics", inter.hydraulics, typeof(HydraulicSystem), true) as HydraulicSystem;
        inter.linked = EditorGUILayout.ObjectField("Linked Analog", inter.linked, typeof(AnalogInteractable), true) as AnalogInteractable;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(inter);
            EditorSceneManager.MarkSceneDirty(inter.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif