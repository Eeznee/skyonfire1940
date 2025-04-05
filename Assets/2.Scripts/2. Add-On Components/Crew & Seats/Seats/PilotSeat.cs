using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Crew Seats/Pilot Seat")]
public class PilotSeat : CrewSeat
{
    public Transform zoomedPOV;

    ManeuversLibrary library;
    DefaultManeuver defaultManeuver;
    ActiveManeuver maneuver;
    HoldFormation holdFormation;
    BreakFormation breakFormation;

    const float bfmManeuverCoolDown = 45f;
    const float bfmManeuverRefresh = 2f;
    private string state;
    private float bfmCounter;

    private bool brokeFormation = false;

    private float difficulty;

    const float sprayTargetCycle = 4f;

    private float perlinRandomizer;
    const float burstPerlinNoob = 0.42f;
    const float burstPerlinExpert = 0.6f;

    public override int Priority => 1024;
    public override void Initialize(SofComplex _complex)
    {
        if (!zoomedPOV) zoomedPOV = defaultPOV;

        base.Initialize(_complex);
        if (aircraft.card.fighter) InvokeRepeating("GetTarget", Random.Range(0f, 1.5f), 1.5f);
        bfmCounter = 0f;
        brokeFormation = false;
        difficulty = aircraft.Difficulty;
        library = new ManeuversLibrary(1f, !aircraft.card.forwardGuns);
        defaultManeuver = new DefaultManeuver();
        holdFormation = new HoldFormation();
        breakFormation = new BreakFormation();
        perlinRandomizer = Random.Range(0f, 1000f);
    }
    const float maxAngleInvert = 1f / 45f;

    public override Vector3 ZoomedHeadPosition => Vector3.Lerp(defaultPOV.position, zoomedPOV.position, 1f - Vector3.Angle(SofCamera.tr.forward,aircraft.tr.forward) * maxAngleInvert);
    public override Vector3 CrosshairPosition => zoomedPOV.position + aircraft.tr.forward * (aircraft ? aircraft.Convergence : 300f);

