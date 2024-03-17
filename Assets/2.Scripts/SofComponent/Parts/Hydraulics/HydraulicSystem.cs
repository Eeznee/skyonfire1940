using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class HydraulicSystem : SofComponent
{
    public HydraulicControl.Type control;

    [SerializeField] private string animParameter;
    [SerializeField] private bool binary = true;
    [SerializeField] private float loweringTime = 3f;
    [SerializeField] private float retractingTime = 3f;
    [SerializeField] private float defaultState = 0f;
    [SerializeField] public SofModule[] essentialParts;

    public bool disabled { get; private set; }
    public float state { get; private set; }
    public float stateInput { get; private set; }
    private float previousState;
    private bool animating;

    private Animator anim;

    public bool IsEssentialPart(GameObject gameObject)
    {
        foreach (SofModule module in essentialParts)
            if (module && module.gameObject == gameObject) return true;

        return false;
    }

    public override void SetReferences(SofComplex _complex)
    {
        base.SetReferences(_complex);
        anim = GetComponentInParent<Animator>();
    }
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        if (HydraulicControl.IsAlwaysBinary(control)) binary = true;
        if (control != HydraulicControl.Type.Custom)
        {
            animParameter = HydraulicControl.GetParameter(control);
            defaultState = 0f;
        }

        sofAudio = new SofAudio(complex.avm, clip, SofAudioGroup.Persistent, false);
        sofAudio.source.pitch = pitch;

        SetInstant(defaultState);
    }
    public virtual void Set() { Set((stateInput == 0f) ? 1f : 0f); }
    public virtual void Set(bool s) { Set(s ? 1f : 0f); }
    public virtual void Set(float input) { stateInput = Mathf.Clamp01(input); }
    public virtual string GetLog(string hydraulicsName, string deploying, string retracting, string damaged)
    {
        if (disabled) return hydraulicsName + " " + damaged;

        string txt = hydraulicsName + " : ";

        if (binary) txt += stateInput == 1f ? deploying : retracting;
        else txt += (state * 100f).ToString("0") + " %";

        return txt;
    }
    public virtual void SetDirection(int speed) { if (speed == 1) stateInput = 1f; if (speed == -1) stateInput = 0f; if (speed == 0 && !binary) stateInput = state; }
    public virtual void SetInstant(bool lowered) { SetInstant(lowered ? 1f : 0f); }
    public virtual void SetInstant(float input) { state = stateInput = previousState = Mathf.Clamp01(input); anim.SetFloat(animParameter, state); }
    private void Update()
    {
        disabled = essentialParts.Length > 0;
        foreach (SofModule p in essentialParts) if (p && p.data == data && !p.ripped) disabled = false;

        animating = (state != stateInput) && !disabled;

        AnimateUpdate();
        AudioUpdate();
    }
    private void AnimateUpdate()
    {
        if (!animating) return;

        state = Mathf.MoveTowards(state, stateInput, Time.deltaTime / (stateInput > state ? loweringTime : retractingTime));
        if (state != previousState) anim.SetFloat(animParameter, state);
        previousState = state;
    }

    //AUDIO SECTION
    private SofAudio sofAudio;
    [SerializeField] private AudioClip clip;
    [SerializeField] private AudioClip extendedLockClip;
    [SerializeField] private AudioClip retractedLockClip;
    [SerializeField] private bool extendOnly = false;
    [SerializeField] private float volume = 0.3f;
    [SerializeField] private float pitch = 1f;
    private void AudioUpdate()
    {
        if (!sofAudio.Enabled()) return;
        bool play = animating && !(extendOnly && stateInput < state);
        if (play != sofAudio.source.isPlaying)
        {
            if (play) sofAudio.source.Play();
            else if (sofAudio.source.volume <= 0f) sofAudio.source.Stop();
        }

        sofAudio.source.volume = Mathf.MoveTowards(sofAudio.source.volume, play ? volume : 0f, Time.deltaTime * 5f);

        if (extendedLockClip && animating && state == 1f) avm.persistent.local.PlayOneShot(extendedLockClip, 1f);
        if (retractedLockClip && animating && state == 0f) avm.persistent.local.PlayOneShot(retractedLockClip, 1f);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(HydraulicSystem)), CanEditMultipleObjects]
public class HydraulicSystemEditor : Editor
{
    static bool showMain = true;
    SerializedProperty control;
    SerializedProperty animParameter;
    SerializedProperty binary;
    SerializedProperty loweringTime;
    SerializedProperty retractingTime;
    SerializedProperty defaultState;
    SerializedProperty essentialParts;

    static bool showAudio = true;
    SerializedProperty clip;
    SerializedProperty extendedLockClip;
    SerializedProperty retractedLockClip;
    SerializedProperty extendOnly;
    SerializedProperty volume;
    SerializedProperty pitch;
    void OnEnable()
    {
        control = serializedObject.FindProperty("control");
        animParameter = serializedObject.FindProperty("animParameter");
        binary = serializedObject.FindProperty("binary");
        loweringTime = serializedObject.FindProperty("loweringTime");
        retractingTime = serializedObject.FindProperty("retractingTime");
        defaultState = serializedObject.FindProperty("defaultState");
        essentialParts = serializedObject.FindProperty("essentialParts");

        clip = serializedObject.FindProperty("clip");
        extendedLockClip = serializedObject.FindProperty("extendedLockClip");
        retractedLockClip = serializedObject.FindProperty("retractedLockClip");
        extendOnly = serializedObject.FindProperty("extendOnly");
        volume = serializedObject.FindProperty("volume");
        pitch = serializedObject.FindProperty("pitch");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        HydraulicSystem hydraulic = (HydraulicSystem)target;

        showMain = EditorGUILayout.Foldout(showMain, "Main", true, EditorStyles.foldoutHeader);
        if (showMain)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(control);

            if (!HydraulicControl.IsAlwaysBinary(hydraulic.control))
                EditorGUILayout.PropertyField(binary);

            if (hydraulic.control == HydraulicControl.Type.Custom)
            {
                EditorGUILayout.PropertyField(animParameter);
                EditorGUILayout.PropertyField(defaultState);
            }

            EditorGUILayout.PropertyField(loweringTime);
            EditorGUILayout.PropertyField(retractingTime);

            EditorGUILayout.PropertyField(essentialParts);

            EditorGUI.indentLevel--;
        }

        showAudio = EditorGUILayout.Foldout(showAudio, "Audio", true, EditorStyles.foldoutHeader);
        if (showAudio)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(clip);
            EditorGUILayout.PropertyField(extendedLockClip);
            EditorGUILayout.PropertyField(retractedLockClip);
            EditorGUILayout.PropertyField(extendOnly);
            EditorGUILayout.PropertyField(volume);
            EditorGUILayout.PropertyField(pitch);

            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
