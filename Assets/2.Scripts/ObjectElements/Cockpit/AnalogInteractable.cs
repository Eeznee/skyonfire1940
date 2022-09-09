using UnityEngine;
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
    public Mode mode = Mode.Lever;
    public float knobAngle = 180f;
    public Vector3 angleOn = new Vector3(20f, 0f, 0f);
    public Vector3 pushOn = new Vector3(0f, 0f, 0.01f);

    //
    public float input;
    protected bool activated = false;
    protected bool stopped = false;
    protected bool isActive = false;

    protected float animSpeed;
    protected Vector3 defaultUp;
    const float threshholdOn = 0.3f;
    const float thresholdOff = 0.7f;

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        animSpeed = (mode == Mode.Push ? pushOn : angleOn).magnitude / Mathf.Max(0.001f, animationTime);
        defaultUp = transform.parent.InverseTransformDirection(transform.up);
    }
    protected override void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {
        base.VRInteraction(gripPos, gripRot);
        switch (mode)
        {
            case Mode.Push:
                input = Mathf.Clamp01(Mathv.InverseLerpVec3(defaultPos, defaultPos + pushOn, transform.parent.InverseTransformPoint(gripPos)));
                transform.localPosition = defaultPos + pushOn * input;
                break;
            case Mode.Lever:
                input = Mathf.Clamp01(Mathv.OptimalLeverRotation(transform, gripPos, angleOn, transform.parent.TransformDirection(defaultUp)) / angleOn.magnitude);
                transform.localRotation = defaultRot * Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(angleOn), input);
                break;
            case Mode.Knob:
                break;
        }
        bool active = input > (isActive ? threshholdOn : thresholdOff);
        activated = !isActive && active;
        stopped = isActive && !active;
        isActive = active;
        if (switchInput) input = Mathf.Round(input);
    }
    protected override void Animate()
    {
        if (xrGrip.isSelected) return;
        switch (mode)
        {
            case Mode.Push:
                Vector3 position = defaultPos + pushOn * input;
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, position, Time.deltaTime * animSpeed);
                break;
            case Mode.Lever:
                Quaternion rotation = defaultRot * Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(angleOn),input);
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, rotation, Time.deltaTime * animSpeed);
                break;
            case Mode.Knob:
                break;
        }
    }
    private void Update()
    {
        CockpitInteractableUpdate();
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
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Analog Configuration", MessageType.None);
        GUI.color = GUI.backgroundColor;
        analog.animationTime = EditorGUILayout.FloatField("Animation Time", analog.animationTime);
        analog.switchInput = EditorGUILayout.Toggle("Switch (binary)", analog.switchInput);
        analog.mode = (AnalogInteractable.Mode)EditorGUILayout.EnumPopup("Mode", analog.mode);
        switch (analog.mode)
        {
            case AnalogInteractable.Mode.Push:
                analog.pushOn = EditorGUILayout.Vector3Field("Pushed Offset", analog.pushOn); break;
            case AnalogInteractable.Mode.Lever:
                analog.angleOn = EditorGUILayout.Vector3Field("Switched On Rotation", analog.angleOn); break;
            case AnalogInteractable.Mode.Knob:
                analog.knobAngle = EditorGUILayout.FloatField("Knob Angle", analog.knobAngle); break;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(analog);
            EditorSceneManager.MarkSceneDirty(analog.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif