using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class GunnerSeat : CrewSeat
{
    private bool mainGun = false;
    [FormerlySerializedAs("turret")] public GunMount gunMount;
    public HydraulicSystem progressiveHydraulic;
    private float perlinRandomizer;

    private Vector3 currentLead;
    private float difficulty;

    const float leadMatchNoob = 12f;
    const float leadMatchExpert = 25f;

    const float maxRange = 700f;
    const float maxRangeAA = 3500f;
    const float sprayTargetCycle = 4f;
    const float burstPerlinNoob = 0.42f;
    const float burstPerlinExpert = 0.52f;

    public override Vector3 CameraUp => gunMount && !SofCamera.lookAround ? gunMount.CameraUp : base.CameraUp;
    public override Vector3 LookingDirection => gunMount ? gunMount.FiringDirection : base.LookingDirection;

    public override Vector3 DefaultHeadPosition
    {
        get
        {
            if(!gunMount) return base.DefaultHeadPosition;

            Vector3 baseToAim = gunMount.AimPosition - defaultPOV.position;
            Vector3 positionShift = Vector3.ProjectOnPlane(baseToAim, LookingDirection) * 0.5f;
            return defaultPOV.position + positionShift;
        }
    }
    public override Vector3 ZoomedHeadPosition => gunMount ? gunMount.AimPosition : base.ZoomedHeadPosition;
    public override Vector3 CrosshairPosition => gunMount ? gunMount.AimPosition + gunMount.FiringDirection * 500f : base.CrosshairPosition;
    
    public override int Priority
    {
        get
        {
            if (mainGun && aircraft && aircraft.crew[0] == Player.crew)
            {
                if (Vector3.Dot(gunMount.FiringDirection, data.forward.Get) < 0.999f) return 3;
                return -1;
            }
            if (target) return 2;
            return 0;
        }
    }

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        InvokeRepeating("GetTarget", Random.Range(0f, 2f), 1.5f);
        difficulty = aircraft ? aircraft.difficulty : 1f;
        perlinRandomizer = Random.Range(0f, 1000f);
        currentLead = Vector3.zero;
        mainGun = gunMount.PilotMainGun;
    }

    private void Update()
    {
        if (gunMount && gunMount.transform.root != transform.root)
        {
            gunMount = null;
            NewGrips(null, null);
        }
    }

    public override void PlayerUpdate(CrewMember crew)
    {
        base.PlayerUpdate(crew);

        if (progressiveHydraulic)
        {
            float input = PlayerActions.gunner.Hydraulics.ReadValue<float>();
            progressiveHydraulic.SetDirection(Mathf.RoundToInt(input));
        }
        if (!gunMount) return;
        if (!handsBusy) NewGrips(defaultRightHand, defaultLeftHand);

        if (complex.bubble) complex.bubble.EnableColliders(false);

        Vector2 basic = PlayerActions.gunner.BasicAxis.ReadValue<Vector2>();
        float special = PlayerActions.gunner.SpecialAxis.ReadValue<float>();

        if (ControlsManager.CurrentMode() == ControlsMode.Tracking && !gunMount.ForceJoystickControls)
            gunMount.OperateMainTracking(SofCamera.directionInput);
        else
            gunMount.OperateMainManual(basic);

        gunMount.OperateSpecialManual(special);

        bool firing = PlayerActions.gunner.Fire.ReadValue<float>() > 0.5f;
        gunMount.OperateTrigger(firing, false);
        if (gunMount.Firing) VibrationsManager.SendVibrations(0.2f, 0.1f, aircraft);
    }
    public override void AiUpdate(CrewMember crew)
    {
        base.AiUpdate(crew);

        if (handsBusy || !gunMount) return;
        if (mainGun && Player.crew == aircraft.crew[0])
        {
            gunMount.OperateMainTracking(data.forward.Get);
            gunMount.OperateTrigger(false, false);
            return;
        }
        bool firing = false;

        if (target && sofObject && !sofObject.destroyed) firing = AiTargeting();

        gunMount.OperateTrigger(firing, true);
    }

    public bool AiTargeting()
    {
        AI.GunnerTargetingData targetData = new AI.GunnerTargetingData(gunMount, target.data.rb); //Compute Ballistic data
        float t = Ballistics.InterceptionTime(gunMount.MuzzleVelocity * 0.95f, targetData.dir, targetData.relativeVel);
        Vector3 gravityLead = -Physics.gravity * t * t * 0.5f;
        Vector3 speedLead = t * target.data.rb.velocity;
        Vector3 targetLead = speedLead + gravityLead - rb.velocity * t * 1.2f;
        targetLead *= Mathf.Lerp(-0.7f + difficulty * 1.1f, 3f - 1.35f * difficulty, Mathf.PerlinNoise(perlinRandomizer, Time.time * 0.33f));

        if (gunMount.Firing) //Spray when firing
        {
            float sprayTarget = Mathf.PingPong(Time.time * 2f / sprayTargetCycle, 2f) - 1f;
            targetLead += targetTr.right * sprayTarget * target.stats.wingSpan * 0.33f;
        }

        currentLead = Vector3.MoveTowards(currentLead, targetLead, Mathf.Lerp(leadMatchNoob, leadMatchExpert, difficulty) * Time.deltaTime);
        currentLead += Random.insideUnitSphere * targetData.distance * 0.01f * Mathf.Lerp(0.8f, 0.15f, difficulty);

        Vector3 targetDir = targetTr.position - transform.position + currentLead;
        gunMount.OperateMainTracking(targetDir);
        gunMount.OperateSpecialTracking(targetDir);

        float gunsAngle = Vector3.Angle(targetDir, gunMount.FiringDirection);
        float minAngle = Mathf.Max(target.stats.wingSpan / targetData.distance * Mathf.Rad2Deg, 1f);

        //Solution temporaire pour 40 mm
        if (!aircraft) gunMount.SetFuze(targetData.distance);

        if (gunsAngle > minAngle || targetData.distance > (aircraft ? maxRange : maxRangeAA)) return false;
        return Mathf.PerlinNoise(perlinRandomizer, Time.time) < Mathf.Lerp(burstPerlinNoob, burstPerlinExpert, difficulty);
    }
    public void GetTarget()
    {
        spotted = visibility.Spot();
        target = TargetPicker.PickTargetGunner(gunMount, spotted, target);
        targetTr = target ? target.transform : null;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(GunnerSeat)), CanEditMultipleObjects]
public class GunnerSeatEditor : CrewSeatEditor
{
    SerializedProperty gunMount;
    SerializedProperty progressiveHydraulic;

    protected override void OnEnable()
    {
        base.OnEnable();

        gunMount = serializedObject.FindProperty("gunMount");
        progressiveHydraulic = serializedObject.FindProperty("progressiveHydraulic");
    }
    protected override void WeaponsGUI()
    {
        EditorGUILayout.PropertyField(gunMount);

        base.WeaponsGUI();

        EditorGUILayout.PropertyField(progressiveHydraulic, new GUIContent("Controlled Hydraulics"));
    }
}
#endif