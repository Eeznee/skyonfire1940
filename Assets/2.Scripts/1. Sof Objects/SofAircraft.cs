using UnityEngine;
using System.Collections.Generic;
using System;

public enum FlightPhase
{
    Landed,
    TakingOff,
    InFlight
}

public class SofAircraft : SofComplex
{
    public AircraftCard card;
    [SerializeField] private Station[] stations;

    public Station[] Stations => stations;


    [SerializeField] private float maxG = 8f;
    [SerializeField] private float speedLimitKph = 700f;
    [SerializeField] private float convergeance = 300f;

    public float SpeedLimitMps => speedLimitKph * UnitsConverter.kphToMps;
    public float MaxGForce => maxG;
    public float Convergence => convergeance;


    [SerializeField] private bool customPIDValues;
    [SerializeField] public PID pidPitch;
    [SerializeField] public PID pidRoll;
    [SerializeField] private float stickTorqueFactor = 1f;
    [SerializeField] private float stallMarginAngle = 1f;
    [SerializeField] private bool hydraulicControls = false;


    public bool CustomPIDValues => customPIDValues;
    public float StickTorqueFactor => stickTorqueFactor;
    public float StallMarginAngle => stallMarginAngle;
    public bool HydraulicControls => hydraulicControls;



    public Game.Squadron squadron;
    public int placeInSquad;



    public PilotSeat mainSeat { get; private set; }
    public BombardierSeat bombardierSeat { get; private set; }
    public Bombsight bombSight { get; private set; }
    public Animator animator { get; private set; }

    //Subsystems
    [NonSerialized] public AircraftStats stats;
    [NonSerialized] public AircraftInputs controls;
    [NonSerialized] public HydraulicsManager hydraulics;
    [NonSerialized] public FuelManager fuel;
    [NonSerialized] public ArmamentManager armament;
    [NonSerialized] public EnginesManager engines;
    public ForcesCompiler forcesCompiler { get; private set; }

    public float cruiseSpeed => stats.altitudeZeroMaxSpeed;
    public float Difficulty => squadron.difficulty;
    public int SquadronId => squadron.id;
    public bool GroundedStart => squadron.airfield >= 0;


    private float timeSinceLastLanding;
    public float TimeSinceLastLanding => timeSinceLastLanding;


    [NonSerialized] public Vector2 ptAbstractControls = Vector2.zero;
    [NonSerialized] public Vector2 ptMultipliers = Vector2.one;


    public void ResetStationsToDefault()
    {
        foreach (Station s in Stations) if (Stations != null && s != null) s.SelectAndDisactivate(0);
    }

    public override void SetReferences()
    {
        foreach (Station s in Stations) if (Stations != null && s != null) s.SelectAndDisactivate();

        bombardierSeat = GetComponentInChildren<BombardierSeat>();
        mainSeat = GetComponentInChildren<PilotSeat>();
        bombSight = GetComponentInChildren<Bombsight>();
        animator = GetComponent<Animator>();

        base.SetReferences();

        stats ??= new AircraftStats(this);
        hydraulics ??= new HydraulicsManager(this);
        armament ??= new ArmamentManager(this);
        engines ??= new EnginesManager(this);
        fuel ??= new FuelManager(this);
        controls ??= new AircraftInputs(this);
    }
    protected override void InitializeImportantComponents()
    {
        base.InitializeImportantComponents();

        forcesCompiler = gameObject.AddComponent<ForcesCompiler>();
        lod = this.GetCreateComponentInChildren<ObjectLOD>();
        if (!simpleDamage) bubble = transform.CreateChild("Object Bubble").gameObject.AddComponent<ObjectBubble>();
    }
    protected override void InitializePhysics()
    {
        base.InitializePhysics();

        if (!GroundedStart) rb.velocity = transform.forward * stats.MaxSpeed(data.altitude.Get, 1f);
        timeSinceLastLanding = GroundedStart ? 0f : 600f;
    }
    protected override void InitializeReferencesAndPlayer()
    {
        base.InitializeReferencesAndPlayer();

        tag = (squadron.team == Game.Team.Ally) ? "Ally" : (squadron.team == Game.Team.Axis) ? "Axis" : "Neutral";

        if (squadron.team == Game.Team.Ally) GameManager.allyAircrafts.Add(this);
        else GameManager.axisAircrafts.Add(this);
        MarkersManager.Add(this);
    }
    protected override void GameInitialization()
    {
        ptAbstractControls = Vector2.zero;
        ptMultipliers = Vector2.one;
        if (!customPIDValues) SetDefaultPIDValues();

        for (int i = 0; i < Stations.Length; i++) Stations[i].SelectAndDestroy(squadron.stations[i]);

        base.GameInitialization();
    }
    private void OnCollisionEnter(Collision col)
    {
        Transform tr = col.GetContact(0).thisCollider.transform;
        tr = tr.GetComponent<WingSkin>() ? tr.parent : tr;
        SofAirframe collided = tr.GetComponent<SofAirframe>();
        if (collided && col.impulse.magnitude > 10000f) collided.Rip();

    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        WaterPhysics();

        //Call the pilot seat behaviour before inputs and force compilation
        CrewMember mainPilot = mainSeat.seatedCrew;
        if (mainPilot != null && !mainPilot.ripped && mainPilot.complex == complex)
        {
            if (mainPilot == Player.crew) mainSeat.PlayerFixed(mainPilot);
            else mainSeat.AiFixed(mainPilot);
        }

        if (data.grounded.Get) timeSinceLastLanding = 0f;
        else timeSinceLastLanding += Time.fixedDeltaTime;

        controls.FixedUpdate();
        forcesCompiler.ApplyForcesOnFixedUpdate();
    }
    public void SetDefaultPIDValues()
    {
        pidPitch = new PID(new Vector3(0.03f, 0f, 0.005f));
        pidRoll = new PID(new Vector3(2f, 0f, 0.5f));
    }
}

