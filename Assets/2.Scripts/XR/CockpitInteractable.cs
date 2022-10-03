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
    [HideInInspector]public XRGrabInteractable xrGrab;
    public HandGrip grip;

    public bool indexSelect = false;

    protected bool wasSelected = false;
    protected Vector3 gripOffset = Vector3.zero;
    protected Vector3 gripDefaultPos;
    protected Vector3 defaultPos;
    protected Quaternion defaultRot;

    public virtual HandGrip CurrentGrip()
    {
        return grip;
    }
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);

        if (firstTime)
        {
            //Default Positions
            defaultPos = transform.localPosition;
            defaultRot = transform.localRotation;
            gripDefaultPos = grip.transform.localPosition;
        }
    }
    public virtual void EnableVR(XRGrabInteractable xrPrefab)
    {
        xrPrefab.colliders.Clear();
        xrPrefab.colliders.AddRange(grip.GetComponentsInChildren<Collider>());
        xrGrab = Instantiate(xrPrefab);

        xrGrab.transform.parent = grip.transform.parent;
        xrGrab.transform.position = grip.transform.position;

        xrGrab.interactionLayers = LayerMask.GetMask(indexSelect ? "TriggerGrab" : "GripGrab");
        xrGrab.gameObject.layer = indexSelect ? 14 : 13;
        foreach (Collider col in xrGrab.colliders)
            col.gameObject.layer = 0;
        xrGrab.attachEaseInTime = 0f;

        Rigidbody rb = xrGrab.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
    }
    public virtual void DisableVR()
    {
        Destroy(xrGrab.gameObject);
    }
    protected virtual void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {

    }
    protected virtual void OnGrab()
    {
        gripOffset = data.transform.InverseTransformVector(xrGrab.transform.position - grip.transform.parent.TransformPoint(gripDefaultPos));
    }
    protected virtual void OnRelease()
    {
    }
    protected virtual void CockpitInteractableUpdate()
    {
        if (xrGrab)
        {
            if (xrGrab.isSelected)
            {
                if (!wasSelected) OnGrab();
                VRInteraction(xrGrab.transform.position - data.transform.TransformVector(gripOffset), xrGrab.transform.rotation);
            }
            else
            {
                xrGrab.transform.SetPositionAndRotation(grip.transform.position, transform.rotation);
                if (wasSelected) OnRelease();
            }
            wasSelected = xrGrab.isSelected;
        }
    }
    protected virtual bool ReadySelect()
    {
        if (xrGrab.isSelected) return true;
        SofVrRig s = SofVrRig.instance;
        bool hovered;
        hovered = s.rightHandTarget && s.rightHandTarget.Equals(xrGrab);
        hovered |= s.leftHandTarget && s.leftHandTarget.Equals(xrGrab);
        return hovered && xrGrab.isHovered;
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
        inter.grip = EditorGUILayout.ObjectField("Hand Grip", inter.grip, typeof(HandGrip), true) as HandGrip;
        GUILayout.Space(15f);
        inter.indexSelect = EditorGUILayout.Toggle("Index Select", inter.indexSelect);
        if (GUI.changed)
        {
            EditorUtility.SetDirty(inter);
            EditorSceneManager.MarkSceneDirty(inter.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
