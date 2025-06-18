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
    [System.NonSerialized] public float aiTargetAltitude;
    [System.NonSerialized] public float aiRandomizedPerlin;

    const float bfmManeuverCoolDown = 45f;
    const float bfmManeuverRefresh = 2f;
    private string state;
    private float bfmCounter;
    private AmmunitionPreset ammoReference;

    private bool brokeFormation = false;

    private float difficulty;

    const float sprayTargetCycle = 4f;

    private float perlinRandomizer;
    const float burstPerlinNoob = 0.42f;
    const float burstPerlinExpert = 0.6f;

    const float maxAngleInvert = 1f / 45f;
    public override Vector3 ZoomedHeadPosition => Vector3.Lerp(defaultPOV.position, zoomedPOV.position, 1f - Vector3.Angle(SofCamera.tr.forward, aircraft.tr.forward) * maxAngleInvert);
    public override Vector3 CrosshairPosition => zoomedPOV.position + aircraft.tr.forward * (aircraft ? aircraft.Convergence : 300f);

    public override int Priority => 1024;
    public override void Initialize(SofModular _complex)
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

        aiTargetAltitude = data.altitude.Get;
        aiRandomizedPerlin = Random.Range(-100000f, 100000f);

        if (aircraft.card.forwardGuns)
        {
            if (aircraft.armament.secondaries.Length > 0)
                ammoReference = aircraft.armament.secondaries[0].gunPreset.ammunition;
            else if (aircraft.armament.primaries.Length > 0)
                ammoReference = aircraft.armament.primaries[0].gunPreset.ammunition;
        }
    }

    public override void PlayerUpdate(CrewMember crew)
    {
        base.PlayerUpdate(crew);
        PlayerActions.PilotUpdateAxes(crew);
    }

    public void PlayerPilotAircraft(CrewMember crew)
    {
        PlayerActions.PilotFixedUpdateAxes(crew);
    }




    public override string Action
    {
        get
        {
            if (aircraft.card.bomber) return "Formation Flight";
            if (maneuver == null)
                return aircraft.CanPairUp() ? "Follow Pair" : state;
            else
                return maneuver.Label();
        }
    }
    public override void AiUpdate(CrewMember crew)
    {
        base.AiUpdate(crew);
        aiTargetAltitude = Mathf.MoveTowards(aiTargetAltitude, data.altitude.Get, Time.deltaTime * 10f);
        aiTargetAltitude = Mathf.Max(aiTargetAltitude, 700f);
    }
    public void AIPilotAircraft(CrewMember crew)
    {
        if (target && !aircraft.card.bomber) //DOGFIGHT TIME !
        {
            AI.GeometricData bfmData = new(aircraft, target);
            state = bfmData.StateString;

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
    private void Maneuver(AI.GeometricData bfmData)
    {
        aircraft.engines.SetThrottleAllEngines(1f, false); //Throttle set to 1 by default, some maneuver may change it

        if (!brokeFormation) { breakFormation.Initialize(bfmData); brokeFormation = true; }
        if (!breakFormation.done) { breakFormation.Execute(bfmData); return; }

        if (maneuver != null && maneuver.done)
        {
            maneuver = null;
            aiTargetAltitude = data.altitude.Get;
        }
        if (bfmCounter == 0f) //Pick new Maneuver
        {
            ActiveManeuver m = library.PickManeuver(bfmData);
            if (m != null) { maneuver = m; bfmCounter = bfmManeuverCoolDown * Random.Range(0.7f, 1.3f) * Mathf.Lerp(1f, 0.5f, difficulty); }
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
        float throttle = Mathf.Min(aircraft.engines.Throttle, limitThrottle);
        if (throttle >= 1f && difficulty > 0.9f) throttle = 1.1f;
        aircraft.engines.SetThrottleAllEngines(throttle, false);
    }
    const float cheatAngle = 7f;
    private void Shooting(AI.GeometricData bfmData)
    {
        if (target.Destroyed) return;
        if (bfmData.state == AI.DogfightState.HeadOn && !HeadOn.CanHeadOn(bfmData)) return;

        float t = Ballistics.PerfectTimeToTarget(this, target, ammoReference.defaultMuzzleVel, ammoReference.DragCoeff);

        Vector3 lead = (target.rb.velocity - rb.velocity) * t + t * t * 0.5f * -Physics.gravity;

        float trueRange = Mathf.Lerp(400f, 600f, difficulty);
        trueRange *= Mathf.Lerp(0.7f, 1.3f, Mathf.PerlinNoise(Time.time * 0.1f, perlinRandomizer));
        if ((bfmData.dir + lead).sqrMagnitude > trueRange * trueRange) return;

        float gunsAngle = Vector3.Angle(bfmData.dir + lead, transform.root.forward);
        //float minAngle = target.stats.wingSpan / bfmData.distance * (1.5f + difficulty * 0.5f) * Mathf.Rad2Deg;
        if (gunsAngle > cheatAngle) return;

        if (Mathf.PerlinNoise(perlinRandomizer, Time.time) > Mathf.Lerp(burstPerlinNoob, burstPerlinExpert, difficulty)) return;
        if (FriendlyInConeOfFire(2f)) return;

        float xSpray = Mathf.PerlinNoise(Time.time / sprayTargetCycle, perlinRandomizer * 8f) * 2f - 1f;
        float ySpray = Mathf.PerlinNoise(Time.time, perlinRandomizer * 6f) * 2f - 1f;
        Vector3 spray = xSpray * target.stats.wingSpan * 0.5f * targetTr.right;
        spray += ySpray * target.stats.wingSpan * 0.2f * targetTr.up;

        aircraft.armament.CheatPointGuns(targetTr.position + lead + spray);
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
            if (localPos.z > 800f) continue;

            float number = Mathf.Abs(localPos.y) * 2f + Mathf.Abs(localPos.x);
            bool isInOrthographicLine = number < a.stats.wingSpan * 0.8f;
            if (isInOrthographicLine) return true;
            if (number * 2f < localPos.z) continue;

            float angle = Vector3.Angle(Vector3.forward, localPos);
            if (angle < minAngle) return true;
        }
        return false;
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