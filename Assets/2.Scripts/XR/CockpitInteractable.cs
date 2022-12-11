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
    public Outline outline;
    private Color outlineColor;

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
            xrGrab = null;
            defaultPos = transform.localPosition;
            defaultRot = transform.localRotation;
            gripDefaultPos = grip.transform.localPosition;

            MeshRenderer meshRend = GetComponentInChildren<MeshRenderer>();
            if (meshRend && !outline)
                outline = meshRend.gameObject.AddComponent<Outline>();
            if (outline)
            {
                outlineColor = indexSelect ? Color.blue : Color.red;
                outline.OutlineColor = outlineColor;
                outline.enabled = true;
                outline.OutlineWidth = 2f;
                outline.enabled = false;
            }
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

        outline.enabled = true;
    }
    public virtual void DisableVR()
    {
        Destroy(xrGrab.gameObject);
        outline.enabled = false;
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

            //Outline
            if (outline && outline.enabled)
            {
                if (xrGrab.interactorsHovering.Count > 0) outline.OutlineColor = Color.white;
                else
                {
                    float rightDis = (SofVrRig.instance.rightHand.transform.position - xrGrab.transform.position).sqrMagnitude;
                    float leftDis = (SofVrRig.instance.leftHand.transform.position - xrGrab.transform.position).sqrMagnitude;
                    float minDis = Mathf.Min(rightDis, leftDis);
                    outline.OutlineColor = outlineColor - new Color(0f, 0f, 0f, Mathf.Pow(minDis, 0.1f));
                }
            }
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
        inter.outline = EditorGUILayout.ObjectField("Outline (optional)", inter.outline, typeof(Outline), true) as Outline;
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
