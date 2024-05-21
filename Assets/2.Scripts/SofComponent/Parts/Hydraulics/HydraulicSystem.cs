using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;



public class HydraulicSystem : SofComponent
{
    public HydraulicControl.Type control;

    public string animParameter;

    [SerializeField] protected bool binary = true;
    [SerializeField] protected float loweringTime = 3f;
    [SerializeField] protected float retractingTime = 3f;
    [SerializeField] protected float defaultState = 0f;
    [SerializeField] public SofModule[] essentialParts;

    public bool disabled { get; private set; }
    public float state { get; private set; }
    public float stateInput { get; private set; }

    protected bool animating;

    public bool IsDisabled()
    {
        if (essentialParts.Length == 0) return false;

        foreach (SofModule p in essentialParts) if (p && p.data == data && !p.ripped) return false;

        return true;
    }
    public bool IsEssentialPart(GameObject gameObject)
    {
        foreach (SofModule module in essentialParts)
        {
            if (module && module.gameObject == gameObject) return true;
            if (module && module.transform == gameObject.transform.parent) return true;
        }
        return false;
    }
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        if (control != HydraulicControl.Type.Custom) animParameter = control.StringParameter();
        if (control.IsAlwaysBinary()) binary = true;
        if (!control.HasCustomDefaultState()) defaultState = control.DefaultState();

        sofAudio = new SofAudio(complex.avm, clip, SofAudioGroup.Persistent, false);
        sofAudio.source.pitch = pitch;

        SetInstant(defaultState);
    }
    public virtual void SetDirection(int speed)
    {
        if (speed == 1) stateInput = 1f;
        if (speed == -1) stateInput = 0f;
        if (speed == 0 && !binary) stateInput = state;
    }
    public virtual void Set() { Set((stateInput == 0f) ? 1f : 0f); }
    public virtual void Set(bool s) { Set(s ? 1f : 0f); }
    public virtual void Set(float input) { stateInput = Mathf.Clamp01(input); }
    public virtual void SetInstant(bool lowered) 
    { 
        SetInstant(lowered ? 1f : 0f); 
    }
    public virtual void SetInstant(float input) 
    { 
        stateInput = state = Mathf.Clamp01(input);
        ApplyStateAnimator();
    }
    private void Update()
    {
        AnimateUpdate();
        AudioUpdate();
    }
    protected virtual void AnimateUpdate()
    {
        disabled = IsDisabled();
        animating = (state != stateInput) && !disabled;
        if (!animating) return;

        float travel = Time.deltaTime / (stateInput > state ? loweringTime : retractingTime);
        state = Mathf.MoveTowards(state, stateInput, travel);
        ApplyStateAnimator();
    }
    protected virtual void ApplyStateAnimator()
    {
        animator.SetFloat(animParameter, state);
    }

    public virtual string GetLog(string hydraulicsName, string deploying, string retracting, string damaged)
    {
        if (disabled) return hydraulicsName + " " + damaged;

        string txt = hydraulicsName + " : ";

        if (binary) txt += stateInput == 1f ? deploying : retracting;
        else txt += (state * 100f).ToString("0") + " %";

        return txt;
    }

    //AUDIO SECTION
    private SofAudio sofAudio;
    [SerializeField] private AudioClip clip;
    [SerializeField] private AudioClip extendedLockClip;
    [SerializeField] private AudioClip retractedLockClip;
    [SerializeField] private bool extendOnly = false;
    [SerializeField] private float volume = 0.3f;
    [SerializeField] private float pitch = 1f;
    protected void AudioUpdate()
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
