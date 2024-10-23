using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class BailOut : CockpitInteractable
{
    public Transform bailOutPoint;
    protected override void VRInteraction(Vector3 gripPos,Quaternion gripRot)
    {
        SofVrRig s = SofVrRig.instance;
        Vector3 delta = Vector3.Lerp(s.rightHandDelta, s.leftHandDelta, 0.5f);
        s.transform.position -= delta;
    }
    protected override void OnRelease()
    {
        base.OnRelease();
        if (Player.crew.transform.root != Player.aircraft.transform) return;
        Vector3 camLocal = bailOutPoint.InverseTransformPoint(Camera.main.transform.position);
        if (camLocal.y > 0f) Player.crew.bailOut.BailInstant();
        else SofVrRig.instance.ResetView();
    }

    private void LateUpdate()
    {
        CockpitInteractableUpdate();
        if (xrGrab && Player.seat.canopy)
        {
            xrGrab.enabled = Player.crew.Seat.canopy.state > 0.7f;
            if (!xrGrab.enabled) outline.OutlineColor = new Color(0f, 0f, 0f, 0f);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BailOut))]
public class BailOutInteractable : CockpitInteractableEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        BailOut bailOut = (BailOut)target;
        bailOut.bailOutPoint = EditorGUILayout.ObjectField("Bail Out Point", bailOut.bailOutPoint, typeof(Transform), true) as Transform;


        if (GUI.changed)
        {
            EditorUtility.SetDirty(bailOut);
            EditorSceneManager.MarkSceneDirty(bailOut.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif