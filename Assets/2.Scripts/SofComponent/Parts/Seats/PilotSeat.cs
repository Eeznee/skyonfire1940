using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

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

    public override int Priority => 3;
    public override void Initialize(SofComplex _complex)
    {
        if (!zoomedPOV) zoomedPOV = defaultPOV;

        base.Initialize(_complex);
        if (aircraft.card.fighter) InvokeRepeating("GetTarget", Random.Range(0f, 1.5f), 1.5f);
        bfmCounter = 0f;
        brokeFormation = false;
        difficulty = aircraft.difficulty;
        library = new ManeuversLibrary(1f, !aircraft.card.forwardGuns);
        defaultManeuver = new DefaultManeuver();
        holdFormation = new HoldFormation();
        breakFormation = new BreakFormation();
        perlinRandomizer = Random.Range(0f, 1000f);
    }
    const float maxAngleInvert = 1f / 45f;

    public override Vector3 ZoomedHeadPosition => Vector3.Lerp(defaultPOV.position, zoomedPOV.position, 1f - Vector3.Angle(SofCamera.tr.forward, data.forward.Get) * maxAngleInvert);
    public override Vector3 CrosshairPosition => zoomedPOV.position + data.forward.Get * (aircraft ? aircraft.convergeance : 300f);
    public override void PlayerUpdate(CrewMember crew)
    {
        base.PlayerUpdate(crew);
        AircraftControl.PlayerUpdate(aircraft);
    }
    public override void PlayerFixed(CrewMember crew)
    {
        base.PlayerFixed(crew);
        AircraftControl.PlayerFixed(aircraft);
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
        aircraft.engines.SetThrottle(1f); //Throttle set to 1 by default, some maneuver may change it
        aircraft.engines.boost = false;

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
        aircraft.engines.SetThrottle(Mathf.Min(aircraft.engines.throttle, Mathf.InverseLerp(aircraft.maxSpeed * 0.9f, aircraft.maxSpeed * 0.7f, data.ias.Get)));
        if (aircraft.engines.throttle == 1f && difficulty > 0.9f) aircraft.engines.boost = true;
        if (maneuver == null || !maneuver.MaxPitch())
        {
            float pitch = aircraft.inputs.target.pitch;

            pitch = Mathf.Min(pitch, Mathf.Lerp(0.7f, 1f, difficulty));
            pitch *= crew.humanBody.Stamina();

            aircraft.inputs.target.pitch = pitch;
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
        foreach (SofAircraft a in GameManager.squadrons[aircraft.squadronId])
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
            AI.GeometricData bfmData = new AI.GeometricData(aircraft, target);
            state = bfmData.state.ToString();

            Maneuver(bfmData);
            PostManeuver(crew);
            aircraft.engines.TurnOnAllEngines();
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
    protected override void HeadPositionsGUI()
    {
        base.HeadPositionsGUI();

        EditorGUILayout.PropertyField(zoomedPov, new GUIContent("Gunsight Pos"));
    }
}
#endif