using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.XR.Interaction.Toolkit;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class CockpitInteractable : SofComponent
{
    //[HideInInspector] public XRGrabInteractable xrGrab;
    public HandGrip grip;
    public Outline outline;
    public Collider[] colliders;
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
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        colliders = grip.GetComponentsInChildren<Collider>();
        //xrGrab = null;

        defaultPos = transform.localPosition;
        defaultRot = transform.localRotation;
        gripDefaultPos = grip.transform.localPosition;

        if (outline) outline.enabled = false;


        //if (GameManager.gm.vr) DisableVR();
        //else RemoveVR();
    }
    /*
    public virtual void EnableVR(XRGrabInteractable xrPrefab)
    {
        xrPrefab.colliders.Clear();
        xrPrefab.colliders.AddRange(colliders);
        xrGrab = Instantiate(xrPrefab);

        xrGrab.transform.parent = grip.transform.parent;
        xrGrab.transform.position = grip.transform.position;

        xrGrab.interactionLayers = LayerMask.GetMask(indexSelect ? "TriggerGrab" : "GripGrab");
        xrGrab.gameObject.layer = indexSelect ? 14 : 13;
        foreach (Collider c in colliders) { c.enabled = true; c.gameObject.layer = 0; }
        xrGrab.attachEaseInTime = 0f;

        Rigidbody rb = xrGrab.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;

        MeshRenderer meshRend = GetComponentInChildren<MeshRenderer>();
        if (meshRend)
        {
            if (!outline) outline = meshRend.gameObject.AddComponent<Outline>();
            outlineColor = indexSelect ? Color.blue : Color.red;
            outline.OutlineColor = outlineColor;
            outline.enabled = true;
            outline.OutlineWidth = 2f;
        }
    }
    public virtual void DisableVR()
    {
        foreach (Collider c in colliders) c.enabled = false;
        if (xrGrab) Destroy(xrGrab.gameObject);
        if (outline) outline.enabled = false;
    }
    public virtual void RemoveVR()
    {
        foreach (Collider c in colliders) Destroy(c);
        if (xrGrab) Destroy(xrGrab.gameObject);
        if (outline) Destroy(outline);
    }
    */
    protected virtual void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {

    }
    
    protected virtual void OnGrab()
    {
        //gripOffset = data.transform.InverseTransformVector(xrGrab.transform.position - grip.transform.parent.TransformPoint(gripDefaultPos));
    }
    protected virtual void OnRelease()
    {
    }
    
    protected virtual void CockpitInteractableUpdate()
    {
        /*
        if (!xrGrab) return;

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
        */
    }
    /*
    protected virtual bool ReadySelect()
    {
        if (xrGrab.isSelected) return true;
        SofVrRig s = SofVrRig.instance;
        bool hovered;
        hovered = s.rightHandTarget && s.rightHandTarget.Equals(xrGrab);
        hovered |= s.leftHandTarget && s.leftHandTarget.Equals(xrGrab);
        return hovered && xrGrab.isHovered;
    }
    */
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
