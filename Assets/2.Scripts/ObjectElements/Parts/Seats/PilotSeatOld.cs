/*
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
    //AI
    float targetAltitude = 250f;
    float priorityMaxPitch;
    float priorityCounter = 0f;
    Vector3 priorityDirection = Vector3.zero;
    bool formationBroke;
    int dogfightAction;

    const float maxRange = 450f;
    float sprayTarget;
    const float sprayTargetCycle = 4f;
    const float formationVelocity = 4f;
    const float throttleIncrement = 0.02f;
    public override SeatInterface SeatUI() { return SeatInterface.Pilot; }

    public override int Priority() { return 3; }
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);

        if (aircraft.card.fighter) InvokeRepeating("GetTarget", Random.Range(0f, 2f), 1.5f);
        targetAltitude = transform.root.position.y;
        formationBroke = false;
    }
    public override void ResetSeat()
    {
        base.ResetSeat();
        targetAltitude = transform.root.position.y;
    }
    public override void PlayerUpdate(CrewMember crew)
    {
        base.PlayerUpdate(crew);
        AiUpdate(crew);
        return;
        Actions.PilotActions pilot = PlayerActions.instance.actions.Pilot;
        if (pilot.FirePrimaries.ReadValue<float>() > 0.1f) aircraft.FirePrimaries();
        if (pilot.FireSecondaries.ReadValue<float>() > 0.7f) aircraft.FireSecondaries();
        aircraft.SetFlaps(Mathf.RoundToInt(pilot.Flaps.ReadValue<float>()));
        aircraft.brake = pilot.Brake.ReadValue<float>();
        
        if (Input.mouseScrollDelta.y != 0f)
        {
            float thr = PlayerManager.player.aircraft.engines[0].throttleInput;
            float input = Input.mouseScrollDelta.y * throttleIncrement;
            aircraft.SetThrottle(thr + input);
            aircraft.boost = (thr == 1f && input > 0f) || aircraft.boost;
            if (aircraft.boost && input < 0f) { aircraft.boost = false; aircraft.SetThrottle(1f); }
        }
    }
    private void Update()
    {
        if(target) Debug.DrawLine(transform.position, target.transform.position, Color.red);
    }
    public override void PlayerFixed(CrewMember crew)
    {
        base.PlayerFixed(crew);
        AiFixed(crew);
        return;
        Actions actions = PlayerActions.instance.actions;
        Vector3 axis = AircraftControl.AutoTargetFlight(aircraft.transform.position + PlayerCamera.directionInput, aircraft, aircraft.transform.right, 1f, 1f);
        bool pitching = actions.Pilot.Pitch.phase == InputActionPhase.Started;
        bool yawing = actions.Pilot.Rudder.phase == InputActionPhase.Started;
        bool rolling = actions.Pilot.Roll.phase == InputActionPhase.Started;
#if MOBILE_INPUT
        pitching = yawing = rolling = true;
#endif
        //override conditions
        if (pitching)
        {
            float maxDeflection = AircraftControl.MaxDeflection(aircraft.hStab, aircraft.optiAlpha);
            axis.x = actions.Pilot.Pitch.ReadValue<float>() * maxDeflection;
        }
        if (yawing) axis.y = -actions.Pilot.Rudder.ReadValue<float>();
        if (rolling || pitching) axis.z = actions.Pilot.Roll.ReadValue<float>();

        aircraft.SetControls(axis, false);
    }

    public void RerollAction()
    {
        dogfightAction = Random.Range(0, 4);
    }
    //Behaviours ----------------------------------------------------------------------------------------------------------------------------------
    private void Engage()
    {
        AI.GeometricData targetData = new AI.GeometricData(aircraft, target);
        aircraft.SetThrottle(1f);
        if (aircraft.card.forwardGuns)
        {

            if (!formationBroke && GameManager.squadrons[aircraft.squadronId][0] != aircraft)
            {
                if (targetData.distance < 1200f)
                {
                    //BREAK FORMATION
                    Vector3 breakDir = aircraft.card.formation.breakDirections[aircraft.placeInSquad];
                    if (priorityCounter <= 0f) PrioritizeDirection(transform.TransformDirection(breakDir), 2f, 0.6f);
                    formationBroke = true;
                }
                else
                {
                    KeepFormation(GameManager.squadrons[aircraft.squadronId][0]);
                }
            }
            else if (AI.TargetInSight(transform.root, targetTr))
            {
                {
                    //Chase ennemy
                    Vector3 relativeVel = target.data.rb.velocity - rb.velocity;
                    float t = Ballistics.InterceptionTime(650f, targetData.dir, relativeVel);
                    Vector3 lead = target.data.rb.velocity * t + Vector3.up * t * t * 9.81f;
                    float gunsAngle = Vector3.Angle(targetData.dir + lead, transform.root.forward);
                    float minAngle = target.wingSpan / targetData.distance * Mathf.Rad2Deg;

                    //Set guns to the lead position
                    float leadFactor = 1f + 2f * Mathf.InverseLerp(minAngle, 20f, gunsAngle);
                    Vector3 targetPos = targetTr.position + lead * leadFactor;
                    targetPos.y = Mathf.Max(GameManager.map.HeightAtPoint(targetPos) + 120f, targetPos.y);
                    Vector3 inputs = AircraftControl.AutoTargetFlight(targetPos, aircraft, targetTr.right, 0.5f, 0.9f);
                    aircraft.SetControls(inputs,false);

                    //Fire if angle is small enough and don't fire head on
                    bool fire = gunsAngle < minAngle && targetData.distance < maxRange && !target.destroyed;
                    fire = fire && targetData.closure > -160f;
                    if (fire) {
                        aircraft.FirePrimaries();
                        aircraft.FireSecondaries();
                    }

                    if (fire)//Spray target and auto aim guns
                    {
                        sprayTarget = Mathf.Cos(Time.time * Mathf.PI * 2f / sprayTargetCycle);
                        lead += targetTr.right * sprayTarget * target.wingSpan / 3f;
                        aircraft.PointGuns(targetTr.position + lead);
                    }
                    else aircraft.PointGuns();
                    RerollAction();
                }
            }
            else
            {
                switch (dogfightAction)
                {
                    case 0:
                        //Do nothing
                        if (target.card.bomber) RerollAction();
                        else if (priorityCounter <= 0f)
                        {
                            PrioritizeDirection(transform.forward + transform.right * Random.Range(-0.5f, 0.5f), 3f, 0.3f);
                            RerollAction();
                        }
                        break;
                    case 1:
                        //Immelmann
                        if (Mathf.Abs(transform.forward.y) > 0.6f) //End of Immelmann
                        {
                            Vector3 targetFuturePos = targetTr.position + target.data.rb.velocity * 10f;
                            PrioritizeDirection(targetFuturePos - transform.position, 5f, 1f);
                            RerollAction();
                        }
                        else if (priorityCounter <= 0f) //Begin Immelmann
                        {
                            if (data.relativeAltitude.Get > data.GroundSpeed * 15f && targetTr.position.y < transform.position.y + 200f)
                            {
                                //Down Immelmann
                                PrioritizeDirection(Vector3.ProjectOnPlane(-transform.forward, Vector3.up).normalized - Vector3.up, 7f, 0.8f);
                            }
                            else if (data.GroundSpeed > 100f)
                            {
                                //Up Immelmann
                                PrioritizeDirection(Vector3.ProjectOnPlane(-transform.forward, Vector3.up).normalized + Vector3.up, 9f, 0.9f);
                            }

                            else
                            {
                                //Can't Immelmann
                                RerollAction();
                            }
                        }
                        break;
                    case 2:
                        //Side Turn
                        if (priorityCounter <= 0f)
                        {
                            Vector3 targetFuturePos = targetTr.position + target.data.rb.velocity * 10f;
                            Vector3 targetFutureDir = targetFuturePos - transform.root.position + data.rb.velocity * 5f;
                            targetFutureDir = Vector3.ProjectOnPlane(targetFutureDir, Vector3.up);
                            targetFutureDir = Vector3.ProjectOnPlane(targetFutureDir, transform.root.forward);
                            PrioritizeDirection(targetFutureDir.normalized, 5f, 0.7f);
                            RerollAction();
                        }
                        break;
                    case 3:
                        //Evade
                        if (targetData.distance > 1000f || targetData.closure > 100f) RerollAction();
                        if (priorityCounter <= 0f)
                        {
                            PrioritizeDirection(transform.forward + Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized * Mathf.Sqrt(Random.Range(-1f, 1f)), 2f, 0.9f);
                        }
                        if (target.data.GroundSpeed < data.GroundSpeed)
                        {
                            aircraft.SetThrottle(0.3f);
                        }
                        if (transform.InverseTransformDirection(targetData.dir).z > 0f) priorityCounter = 0f;
                        break;
                }
            }
        }
        else // Use the turret (Defiant Only for now)
        {
            if (target.card.fighter) //Turn in circle
            {
                aircraft.SetControls(AircraftControl.AutoTargetFlight(targetTr.position, aircraft, targetTr.right, 0.5f, 0.9f),false);
                aircraft.controlTarget.x = Mathf.Clamp(aircraft.controlTarget.x, -0.2f, 0.2f);
            }
            else // Pass below the target
            {
                aircraft.SetControls(AircraftControl.AutoTargetFlight(targetTr.TransformPoint(0f, -200f, 1000f), aircraft, Vector3.right, 0.2f, 0.9f),false);
                Vector3 toTarget = targetTr.position - transform.position;
                float targetZDistance = targetTr.InverseTransformDirection(toTarget).z;
                float velocityOffset = formationVelocity * 3f * Mathf.Clamp(targetZDistance / 200f, -1f, 1f);
                aircraft.SetThrottle(target.throttle * Mathf.Pow(1f + velocityOffset / aircraft.data.GroundSpeed, 5));
            }
        }
    }
    private void Patrol()
    {
        Vector3 point = transform.root.position + transform.root.forward * 500f;
        targetAltitude = Mathf.Max(GameManager.map.HeightAtPoint(point) + 50f, targetAltitude);
        point.y = targetAltitude;
        aircraft.SetControls(AircraftControl.AutoTargetFlight(point, aircraft, Vector3.right, 1f, 0.7f),false);
        aircraft.SetThrottle(0.95f);
    }
    private void KeepFormation(SofAircraft leader)
    {
        //Find the formation target point
        Vector3 squadPos = aircraft.card.formation.aircraftPositions[aircraft.placeInSquad];
        Vector3 targetPoint = leader.transform.position;
        targetPoint += Vector3.ProjectOnPlane(leader.transform.right, Vector3.up).normalized * squadPos.x * Mathf.Sign(leader.transform.up.y);
        targetPoint += Vector3.up * squadPos.y;
        targetPoint += Vector3.ProjectOnPlane(leader.transform.forward, Vector3.up).normalized * squadPos.z;

        Vector3 targetPos = targetPoint + leader.transform.forward * 320f;
        targetPos.y = Mathf.Max(GameManager.map.HeightAtPoint(targetPos) + 50f, targetPos.y);
        float maxPitch = Mathf.Max(0.7f, leader.controlTarget.x);
        aircraft.SetControls(AircraftControl.AutoTargetFlight(targetPos, aircraft, leader.transform.right, 1f, maxPitch),false);

        Vector3 toLeader = leader.transform.position - transform.position;
        float leaderZDistance = leader.transform.InverseTransformDirection(toLeader).z;
        float velocityOffset = formationVelocity * Mathf.Clamp(leaderZDistance / 200f, -1f, 1f);
        aircraft.SetThrottle(leader.throttle * Mathf.Pow(1f + velocityOffset / aircraft.data.GroundSpeed, 3));
    }
    private void PrioritizeDirection(Vector3 dir, float time, float pitch)
    {
        priorityMaxPitch = pitch;
        priorityCounter = time;
        priorityDirection = dir;
    }
    private void SafeFlight()
    {
        //Don't crash
        if (-data.VerticalSpeed * 7f > data.relativeAltitude.Get)
        {
            PrioritizeDirection(aircraft.transform.up, 1f, 1f);
        }
        //Don't ram into squad members
        if (GameManager.squadrons[aircraft.squadronId].Length > 1)
        {
            for (int i = 0; i < GameManager.squadrons[aircraft.squadronId].Length; i++)
            {
                if (aircraft.placeInSquad == i) continue;
                SofAircraft squadMate = GameManager.squadrons[aircraft.squadronId][i];
                if ((squadMate.transform.position - transform.position).magnitude < aircraft.card.formation.minDistance)
                {
                    Vector3 direction = transform.forward;
                    direction += (transform.InverseTransformPoint(squadMate.transform.position).y > 0f) ? -transform.up * 0.25f : transform.up * 0.25f;
                    PrioritizeDirection(direction, 0.1f, 0.5f);
                }
            }
        }

        //Don't overthrottle
        if (data.InAirSpeed > aircraft.maxSpeed * 0.7f && transform.forward.y < 0.2f) aircraft.SetThrottle(Mathf.InverseLerp(aircraft.maxSpeed * 0.9f, aircraft.maxSpeed * 0.7f, data.InAirSpeed));
        if (data.InAirSpeed > aircraft.maxSpeed * 0.8f && transform.forward.y < 0.2f) PrioritizeDirection(Vector3.up, 2f, 0.7f);
    }

    //ACTIONS END ----------------------------------------------------------------------------------------------------------------------------------
    public override void AiFixed(CrewMember crew)
    {
        base.AiFixed(crew);

        SofAircraft leader = GameManager.squadrons[aircraft.squadronId][0];
        if (aircraft.card.fighter && target)
            Engage();
        else if (aircraft.placeInSquad == 0 || leader.destroyed)
            Patrol();
        else
            KeepFormation(leader);

        SafeFlight();

        if (priorityCounter > 0f)
        {
            Vector3 targetPos = transform.position + priorityDirection.normalized * 500f;
            targetPos.y = Mathf.Max(GameManager.map.HeightAtPoint(targetPos) + 200f, targetPos.y);
            aircraft.SetControls(AircraftControl.AutoTargetFlight(targetPos, aircraft, Vector3.right, 1f, priorityMaxPitch),false);

            priorityCounter -= Time.fixedDeltaTime;
            return;
        }

        if (target) headLookDirection = targetTr.position - transform.position;
        else headLookDirection = transform.root.forward;
    }
    public void GetTarget()
    {
        spotted = visibility.Spot();
        target = TargetPicker.PickTargetPilot(aircraft, spotted, target);
        targetTr = target ? target.transform : null;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(PilotSeat))]
public class PilotSeatEditor : CrewSeatEditor
{

}
#endif
*/