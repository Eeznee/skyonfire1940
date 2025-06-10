using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


[AddComponentMenu("Sof Components/Crew Seats/Gunner Seat")]
public class GunnerSeat : CrewSeat
{
    public GunMount gunMount;
    public HydraulicSystem progressiveHydraulic;

    private float difficulty;
    private bool mainGun = false;
    private float maxGunRange;
    private float perlinRandomizer1;
    private float perlinRandomizer2;
    private float perlinRandomizer3;
    private float perlinRandomizer4;

    private Vector3 currentLead;
    private Quaternion lead;


    const float leadMatchNoob = 12f;
    const float leadMatchExpert = 25f;

    const float maxRangeAircraftGunner = 700f;
    const float sprayTargetCycle = 4f;
    const float burstPerlinNoob = 0.42f;
    const float burstPerlinExpert = 0.52f;

    public override Vector3 CameraUp => gunMount && !SofCamera.lookAround ? gunMount.CameraUp : base.CameraUp;
    public override Vector3 LookingDirection => gunMount ? gunMount.FiringDirection : base.LookingDirection;

    public override Vector3 DefaultHeadPosition
    {
        get
        {
            if (!gunMount) return base.DefaultHeadPosition;

            Vector3 baseToAim = gunMount.AimPosition - defaultPOV.position;
            Vector3 positionShift = Vector3.ProjectOnPlane(baseToAim, LookingDirection) * 0.3f;
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
                if (Vector3.Angle(gunMount.FiringDirection, aircraft.tr.forward) > 0.001f) return 5;
                return -1;
            }
            if (target) return 2;
            return 0;
        }
    }

    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);

        difficulty = aircraft ? aircraft.Difficulty : 1f;
        mainGun = gunMount.PilotMainGun;
        maxGunRange = gunMount.Ammunition.MaxRange * 0.9f;

        perlinRandomizer1 = Random.Range(0f, 1000f);
        perlinRandomizer2 = Random.Range(0f, 1000f);
        perlinRandomizer3 = Random.Range(0f, 1000f);
        perlinRandomizer4 = Random.Range(0f, 1000f);

        InvokeRepeating(nameof(GetTarget), Random.Range(0f, 2f), 1.5f);

        currentLead = Vector3.zero;

        sofModular.onComponentRootRemoved += CheckIfGunMountHasBeenRemoved;
    }
    private void CheckIfGunMountHasBeenRemoved(SofComponent detachedComponent)
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
        if (!sofObject) return;
        if (sofObject.destroyed) return;

        if (mainGun && Player.crew == aircraft.crew[0])
        {
            gunMount.OperateResetToDefault();
            return;
        }

        if (target)
        {
            bool firing = AiTargeting();
            gunMount.OperateTrigger(firing, true);
        }
        else if(!gunMount.AlignedWithDefaultDirection)
        {
            gunMount.OperateResetToDefault();
        }
    }

    public void GetTarget()
    {
        if (!gunMount || !sofObject || sofObject.destroyed) return;

        spotted = visibility.Spot();

        SofAircraft newTarget = TargetPicker.PickTargetGunner(gunMount, spotted, target);
        if (newTarget != target)
        {
            target = newTarget;
            targetTr = target ? target.transform : null;

            previousLead = Quaternion.identity;
            leadAccuracy = 1f;
        }
    }

    private Quaternion previousLead;
    private float leadAccuracy = 1f;

    const float accuracyGain = 0.1f;
    const float minSway = 2f;
    const float swayGain = 0.4f;


    public bool AiTargeting()
    {
        //PERFECT LEAD
        float t = Ballistics.PerfectTimeToTarget(gunMount, target, gunMount.Ammunition.defaultMuzzleVel, gunMount.Ammunition.DragCoeff);
        Vector3 gravityLead = 0.5f * t * t * -Physics.gravity;
        Vector3 velocityLead = t * target.rb.velocity;
        Vector3 perfectLead = velocityLead + gravityLead - gunMount.rb.velocity * t;

        Vector3 targetDirection = target.tr.position - gunMount.tr.position;
        Vector3 targetVel = target.rb.velocity;
        float targetDistance = (targetDirection + perfectLead).magnitude;
        float relativeTargetAngularVelocity = Vector3.Angle(targetDirection, targetDirection + targetVel);

        Quaternion perfectLeadRot = Quaternion.FromToRotation(targetDirection, targetDirection + perfectLead);

        //ACCURACY GAIN/LOSS
        leadAccuracy = Mathf.MoveTowards(leadAccuracy, 0f, leadAccuracy * accuracyGain * Time.deltaTime);
        leadAccuracy += Vector3.Angle(previousLead * Vector3.forward, perfectLeadRot * Vector3.forward);
        previousLead = perfectLeadRot;


        //INACCURATE LEAD
        float velocityInaccuracy = Mathf.PerlinNoise(perlinRandomizer1, Time.time * 0.33f) * 2f;
        velocityInaccuracy = Mathf.Lerp(1f, velocityInaccuracy, leadAccuracy);
        float gravityInaccuracy = Mathf.PerlinNoise(perlinRandomizer2, Time.time * 0.33f) * 2f * velocityInaccuracy;
        gravityInaccuracy = Mathf.Lerp(1f, gravityInaccuracy, leadAccuracy);

        Vector3 inaccurateLead = velocityInaccuracy * velocityLead + gravityInaccuracy * gravityLead - gunMount.rb.velocity * t;
        Quaternion inaccurateLeadRot = Quaternion.FromToRotation(targetDirection, targetDirection + inaccurateLead);

        //SWAY
        float perlinXSway = Mathf.PerlinNoise(perlinRandomizer3, Time.time * 0.33f) * 2f - 1f;
        float perlinYSway = Mathf.PerlinNoise(perlinRandomizer4, Time.time * 0.33f) * 2f - 1f;
        Vector2 sway = new Vector2(perlinXSway, perlinYSway);
        sway *= minSway + swayGain * relativeTargetAngularVelocity;

        Quaternion swayRot = Quaternion.Euler(sway.y, sway.x, 0f);
        
        //OPERATE GUN MOUNT
        Vector3 aimingDirection = swayRot * inaccurateLeadRot * targetDirection;
        gunMount.OperateMainTracking(aimingDirection);
        gunMount.OperateSpecialTracking(aimingDirection);

        
        //CHECK IF GUNS SHOULD BE SHOOTING
        float gunsAngle = Vector3.Angle(aimingDirection, gunMount.FiringDirection);
        float minAngle = Mathf.Max(target.stats.wingSpan / targetDistance * Mathf.Rad2Deg, 1f);

        float fuzeInaccuracy = 100f + Mathf.Abs(Vector3.Dot(gunMount.FiringDirection, target.rb.velocity));
        if (!aircraft) gunMount.SetFuze(targetDistance + Random.Range(-fuzeInaccuracy, fuzeInaccuracy));

        if (gunsAngle > minAngle || targetDistance > (aircraft ? maxRangeAircraftGunner : maxGunRange)) return false;
        return Mathf.PerlinNoise(perlinRandomizer1, Time.time) < Mathf.Lerp(burstPerlinNoob, burstPerlinExpert, difficulty);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(GunnerSeat)), CanEditMultipleObjects]
public class GunnerSeatEditor : CrewSeatEditor
{
    SerializedProperty gunMount;
    SerializedProperty progressiveHydraulic;
    SerializedProperty shipGunnerProtectedFromStun;

    protected override void OnEnable()
    {
        base.OnEnable();

        gunMount = serializedObject.FindProperty("gunMount");
        progressiveHydraulic = serializedObject.FindProperty("progressiveHydraulic");
        shipGunnerProtectedFromStun = serializedObject.FindProperty("shipGunnerProtectedFromStun");
    }
    protected override void WeaponsGUI()
    {

        GunnerSeat seat = (GunnerSeat)target;

        if (seat.GetComponentInParent<SofShip>())
        {
            EditorGUILayout.PropertyField(shipGunnerProtectedFromStun);
        }

        EditorGUILayout.PropertyField(gunMount);

        base.WeaponsGUI();

        EditorGUILayout.PropertyField(progressiveHydraulic, new GUIContent("Controlled Hydraulics"));
    }
}
#endif