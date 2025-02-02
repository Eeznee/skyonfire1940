using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class HydraulicsInteractable : AnalogInteractable
{
    public enum HydraulicsInteractableType
    {
        State,
        Direction,
        Dual
    }
    public HydraulicSystem hydraulics;
    public HydraulicsInteractableType type = HydraulicsInteractableType.State;
    public AnalogInteractable opposite;

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        if (type == HydraulicsInteractableType.Dual)
        {
            opposite.switchInput = switchInput;
            opposite.animationTime = animationTime;
        }
        if (type == HydraulicsInteractableType.Direction)
            switchInput = false;
    }
    protected override void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {
        base.VRInteraction(gripPos, gripRot);
        if (type == HydraulicsInteractableType.Dual) opposite.input = Mathf.Clamp(opposite.input, 0f, 1f - input);
        if (type == HydraulicsInteractableType.Direction) input = 0.5f * Mathf.Round(input * 2f);
        SendToHydraulics();
    }

    private void SendToHydraulics()
    {
        switch (type)
        {
            case HydraulicsInteractableType.State:
                hydraulics.Set(input);
                break;
            case HydraulicsInteractableType.Direction:
                int intInput = input < 0.5f ? -1 : (input == 0.5f ? 0 : 1);
                hydraulics.SetDirection(intInput);
                break;
            case HydraulicsInteractableType.Dual:
                if (input > 0.5f && opposite.input < input) hydraulics.Set(1f);
                if (opposite.input > 0.5f && opposite.input > input) hydraulics.Set(0f);
                if (opposite.input < 0.5f && input < 0.5f) hydraulics.SetDirection(0);
                break;
        }
    }

    protected override void CockpitInteractableUpdate()
    {
        base.CockpitInteractableUpdate();

    }

    protected override void Animate(float animInput)
    {
        //Twin
        if (type == HydraulicsInteractableType.Dual)
        {
            if (opposite.xrGrab && opposite.xrGrab.isSelected)
            {
                input = Mathf.Clamp(input, 0f, 1f - opposite.input);
                SendToHydraulics();
            }
            base.Animate(hydraulics.state == hydraulics.stateInput ? 0f : hydraulics.stateInput);
        }
        else
            base.Animate(hydraulics.stateInput);
    }

    private void Update()
    {
        CockpitInteractableUpdate();
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
        inter.type = (HydraulicsInteractable.HydraulicsInteractableType)EditorGUILayout.EnumPopup("Type", inter.type);
        if (inter.type == HydraulicsInteractable.HydraulicsInteractableType.Dual)
            inter.opposite = EditorGUILayout.ObjectField("Opposite Hydraulic", inter.opposite, typeof(AnalogInteractable), true) as AnalogInteractable;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(inter);
            EditorSceneManager.MarkSceneDirty(inter.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif