using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class AnalogInteractable : CockpitInteractable
{
    public enum Mode
    {
        Push,
        Lever,
        Knob
    }
    public float animationTime = 0f;
    public bool switchInput = false;
    public bool clicking = false;
    public Mode mode = Mode.Lever;

    public Vector3 axis = new Vector3(1f, 0f, 0f);
    public float angleOn = 20f;
    public Vector3 pushOn = new Vector3(0f, 0f, 0.01f);

    public float input;

    protected float animSpeed;
    protected Vector3 defaultUp;

    //Group Grip
    public bool group = false;
    public AnalogInteractable linked;
    public HandGrip groupGrip;
    private Vector3 gripPosition;
    private Quaternion gripRotation;

    protected bool GroupSelect()
    {
        if (!xrGrab || !group) return false;
        bool groupSelect = SofVrRig.instance.rightHandTarget == this && SofVrRig.instance.rightHand.enabled;
        groupSelect |= SofVrRig.instance.leftHandTarget == this && SofVrRig.instance.leftHand.enabled;
        return groupSelect;
    }
    public override HandGrip CurrentGrip()
    {
        if (GroupSelect()) return groupGrip;
        return base.CurrentGrip();
    }
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        animSpeed = (mode == Mode.Push ? pushOn.magnitude : Mathf.Abs(angleOn)) / Mathf.Max(0.001f, animationTime);
        defaultUp = transform.parent.InverseTransformDirection(grip.transform.position - transform.position);
    }
    public override void EnableVR(XRGrabInteractable xrPrefab)
    {
        base.EnableVR(xrPrefab);
        if (indexSelect && group)
        {
            xrGrab.interactionLayers = -1;//LayerMask.GetMask("Default");
            xrGrab.gameObject.layer = 0;
        }
    }
    protected override void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {
        gripPosition = gripPos;
        gripRotation = gripRot;
        base.VRInteraction(gripPos, gripRot);
        switch (mode)
        {
            case Mode.Push:
                input = Mathf.Clamp01(Mathv.InverseLerpVec3(defaultPos, defaultPos + pushOn, transform.parent.InverseTransformPoint(gripPos)));
                break;
            case Mode.Lever:
                input = Mathf.Clamp01(Mathv.OptimalLeverRotation(transform, gripPos, axis, transform.parent.TransformDirection(defaultUp)) / angleOn);
                break;
            case Mode.Knob:
                break;
        }
        if (switchInput && clicking) input = input > 0.5f ? 1f : 0f;
    }
    protected override void CockpitInteractableUpdate()
    {
        base.CockpitInteractableUpdate();
        if (group && GroupSelect())
        {
            linked.VRInteraction(gripPosition, gripRotation);
        }
    }
    protected virtual void Animate(bool animInput)
    {
        Animate(animInput ? 1f : 0f);
    }
    protected virtual void Animate(float animInput)
    {
        switch (mode)
        {
            case Mode.Push:
                Vector3 position = defaultPos + pushOn * animInput;
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, position, Time.deltaTime * animSpeed);
                break;
            case Mode.Lever:
                Quaternion rotation = defaultRot * Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(axis * angleOn), animInput);
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, rotation, Time.deltaTime * animSpeed);
                break;
            case Mode.Knob:
                break;
        }
    }
    protected override void OnRelease()
    {
        base.OnRelease();
        if (switchInput) input = input > 0.5f ? 1f : 0f;
    }
    private void Update()
    {
        CockpitInteractableUpdate();
        Animate(input);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AnalogInteractable))]
public class AnalogInteractableEditor : CockpitInteractableEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        AnalogInteractable analog = (AnalogInteractable)target;

        if (analog.indexSelect)
        {
            analog.group = EditorGUILayout.Toggle("Analog Group", analog.group);
            if (analog.group)
            {
                analog.linked = EditorGUILayout.ObjectField("Linked Analog", analog.linked, typeof(AnalogInteractable), true) as AnalogInteractable;
                analog.groupGrip = EditorGUILayout.ObjectField("Group Grip", analog.groupGrip, typeof(HandGrip), true) as HandGrip;
            }
        }

        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Analog Configuration", MessageType.None);
        GUI.color = GUI.backgroundColor;

        analog.switchInput = EditorGUILayout.Toggle("Switch (binary)", analog.switchInput);
        if (analog.switchInput) analog.clicking = EditorGUILayout.Toggle("Clicking", analog.clicking);
        analog.mode = (AnalogInteractable.Mode)EditorGUILayout.EnumPopup("Mode", analog.mode);
        switch (analog.mode)
        {
            case AnalogInteractable.Mode.Push:
                analog.pushOn = EditorGUILayout.Vector3Field("On State Offset", analog.pushOn); break;
            case AnalogInteractable.Mode.Lever:
                analog.axis = EditorGUILayout.Vector3Field("Axis", analog.axis);
                analog.angleOn = EditorGUILayout.FloatField("On State Angle", analog.angleOn);
                break;
            case AnalogInteractable.Mode.Knob:
                break;
        }
        analog.animationTime = EditorGUILayout.FloatField("Animation Time", analog.animationTime);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(analog);
            EditorSceneManager.MarkSceneDirty(analog.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif