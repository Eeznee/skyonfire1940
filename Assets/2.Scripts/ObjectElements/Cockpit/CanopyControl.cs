using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class CanopyControl : HydraulicsInteractable
{
    public BailOut bailOut;

    protected override void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {
        base.VRInteraction(gripPos, gripRot);
        
    }

    private void Update()
    {
        CockpitInteractableUpdate();
        Animate(hydraulics.stateInput);
        if (xrGrab && bailOut) bailOut.xrGrab.colliders[0].enabled = hydraulics.state > 0.7f;//bailOut.secondXrGrip.colliders[0].enabled = hydraulics.state > 0.7f;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CanopyControl))]
public class CanopyControlEditor : HydraulicsInteractableEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        CanopyControl canopy = (CanopyControl)target;
        canopy.bailOut = EditorGUILayout.ObjectField("Bail Out", canopy.bailOut, typeof(BailOut), true) as BailOut;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(canopy);
            EditorSceneManager.MarkSceneDirty(canopy.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
