using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class HydraulicsInteractable : AnalogInteractable
{
    public HydraulicSystem hydraulics;
    public AnalogInteractable opposite;

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (opposite)
        {
            opposite.switchInput = switchInput;
            opposite.animationTime = animationTime;
        }
    }
    protected override void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {
        base.VRInteraction(gripPos, gripRot);
        if (opposite) opposite.input = Mathf.Clamp(opposite.input, 0f, 1f - input);
        SendToHydraulics();
    }

    private void SendToHydraulics()
    {
        if (!opposite)
        {
            hydraulics.Set(input);
            return;
        }
        if (input > 0.5f && opposite.input < input) hydraulics.Set(1f);
        if (opposite.input > 0.5f && opposite.input > input) hydraulics.Set(0f);
        if (opposite.input < 0.5f && input < 0.5f) hydraulics.Stop();
    }

    private void Update()
    {
        CockpitInteractableUpdate();
        //Twin
        if (opposite)
        {
            if (opposite.xrGrab && opposite.xrGrab.isSelected)
            {
                input = Mathf.Clamp(input, 0f, 1f - opposite.input);
                SendToHydraulics();
            }
            Animate(hydraulics.state == hydraulics.stateInput ? 0f : hydraulics.stateInput);
        }
        else 
            Animate(hydraulics.stateInput);
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
        inter.opposite = EditorGUILayout.ObjectField("Opposite Input (Optional)", inter.opposite, typeof(AnalogInteractable), true) as AnalogInteractable;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(inter);
            EditorSceneManager.MarkSceneDirty(inter.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif