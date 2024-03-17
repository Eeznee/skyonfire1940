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

    public override SeatInterface SeatUI() { return SeatInterface.Pilot; }

    public override int Priority() { return 3; }
    public override void Initialize(SofComplex _complex)
    {
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
    public override Vector3 CrosshairDirection()
    {
        if (aircraft) return data.forward.Get * aircraft.convergeance;
        return data.forward.Get * 500f;
    }
    const float maxAngleInvert = 1f / 45f;
    public override Vector3 HeadPosition(bool player)
    {
        Vector3 pov = defaultPOV.position;
        if (player && CameraInputs.zoomed)
        {
            float lerp = 1f - Vector3.Angle(SofCamera.tr.forward, data.forward.Get) * maxAngleInvert;
            pov = Vector3.Lerp(pov, zoomedPOV.position,lerp);
        }
        return pov;
    }
    public override void PlayerUpdate(CrewMember crew)
    {
        base.PlayerUpdate(crew);
        AircraftControl.PlayerUpdate(aircraft);
        aircraft.PointGuns(Vector3.zero,0f);
    }
    public override void PlayerFixed(CrewMember crew)
    {
        base.PlayerFixed(crew);
        AircraftControl.PlayerFixed(aircraft);
    }
    public override string Action()
    {
        if (aircraft.card.bomber) return "Formation Flight";
        if (maneuver == null)
        {
            if (aircraft.CanPairUp()) return "Follow Pair";
            return "Default " + state;
        }
        else
        {
            return maneuver.Label();
        }
    }
    private void Maneuver(AI.GeometricData bfmData)
    {
        aircraft.SetThrottle(1f); //Throttle set to 1 by default, some maneuver may change it
        aircraft.boost = false;

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
        aircraft.SetThrottle(Mathf.Min(aircraft.throttle, Mathf.InverseLerp(aircraft.maxSpeed * 0.9f, aircraft.maxSpeed * 0.7f, data.ias.Get)));
        if (aircraft.throttle == 1f && difficulty > 0.9f) aircraft.boost = true;
        if (maneuver == null || !maneuver.MaxPitch())
        {
            aircraft.controlTarget.x = Mathf.Min(aircraft.controlTarget.x, Mathf.Lerp(0.7f, 1f, difficulty));
            aircraft.controlTarget.x = Mathf.Min(aircraft.controlTarget.x, crew.humanBody.Stamina());
        }
    }
    private void Shooting(AI.GeometricData bfmData)
    {
        Vector3 relativeVel = target.data.rb.velocity - rb.velocity;
        float t = Ballistics.InterceptionTime(aircraft.primaries[0].gunPreset.ammunition.defaultMuzzleVel * 0.85f, bfmData.dir, relativeVel);
        Vector3 lead = target.data.rb.velocity * t + -Physics.gravity * t * t * 0.5f;
        lead *= Mathf.Lerp(difficulty*0.7f, 3f - 1.7f * difficulty, Mathf.PerlinNoise(perlinRandomizer, Time.time / 3f));

        float gunsAngle = Vector3.Angle(bfmData.dir + lead, transform.root.forward);
        float minAngle = target.stats.wingSpan / bfmData.distance * (1f + difficulty * 0.5f) * Mathf.Rad2Deg;
        float trueRange = Mathf.Lerp(400f, 600f, difficulty);

        //Fire if angle is small enough and don't fire head on
        bool fire = gunsAngle < minAngle && bfmData.distance < trueRange && !target.destroyed && bfmData.closure > -160f;
        fire &= Mathf.PerlinNoise(perlinRandomizer, Time.time) < Mathf.Lerp(burstPerlinNoob, burstPerlinExpert, difficulty);
        if (fire)//Spray target and auto aim guns
        {
            float sprayTarget = Mathf.PingPong(Time.time / sprayTargetCycle * 2f, 2f) - 1f;
            lead += targetTr.right * sprayTarget * target.stats.wingSpan / 3f;

            aircraft.PointGuns(targetTr.position + lead, Mathf.PerlinNoise(perlinRandomizer * 2f, Time.time / 3f) + 0.2f + difficulty);
            foreach (SofAircraft a in GameManager.squadrons[aircraft.squadronId])
            {
                if (aircraft == a) continue;
                Vector3 localPos = aircraft.gunsPointer.InverseTransformPoint(a.transform.position);
                if (Mathf.Abs(localPos.y) < a.stats.wingSpan / 2f && Mathf.Abs(localPos.x) < a.stats.wingSpan && localPos.z > 0f) return;
            }

            aircraft.FirePrimaries();
            aircraft.FireSecondaries();
        }
    }
    private void Mechanics()
    {
        foreach (Engine e in aircraft.engines) if (!e.Working() && e.Functional()) e.Set(true, false);
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
            Mechanics();
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

}
#endif