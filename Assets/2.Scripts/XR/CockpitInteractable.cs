using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class CockpitInteractable : ObjectElement
{
    public XRGrabInteractable xrGrip;
    public HandGrip grip;
    public Outline outline;

    public bool indexSelect = false;
    public bool relative = true;

    protected private bool selectedPrevious;
    protected private Vector3 gripOffset = Vector3.zero;
    protected private Vector3 xrGripDefaultPos;
    protected private Vector3 defaultPos;
    protected private Quaternion defaultRot;
    protected Transform anchor;

    Color indexOutline = new Color(0f, 0.585f,1f);
    Color handOutline = new Color(1f, 0.19f,0.19f);
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        defaultPos = transform.localPosition;
        defaultRot = transform.localRotation;
        if (!xrGrip) return;
        selectedPrevious = false;
        xrGripDefaultPos = xrGrip.transform.localPosition;
        if (xrGrip.colliders[0]) xrGrip.colliders[0].gameObject.layer = xrGrip.gameObject.layer =  indexSelect ? 14 : 13;
        xrGrip.interactionLayerMask = LayerMask.GetMask(indexSelect ? "TriggerGrab" : "GripGrab");
        xrGrip.attachEaseInTime = 0f;
        anchor = xrGrip.transform.parent;
        if (outline)
        {
            outline.OutlineColor = indexSelect ? indexOutline : handOutline;
            outline.enabled = false;
        }
    }
    protected virtual void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {

    }
    protected virtual void Animate()
    {

    }
    protected virtual void OnGrab()
    {
        if (relative) gripOffset = data.transform.InverseTransformVector(xrGrip.transform.position - anchor.TransformPoint(xrGripDefaultPos));
    }
    protected virtual void OnRelease()
    {
        xrGrip.transform.localPosition = xrGripDefaultPos;
    }
    protected virtual void CockpitInteractableUpdate()
    {
        if (!xrGrip) return;
        if (xrGrip.isSelected)
        {
            if (!selectedPrevious) OnGrab();
            VRInteraction(xrGrip.transform.position - data.transform.TransformVector(gripOffset), xrGrip.transform.rotation);
        }
        else
        {
            if (selectedPrevious) OnRelease();
        }
        if (GameManager.gm.vr && outline) outline.enabled = ReadySelect();
        selectedPrevious = xrGrip.isSelected;
        if (Time.timeScale > 0f) Animate();
    }
    protected virtual bool ReadySelect()
    {
        if (xrGrip.isSelected) return true;
        SofVrRig s = SofVrRig.instance;
        bool hovered;
        if (indexSelect)
        {
            hovered = s.rightIndexTarget && s.rightIndexTarget.Equals(xrGrip);
            hovered = hovered || (s.leftIndexTarget && s.leftIndexTarget.Equals(xrGrip));
        } else
        {
            hovered = s.rightHandTarget && s.rightHandTarget.Equals(xrGrip);
            hovered = hovered || (s.leftHandTarget && s.leftHandTarget.Equals(xrGrip));
        }
        return hovered && xrGrip.isHovered;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(CockpitInteractable))]
public class CockpitInteractableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CockpitInteractable inter = (CockpitInteractable)target;

        GUI.color = GUI.backgroundColor;
        inter.xrGrip = EditorGUILayout.ObjectField("XR Grab Interactable", inter.xrGrip, typeof(XRGrabInteractable), true) as XRGrabInteractable;
        inter.grip = EditorGUILayout.ObjectField("Hand Grip", inter.grip, typeof(HandGrip), true) as HandGrip;
        inter.outline = EditorGUILayout.ObjectField("Outline", inter.outline, typeof(Outline), true) as Outline;
        GUILayout.Space(15f);
        inter.indexSelect = EditorGUILayout.Toggle("Index Select", inter.indexSelect);
        inter.relative = EditorGUILayout.Toggle("Relative Grab", inter.relative);
        if (GUI.changed)
        {
            EditorUtility.SetDirty(inter);
            EditorSceneManager.MarkSceneDirty(inter.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