    const float throttleIncrement = 0.0002f;
    public override void PlayerUpdate(CrewMember crew)
    {
        base.PlayerUpdate(crew);

        Actions.PilotActions pilot = PlayerActions.pilot;
        if (pilot.FirePrimaries.ReadValue<float>() > 0.1f) aircraft.armament.FirePrimaries();
        if (pilot.FireSecondaries.ReadValue<float>() > 0.7f) aircraft.armament.FireSecondaries();
        aircraft.hydraulics.SetFlaps(Mathf.RoundToInt(pilot.Flaps.ReadValue<float>()));
        aircraft.inputs.brake = pilot.Brake.ReadValue<float>();
#if MOBILE_INPUT
        aircraft.boost = pilot.Boost.ReadValue<float>() > 0.5f;
#else
        float scrollValue = PlayerActions.general.Scroll.ReadValue<float>();
        if (scrollValue != 0f)
        {
            CompleteThrottle current = Player.aircraft.engines.Throttle;

            float currentThrottle = aircraft.engines.Throttle;
            float throttleIncrement = scrollValue * PilotSeat.throttleIncrement;

            bool maxedThrottleAndPositiveIncrement = currentThrottle >= 1f && throttleIncrement > 0f;
            bool boostedAndNegativeIncrement = aircraft.engines.Throttle.Boost && throttleIncrement < 0f;

            if (maxedThrottleAndPositiveIncrement)
                aircraft.engines.SetThrottleAllEngines(1.1f, true);

            else if (boostedAndNegativeIncrement)

                aircraft.engines.SetThrottleAllEngines(1f, false);

            else
                aircraft.engines.SetThrottleAllEngines(currentThrottle + throttleIncrement, false);
        }
#endif
    }
    public override void PlayerFixed(CrewMember crew)
    {
        base.PlayerFixed(crew);
        Actions.PilotActions actions = PlayerActions.pilot;
        AircraftAxes axes = AircraftAxes.zero;
        if (ControlsManager.CurrentMode() == ControlsMode.Tracking) //Tracking input, mouse
        {
            bool pitching = actions.Pitch.phase == InputActionPhase.Started;
            bool rolling = actions.Roll.phase == InputActionPhase.Started;
            bool yawing = actions.Rudder.phase == InputActionPhase.Performed;

            AircraftAxes forcedAxes = new AircraftAxes(float.NaN, float.NaN, float.NaN);

            if (pitching)
            {
                forcedAxes.pitch = -actions.Pitch.ReadValue<float>();
                if (PlayerPrefs.GetInt("InvertPitch", 0) == 1) axes.pitch = -axes.pitch;
            }
            if (rolling || pitching) forcedAxes.roll = actions.Roll.ReadValue<float>();
            if (yawing || pitching) forcedAxes.yaw = -actions.Rudder.ReadValue<float>();

            axes = NewPointTracking.FindOptimalControls(SofCamera.directionInput, aircraft, forcedAxes);

            aircraft.inputs.SetTargetInput(axes, PitchCorrectionMode.Raw);
        }
        else //Direct input, joystick, phone
        {
            axes.pitch = -actions.Pitch.ReadValue<float>();
            if (PlayerPrefs.GetInt("InvertPitch", 0) == 1) axes.pitch = -axes.pitch;
            axes.roll = actions.Roll.ReadValue<float>();
            axes.yaw = -actions.Rudder.ReadValue<float>();
            aircraft.inputs.SetTargetInput(axes, ControlsManager.pitchCorrectionMode);
        }
    }
    public override string Action
    {
        get
        {
            if (aircraft.card.bomber) return "Formation Flight";
            if (maneuver == null)
                return aircraft.CanPairUp() ? "Follow Pair" : "Default " + state;
            else
                return maneuver.Label();
        }
    }
    private void Maneuver(AI.GeometricData bfmData)
    {
        aircraft.engines.SetThrottleAllEngines(1f, false); //Throttle set to 1 by default, some maneuver may change it

        if (!brokeFormation) { breakFormation.Initialize(bfmData); brokeFormation = true; }
        if (!breakFormation.done) { breakFormation.Execute(bfmData); return; }


        if (maneuver != null && maneuver.done) maneuver = null;
        if (bfmCounter == 0f) //Pick new Maneuver
        {
            ActiveManeuver m = library.PickManeuver(bfmData);
            if (m != null) { maneuver = m; bfmCounter = bfmManeuverCoolDown * Random.Range(0.5f, 1.5f) * Mathf.Lerp(1f, 0.3f, difficulty); }
            else bfmCounter = bfmManeuverRefresh;
        }
        //Execute current active maneuver if available
        if (maneuver != null) maneuver.Execute(bfmData);
        else //Execute default maneuver
        {
            defaultManeuver.Execute(bfmData);
            bfmCounter = Mathf.Max(0f, bfmCounter - Time.fixedDeltaTime);
        }
    }
    private void PostManeuver(CrewMember crew)
    {
        //Post Maneuver
        float limitThrottle = Mathf.InverseLerp(aircraft.SpeedLimitMps * 0.9f, aircraft.SpeedLimitMps * 0.7f, data.ias.Get);
        float throttle = Mathf.Min(aircraft.engines.Throttle,limitThrottle);
        if (throttle >= 1f && difficulty > 0.9f) throttle = 1.1f;
        aircraft.engines.SetThrottleAllEngines(throttle, false);

        if (maneuver == null || !maneuver.MaxPitch())
        {
            float pitch = aircraft.inputs.current.pitch;

            pitch = Mathf.Min(pitch, Mathf.Lerp(0.7f, 1f, difficulty));
            pitch *= crew.forcesEffect.Stamina();

            aircraft.inputs.current.pitch = pitch;
        }
    }
    private void Shooting(AI.GeometricData bfmData)
    {
        if (target.destroyed) return;
        if (bfmData.closure < -160f) return;

        float trueRange = Mathf.Lerp(400f, 600f, difficulty);
        if (bfmData.distance > trueRange) return;

        Vector3 relativeVel = target.data.rb.velocity - rb.velocity;
        float t = Ballistics.InterceptionTime(aircraft.armament.primaries[0].gunPreset.ammunition.defaultMuzzleVel * 0.85f, bfmData.dir, relativeVel);

        Vector3 lead = target.data.rb.velocity * t + -Physics.gravity * t * t * 0.5f;
        lead *= Mathf.Lerp(difficulty * 0.7f, 3f - 1.7f * difficulty, Mathf.PerlinNoise(perlinRandomizer, Time.time / 3f));

        float gunsAngle = Vector3.Angle(bfmData.dir + lead, transform.root.forward);
        float minAngle = target.stats.wingSpan / bfmData.distance * (1f + difficulty * 0.5f) * Mathf.Rad2Deg;


        if (gunsAngle > minAngle) return;
        if (Mathf.PerlinNoise(perlinRandomizer, Time.time) > Mathf.Lerp(burstPerlinNoob, burstPerlinExpert, difficulty)) return;
        if (FriendlyInConeOfFire(minAngle)) return;

        float sprayTarget = Mathf.PingPong(Time.time / sprayTargetCycle * 2f, 2f) - 1f;
        Vector3 spray = targetTr.right * sprayTarget * target.stats.wingSpan / 3f;

        aircraft.armament.CheatPointGuns(targetTr.position + lead + spray, Mathf.PerlinNoise(perlinRandomizer * 2f, Time.time / 3f) + 0.2f + difficulty);
        aircraft.armament.FirePrimaries();
        aircraft.armament.FireSecondaries();
    }
    public bool FriendlyInConeOfFire(float minAngle)
    {
        foreach (SofAircraft a in GameManager.squadrons[aircraft.SquadronId])
        {
            if (aircraft == a) continue;

            Vector3 localPos = tr.InverseTransformPoint(a.tr.position);

            if (localPos.z <= 0f) continue;

            float number = Mathf.Abs(localPos.y) * 2f + Mathf.Abs(localPos.x);
            bool isInOrthographicLine = number < a.stats.wingSpan * 0.8f;
            if (isInOrthographicLine) return true;
            if (number * 2f < localPos.z) continue;

            float angle = Vector3.Angle(Vector3.forward, localPos);
            if (angle < minAngle) return true;
        }
        return false;
    }
    public override void AiFixed(CrewMember crew)
    {
        base.AiFixed(crew);
        if (target && !aircraft.card.bomber) //DOGFIGHT TIME !
        {
            AI.GeometricData bfmData = new (aircraft, target);
            state = bfmData.state.ToString();

            Maneuver(bfmData);
            PostManeuver(crew);
            if (aircraft.card.forwardGuns)
                Shooting(bfmData);
        }
        else //No target
        {
            holdFormation.Execute(aircraft);
        }
    }
    public void GetTarget()
    {
        spotted = visibility.Spot();
        SofAircraft previousTarget = target;
        target = TargetPicker.PickTargetPilot(aircraft, spotted, target, aircraft.card.forwardGuns ? 0.5f : 1f);
        if (previousTarget != target) bfmCounter = bfmManeuverCoolDown * Random.Range(0.3f, 1f) * Mathf.Lerp(1f, 0.3f, difficulty);
        targetTr = target ? target.transform : null;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(PilotSeat))]
public class PilotSeatEditor : CrewSeatEditor
{
    SerializedProperty zoomedPov;

    protected override void OnEnable()
    {
        base.OnEnable();
        zoomedPov = serializedObject.FindProperty("zoomedPOV");
    }
    protected override void CameraPositions()
    {
        base.CameraPositions();

        EditorGUILayout.PropertyField(zoomedPov, new GUIContent("Zoomed Head Pos"));
    }
}
#endif