using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class HydraulicSystem : ObjectElement
{

    //Hydraulics settings
    public string animParameter;
    public bool binary = true;
    public float loweringTime = 3f;
    public float retractingTime = 3f;
    public float defaultState = 0f;

    //Audio Settings
    public AudioClip clip;
    public AudioClip extendedLockClip;
    public AudioClip retractedLockClip;
    public bool extendOnly = false;
    public float volume = 0.3f;
    public float pitch = 1f;

    public bool disabled;
    public float state;
    public float stateInput;
    private float previousState;

    private float targetVolume;

    //References
    public Module[] essentialParts;
    private Animator anim;
    private SofAudio sofAudio;

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);

        if (firstTime)
        {
            anim = GetComponentInParent<Animator>();
            sofAudio = new SofAudio(sofObject.avm, clip, SofAudioGroup.Persistent, false);
            sofAudio.source.pitch = pitch;
            SetInstant(defaultState);
        }
    }


    private void Update()
    {
        disabled = essentialParts.Length > 0;
        foreach (Module p in essentialParts) if (p && p.data == data && !p.ripped) disabled = false;

        bool animating = (state != stateInput) && !disabled;
        if (animating)
        {
            state = Mathf.MoveTowards(state, stateInput, Time.deltaTime / (stateInput > state ? loweringTime : retractingTime));
            if (state != previousState) anim.SetFloat(animParameter, state);
        }
        if (sofAudio.Enabled())
        {
            bool play = animating && !(extendOnly && stateInput < state);
            if (play != sofAudio.source.isPlaying)
            {
                if (play) sofAudio.source.Play();
                else if (sofAudio.source.volume <= 0f) sofAudio.source.Stop();
            }

            targetVolume = play ? volume : 0f;
            sofAudio.source.volume = Mathf.MoveTowards(sofAudio.source.volume, targetVolume, Time.deltaTime * 5f);

            if (extendedLockClip && state != previousState && state == 1f) sofObject.avm.persistent.local.PlayOneShot(extendedLockClip, 1f);
            if (retractedLockClip && state != previousState && state == 0f) sofObject.avm.persistent.local.PlayOneShot(retractedLockClip, 1f);
        }
        previousState = state;
    }
    public virtual void Set()
    {
        Set((stateInput == 0f) ? 1f : 0f);
    }
    public virtual void Set(bool s)
    {
        Set(s ? 1f : 0f);
    }
    public virtual void Set(float input)
    {
        stateInput = Mathf.Clamp01(input);
    }
    public virtual void SetDirection(int speed)
    {
        if (speed == 1) stateInput = 1f;
        if (speed == -1) stateInput = 0f;
        if (speed == 0 && !binary) stateInput = state;
    }
    public virtual void SetInstant(bool lowered)
    {
        SetInstant(lowered ? 1f : 0f);
    }
    public virtual void SetInstant(float input)
    {
        state = stateInput = previousState = Mathf.Clamp01(input);
        anim.SetFloat(animParameter, state);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(HydraulicSystem))]
public class HydraulicsEditor : Editor
{

    public override void OnInspectorGUI()
    {
        HydraulicSystem hydraulics = (HydraulicSystem)target;
        GUILayout.Space(10f);
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Hydraulics Settings", MessageType.None);
        GUI.color = GUI.backgroundColor;
        hydraulics.animParameter = EditorGUILayout.TextField("Animation parameter name", hydraulics.animParameter);
        hydraulics.binary = EditorGUILayout.Toggle("Binary", hydraulics.binary);
        hydraulics.loweringTime = EditorGUILayout.FloatField("Lowering Time", hydraulics.loweringTime);
        hydraulics.retractingTime = EditorGUILayout.FloatField("Retracting Time", hydraulics.retractingTime);
        hydraulics.defaultState = EditorGUILayout.Slider("Default state", hydraulics.defaultState, 0f, 1f);
        SerializedProperty essentialParts = serializedObject.FindProperty("essentialParts");
        EditorGUILayout.PropertyField(essentialParts, true);
        GUILayout.Space(30f);
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Audio Settings", MessageType.None);
        GUI.color = GUI.backgroundColor;
        hydraulics.clip = EditorGUILayout.ObjectField("Audio Clip", hydraulics.clip, typeof(AudioClip),false) as AudioClip;
        hydraulics.extendedLockClip = EditorGUILayout.ObjectField("Extended Lock Clip", hydraulics.extendedLockClip, typeof(AudioClip), false) as AudioClip;
        hydraulics.retractedLockClip = EditorGUILayout.ObjectField("Retracted Lock Clip", hydraulics.retractedLockClip, typeof(AudioClip), false) as AudioClip;
        hydraulics.extendOnly = EditorGUILayout.Toggle("Extending Only", hydraulics.extendOnly);
        hydraulics.volume = EditorGUILayout.FloatField("Volume", hydraulics.volume);
        hydraulics.pitch = EditorGUILayout.FloatField("Pitch", hydraulics.pitch);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(hydraulics);
            EditorSceneManager.MarkSceneDirty(hydraulics.gameObject.scene);
        }
    }
}
#endif