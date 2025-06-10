using UnityEngine;


[AddComponentMenu("Sof Components/Liquid Systems/Hydraulics")]
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


    protected void Awake()
    {
        animator = GetComponentInParent<Animator>();
        if (control != HydraulicControl.Type.Custom) animParameter = control.StringParameter();
        SetInstant(0f);
        animator.Update(100f);
        foreach (MechanicalLink linker in transform.root.GetComponentsInChildren<MechanicalLink>())
        {
            linker.PrecomputeValues();
            linker.MechanicalAnimation();
        }
    }

    public bool IsAnimated(Transform transformToCheck)
    {
        for (int i = 0; i < essentialParts.Length; i++)
        {
            if (essentialParts[i] && transformToCheck.IsChildOf(essentialParts[i].tr)) return true;
        }

        return false;
    }
    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);

        if (control.IsAlwaysBinary()) binary = true;
        if (!control.HasCustomDefaultState()) defaultState = control.DefaultState();

        sofAudio = new SofAudio(sofModular.avm, clip, SofAudioGroup.Persistent, false);
        sofAudio.source.pitch = pitch;

        if (control == HydraulicControl.Type.LandingGear) SetInstant(aircraft.GroundedStart);
        else SetInstant(defaultState);

        sofModular.onComponentRootRemoved += CheckIfDisabled;
    }
    public void CheckIfDisabled(SofComponent root)
    {
        disabled = true;
        if (essentialParts.Length == 0) disabled = false;
        foreach (SofModule p in essentialParts)
        {
            if (p == null)
            {
                continue;
            }
            if (p.sofModular == sofModular && !p.ripped) disabled = false;
        }

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

    private float lastStateUpdatedLOD = 0f;
    const float updateLODthreshold = 0.05f;
    protected virtual void AnimateUpdate()
    {
        animating = (state != stateInput) && !disabled;
        if (!animating) return;

        float travel = Time.deltaTime / (stateInput > state ? loweringTime : retractingTime);
        state = Mathf.MoveTowards(state, stateInput, travel);
        if (control == HydraulicControl.Type.LandingGear && aircraft)
        {
            aircraft.RecomputeRealMass();
        }
        if (aircraft && Mathf.Abs(lastStateUpdatedLOD - state) > updateLODthreshold || state == stateInput)
        {
            aircraft.lod.UpdateMergedModel();
            lastStateUpdatedLOD = state;
        }
        ApplyStateAnimator();
    }
    private float oneFrameStatePrevious;
    public void OneFrameSetStart(float newState)
    {
        oneFrameStatePrevious = state;
        state = Mathf.Clamp01(newState);
        ApplyStateAnimator();
        animator.Update(100f);
    }
    public void OneFrameSetEnd()
    {
        state = oneFrameStatePrevious;
        ApplyStateAnimator();
        animator.Update(100f);
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

        if (extendedLockClip && animating && state == 1f) aircraft.avm.persistent.local.PlayOneShot(extendedLockClip, 1f);
        if (retractedLockClip && animating && state == 0f) aircraft.avm.persistent.local.PlayOneShot(retractedLockClip, 1f);
    }
}
