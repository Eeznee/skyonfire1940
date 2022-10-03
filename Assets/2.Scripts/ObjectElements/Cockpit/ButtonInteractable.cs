using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class ButtonInteractable : AnalogInteractable
{
    public bool spring = true;
    protected bool activated = false;
    protected bool stopped = false;
    private float previousInput = 0f;

    protected override void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {
        base.VRInteraction(gripPos, gripRot);
        activated = input > 0.7f && previousInput < 0.7f;
        stopped = input < 0.3f && previousInput > 0.3f;
        previousInput = input;
    }
    protected override void Animate(bool animInput)
    {
        if (!spring) base.Animate(animInput);
        else base.Animate(xrGrab && xrGrab.isSelected ? animInput : false);
    }
    protected override void Animate(float animInput)
    {
        if (!spring) base.Animate(animInput);
        else base.Animate(xrGrab && xrGrab.isSelected ? animInput : 0f);
    }
    private void Update()
    {
        CockpitInteractableUpdate();
        Animate(input);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(ButtonInteractable))]
public class ButtonInteractableEditor : AnalogInteractableEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        ButtonInteractable button = (ButtonInteractable)target;
        button.spring = EditorGUILayout.Toggle("Spring", button.spring);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(button);
            EditorSceneManager.MarkSceneDirty(button.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
